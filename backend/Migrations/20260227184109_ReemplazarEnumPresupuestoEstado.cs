using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ReemplazarEnumPresupuestoEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Presupuestos");

            migrationBuilder.AddColumn<int>(
                name: "IdPresupuestoEstado",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PresupuestoEstados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PresupuestoEstados", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdPresupuestoEstado",
                table: "Presupuestos",
                column: "IdPresupuestoEstado");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_PresupuestoEstados_IdPresupuestoEstado",
                table: "Presupuestos",
                column: "IdPresupuestoEstado",
                principalTable: "PresupuestoEstados",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_PresupuestoEstados_IdPresupuestoEstado",
                table: "Presupuestos");

            migrationBuilder.DropTable(
                name: "PresupuestoEstados");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdPresupuestoEstado",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "IdPresupuestoEstado",
                table: "Presupuestos");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Presupuestos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
