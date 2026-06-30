using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MindLens.Api.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MindLens API",
                Version = "v1",
                Description = "REST API for MindLens Application"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            
            options.IncludeXmlComments(xmlPath);

            ConfigureJwt(options);
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "MindLens API v1");
            options.DocumentTitle = "MindLens API";
            options.DisplayRequestDuration();
        });
        
        return app;
    }

    private static void ConfigureJwt(SwaggerGenOptions options)
    {
        const string schemeName = "Bearer";
        
        options.AddSecurityDefinition(schemeName, new OpenApiSecurityScheme 
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "Enter your JWT token"
        });
        
        options.AddSecurityRequirement(document =>
        {
            return new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeName)] = []
            };
        });
    }
}