using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class PermitirProductosNuloEnDetalles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesFactura_Facturas_IdFactura",
                table: "DetallesFactura");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesFactura_Facturas_IdFactura",
                table: "DetallesFactura",
                column: "IdFactura",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesFactura_Facturas_IdFactura",
                table: "DetallesFactura");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesFactura_Facturas_IdFactura",
                table: "DetallesFactura",
                column: "IdFactura",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
