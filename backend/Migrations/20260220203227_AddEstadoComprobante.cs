using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdEstadoComprobante",
                table: "Comprobantes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EstadosComprobantes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosComprobantes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comprobantes_IdEstadoComprobante",
                table: "Comprobantes",
                column: "IdEstadoComprobante");

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_EstadosComprobantes_IdEstadoComprobante",
                table: "Comprobantes",
                column: "IdEstadoComprobante",
                principalTable: "EstadosComprobantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_EstadosComprobantes_IdEstadoComprobante",
                table: "Comprobantes");

            migrationBuilder.DropTable(
                name: "EstadosComprobantes");

            migrationBuilder.DropIndex(
                name: "IX_Comprobantes_IdEstadoComprobante",
                table: "Comprobantes");

            migrationBuilder.DropColumn(
                name: "IdEstadoComprobante",
                table: "Comprobantes");
        }
    }
}
