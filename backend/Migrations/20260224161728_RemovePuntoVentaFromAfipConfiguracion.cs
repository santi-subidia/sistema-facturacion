using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovePuntoVentaFromAfipConfiguracion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PuntoVentaPorDefecto",
                table: "AfipConfiguraciones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PuntoVentaPorDefecto",
                table: "AfipConfiguraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
