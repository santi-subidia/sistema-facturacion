using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Destinatario = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    IdComprobante = table.Column<int>(type: "INTEGER", nullable: true),
                    IdPresupuesto = table.Column<int>(type: "INTEGER", nullable: true),
                    Intentos = table.Column<int>(type: "INTEGER", nullable: false),
                    ProximoReintento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ErrorUltimoIntento = table.Column<string>(type: "TEXT", nullable: true),
                    Exitoso = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailQueues_Comprobantes_IdComprobante",
                        column: x => x.IdComprobante,
                        principalTable: "Comprobantes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmailQueues_Presupuestos_IdPresupuesto",
                        column: x => x.IdPresupuesto,
                        principalTable: "Presupuestos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_IdComprobante",
                table: "EmailQueues",
                column: "IdComprobante");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_IdPresupuesto",
                table: "EmailQueues",
                column: "IdPresupuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailQueues");
        }
    }
}
