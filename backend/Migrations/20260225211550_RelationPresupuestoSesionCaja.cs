using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RelationPresupuestoSesionCaja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SesionCajaId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_SesionCajaId",
                table: "Presupuestos",
                column: "SesionCajaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_SesionesCaja_SesionCajaId",
                table: "Presupuestos",
                column: "SesionCajaId",
                principalTable: "SesionesCaja",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_SesionesCaja_SesionCajaId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_SesionCajaId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "SesionCajaId",
                table: "Presupuestos");
        }
    }
}
