using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MindLens.Api.Configuration;
using MindLens.Api.Models;
using MindLens.Api.Data;
using MindLens.Api.Middlewares;
using MindLens.Api.Services;
using MindLens.Api.Services.Interfaces;
using MindLens.Api.Data.Seeders;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuration Setup
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSwaggerConfiguration();

// Services Setup
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();
builder.Services.AddScoped<IJournalingService, JournalingService>();

// AWS Services Setup
var awsEndpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL");
if (!string.IsNullOrEmpty(awsEndpoint))
{
    var sqsConfig = new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsEndpoint };
    builder.Services.AddSingleton<Amazon.SQS.IAmazonSQS>(new Amazon.SQS.AmazonSQSClient(sqsConfig));
    
    var s3Config = new Amazon.S3.AmazonS3Config { ServiceURL = awsEndpoint, ForcePathStyle = true };
    builder.Services.AddSingleton<Amazon.S3.IAmazonS3>(new Amazon.S3.AmazonS3Client(s3Config));
}
else
{
    builder.Services.AddAWSService<Amazon.SQS.IAmazonSQS>();
    builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
}
builder.Services.AddSingleton<IAwsHelper, AwsHelper>();

// DB Context Setup
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("SharedDB")
    );
});

// Tenant DB Context Setup
builder.Services.AddDbContext<TenantContext>((sp, options) =>
{
    var tenantService = sp.GetRequiredService<ITenantService>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    var builder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("SharedDb"));
    builder.Database = tenantService.CurrentTenant?.DatabaseName ?? "Setup";
    
    options.UseNpgsql(builder.ConnectionString, o => 
    {
        o.UseVector(); // Registering vector extension
    });
});

// Identity Setup
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // Identity User Properties Configuration
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationContext>()
.AddDefaultTokenProviders();

// JWT Setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
        
        RoleClaimType = ClaimTypes.Role
    };
    
    // Debugging
    options.Events = new JwtBearerEvents 
    {
        OnMessageReceived = context => 
        {
            Console.WriteLine($"Authorization: {context.Request.Headers.Authorization}");
            return Task.CompletedTask;
        },
        
        OnTokenValidated = context =>
        {
            Console.WriteLine("VALID TOKEN");
            return Task.CompletedTask;
        },
        
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine(context.Exception.ToString());
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();

// Controllers Setup
builder.Services.AddControllers().AddJsonOptions(options => 
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

// Seeders
using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedAsync(scope.ServiceProvider);
    await MasterDataSeeder.SeedAsync(scope.ServiceProvider);
}

// Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();

// Tenant Authentication Middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();
app.MapControllers();

app.Run();