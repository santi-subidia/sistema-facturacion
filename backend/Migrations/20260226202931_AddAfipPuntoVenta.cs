using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAfipPuntoVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AfipPuntosVenta",
                columns: table => new
                {
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    EmisionTipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Bloqueado = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    FechaBaja = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfipPuntosVenta", x => x.Numero);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cajas_PuntoVenta",
                table: "Cajas",
                column: "PuntoVenta");

            migrationBuilder.AddForeignKey(
                name: "FK_Cajas_AfipPuntosVenta_PuntoVenta",
                table: "Cajas",
                column: "PuntoVenta",
                principalTable: "AfipPuntosVenta",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_AfipPuntosVenta_PuntoVenta",
                table: "Comprobantes",
                column: "PuntoVenta",
                principalTable: "AfipPuntosVenta",
                principalColumn: "Numero",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cajas_AfipPuntosVenta_PuntoVenta",
                table: "Cajas");

            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_AfipPuntosVenta_PuntoVenta",
                table: "Comprobantes");

            migrationBuilder.DropTable(
                name: "AfipPuntosVenta");

            migrationBuilder.DropIndex(
                name: "IX_Cajas_PuntoVenta",
                table: "Cajas");
        }
    }
}
