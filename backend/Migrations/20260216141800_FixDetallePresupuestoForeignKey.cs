using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixDetallePresupuestoForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Limpiar datos existentes para evitar conflictos de foreign key
            migrationBuilder.Sql("DELETE FROM DetallesPresupuesto");
            migrationBuilder.Sql("DELETE FROM Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPresupuesto_Presupuestos_PresupuestoId",
                table: "DetallesPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPresupuesto_Productos_ProductoId",
                table: "DetallesPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteId",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_CondicionesVenta_CondicionVentaId",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Facturas_FacturaGeneradaId",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_FormasPago_FormaPagoId",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Usuarios_Creado_porId",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Usuarios_Eliminado_porId",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Usuarios_UsuarioDescontoStockId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_ClienteId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_CondicionVentaId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_Creado_porId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_Eliminado_porId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_FacturaGeneradaId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_FormaPagoId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_UsuarioDescontoStockId",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPresupuesto_PresupuestoId",
                table: "DetallesPresupuesto");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPresupuesto_ProductoId",
                table: "DetallesPresupuesto");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "CondicionVentaId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "Creado_porId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "Eliminado_porId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "FacturaGeneradaId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "FormaPagoId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "UsuarioDescontoStockId",
                table: "Presupuestos");

            migrationBuilder.DropColumn(
                name: "PresupuestoId",
                table: "DetallesPresupuesto");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "DetallesPresupuesto");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Presupuestos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdCliente",
                table: "Presupuestos",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdCondicionVenta",
                table: "Presupuestos",
                column: "IdCondicionVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdCreado_por",
                table: "Presupuestos",
                column: "IdCreado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdEliminado_por",
                table: "Presupuestos",
                column: "IdEliminado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdFacturaGenerada",
                table: "Presupuestos",
                column: "IdFacturaGenerada");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdFormaPago",
                table: "Presupuestos",
                column: "IdFormaPago");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_IdUsuarioDescontoStock",
                table: "Presupuestos",
                column: "IdUsuarioDescontoStock");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPresupuesto_IdPresupuesto",
                table: "DetallesPresupuesto",
                column: "IdPresupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPresupuesto_IdProducto",
                table: "DetallesPresupuesto",
                column: "IdProducto");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPresupuesto_Presupuestos_IdPresupuesto",
                table: "DetallesPresupuesto",
                column: "IdPresupuesto",
                principalTable: "Presupuestos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPresupuesto_Productos_IdProducto",
                table: "DetallesPresupuesto",
                column: "IdProducto",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Clientes_IdCliente",
                table: "Presupuestos",
                column: "IdCliente",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_CondicionesVenta_IdCondicionVenta",
                table: "Presupuestos",
                column: "IdCondicionVenta",
                principalTable: "CondicionesVenta",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Facturas_IdFacturaGenerada",
                table: "Presupuestos",
                column: "IdFacturaGenerada",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_FormasPago_IdFormaPago",
                table: "Presupuestos",
                column: "IdFormaPago",
                principalTable: "FormasPago",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Usuarios_IdCreado_por",
                table: "Presupuestos",
                column: "IdCreado_por",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Usuarios_IdEliminado_por",
                table: "Presupuestos",
                column: "IdEliminado_por",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Usuarios_IdUsuarioDescontoStock",
                table: "Presupuestos",
                column: "IdUsuarioDescontoStock",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPresupuesto_Presupuestos_IdPresupuesto",
                table: "DetallesPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_DetallesPresupuesto_Productos_IdProducto",
                table: "DetallesPresupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Clientes_IdCliente",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_CondicionesVenta_IdCondicionVenta",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Facturas_IdFacturaGenerada",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_FormasPago_IdFormaPago",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Usuarios_IdCreado_por",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Usuarios_IdEliminado_por",
                table: "Presupuestos");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Usuarios_IdUsuarioDescontoStock",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdCliente",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdCondicionVenta",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdCreado_por",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdEliminado_por",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdFacturaGenerada",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdFormaPago",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_Presupuestos_IdUsuarioDescontoStock",
                table: "Presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPresupuesto_IdPresupuesto",
                table: "DetallesPresupuesto");

            migrationBuilder.DropIndex(
                name: "IX_DetallesPresupuesto_IdProducto",
                table: "DetallesPresupuesto");

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CondicionVentaId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Creado_porId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Eliminado_porId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FacturaGeneradaId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FormaPagoId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioDescontoStockId",
                table: "Presupuestos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PresupuestoId",
                table: "DetallesPresupuesto",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "DetallesPresupuesto",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_ClienteId",
                table: "Presupuestos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_CondicionVentaId",
                table: "Presupuestos",
                column: "CondicionVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_Creado_porId",
                table: "Presupuestos",
                column: "Creado_porId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_Eliminado_porId",
                table: "Presupuestos",
                column: "Eliminado_porId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_FacturaGeneradaId",
                table: "Presupuestos",
                column: "FacturaGeneradaId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_FormaPagoId",
                table: "Presupuestos",
                column: "FormaPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_UsuarioDescontoStockId",
                table: "Presupuestos",
                column: "UsuarioDescontoStockId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPresupuesto_PresupuestoId",
                table: "DetallesPresupuesto",
                column: "PresupuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPresupuesto_ProductoId",
                table: "DetallesPresupuesto",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPresupuesto_Presupuestos_PresupuestoId",
                table: "DetallesPresupuesto",
                column: "PresupuestoId",
                principalTable: "Presupuestos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesPresupuesto_Productos_ProductoId",
                table: "DetallesPresupuesto",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Clientes_ClienteId",
                table: "Presupuestos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_CondicionesVenta_CondicionVentaId",
                table: "Presupuestos",
                column: "CondicionVentaId",
                principalTable: "CondicionesVenta",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Facturas_FacturaGeneradaId",
                table: "Presupuestos",
                column: "FacturaGeneradaId",
                principalTable: "Facturas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_FormasPago_FormaPagoId",
                table: "Presupuestos",
                column: "FormaPagoId",
                principalTable: "FormasPago",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Usuarios_Creado_porId",
                table: "Presupuestos",
                column: "Creado_porId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Usuarios_Eliminado_porId",
                table: "Presupuestos",
                column: "Eliminado_porId",
                principalTable: "Usuarios",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Usuarios_UsuarioDescontoStockId",
                table: "Presupuestos",
                column: "UsuarioDescontoStockId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
