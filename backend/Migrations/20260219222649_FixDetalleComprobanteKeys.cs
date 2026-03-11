using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixDetalleComprobanteKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_DetallesComprobante_Productos_ProductoId",
            //    table: "DetallesComprobante");

            //migrationBuilder.DropIndex(
            //    name: "IX_DetallesComprobante_ProductoId",
            //    table: "DetallesComprobante");

            //migrationBuilder.DropColumn(
            //    name: "ProductoId",
            //    table: "DetallesComprobante");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesComprobante_IdProducto",
                table: "DetallesComprobante",
                column: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesComprobante_Productos_IdProducto",
                table: "DetallesComprobante",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesComprobante_Productos_IdProducto",
                table: "DetallesComprobante");

            migrationBuilder.DropIndex(
                name: "IX_DetallesComprobante_IdProducto",
                table: "DetallesComprobante");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "DetallesComprobante",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesComprobante_ProductoId",
                table: "DetallesComprobante",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesComprobante_Productos_ProductoId",
                table: "DetallesComprobante",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");
        }
    }
}
