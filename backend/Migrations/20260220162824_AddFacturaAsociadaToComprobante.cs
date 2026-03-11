using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaAsociadaToComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdFacturaAsociada",
                table: "Comprobantes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_IdFacturaAsociada",
                table: "Comprobantes",
                column: "IdFacturaAsociada");

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_Comprobantes_IdFacturaAsociada",
                table: "Comprobantes",
                column: "IdFacturaAsociada",
                principalTable: "Comprobantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_Comprobantes_IdFacturaAsociada",
                table: "Comprobantes");

            migrationBuilder.DropIndex(
                name: "IX_Comprobantes_IdFacturaAsociada",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "IdFacturaAsociada",
                table: "Comprobantes");
        }
    }
}
