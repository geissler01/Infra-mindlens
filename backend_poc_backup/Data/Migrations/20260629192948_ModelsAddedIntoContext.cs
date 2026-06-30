using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MindLens.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelsAddedIntoContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TreatmentRegistry",
                table: "TreatmentRegistry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PsychologistApplication",
                table: "PsychologistApplication");

            migrationBuilder.RenameTable(
                name: "TreatmentRegistry",
                newName: "TreatmentsRegistry");

            migrationBuilder.RenameTable(
                name: "PsychologistApplication",
                newName: "PsychologistApplications");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreatmentsRegistry",
                table: "TreatmentsRegistry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PsychologistApplications",
                table: "PsychologistApplications",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TreatmentsRegistry",
                table: "TreatmentsRegistry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PsychologistApplications",
                table: "PsychologistApplications");

            migrationBuilder.RenameTable(
                name: "TreatmentsRegistry",
                newName: "TreatmentRegistry");

            migrationBuilder.RenameTable(
                name: "PsychologistApplications",
                newName: "PsychologistApplication");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TreatmentRegistry",
                table: "TreatmentRegistry",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PsychologistApplication",
                table: "PsychologistApplication",
                column: "Id");
        }
    }
}
