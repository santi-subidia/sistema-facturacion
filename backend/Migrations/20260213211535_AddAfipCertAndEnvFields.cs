using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAfipCertAndEnvFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificadoNombre",
                table: "AfipConfiguraciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificadoPassword",
                table: "AfipConfiguraciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsProduccion",
                table: "AfipConfiguraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificadoNombre",
                table: "AfipConfiguraciones");

            migrationBuilder.DropColumn(
                name: "CertificadoPassword",
                table: "AfipConfiguraciones");

            migrationBuilder.DropColumn(
                name: "EsProduccion",
                table: "AfipConfiguraciones");
        }
    }
}
