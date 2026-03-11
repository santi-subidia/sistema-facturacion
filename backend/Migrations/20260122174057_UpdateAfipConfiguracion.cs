using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAfipConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaMonotributo",
                table: "AfipConfiguraciones");

            migrationBuilder.DropColumn(
                name: "TieneFacturaElectronica",
                table: "AfipConfiguraciones");

            migrationBuilder.RenameColumn(
                name: "LimiteMontoFactura",
                table: "AfipConfiguraciones",
                newName: "LimiteMontoConsumidorFinal");

            migrationBuilder.AddColumn<string>(
                name: "NombreFantasia",
                table: "AfipConfiguraciones",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NombreFantasia",
                table: "AfipConfiguraciones");

            migrationBuilder.RenameColumn(
                name: "LimiteMontoConsumidorFinal",
                table: "AfipConfiguraciones",
                newName: "LimiteMontoFactura");

            migrationBuilder.AddColumn<string>(
                name: "CategoriaMonotributo",
                table: "AfipConfiguraciones",
                type: "TEXT",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneFacturaElectronica",
                table: "AfipConfiguraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
