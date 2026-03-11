using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPresupuestos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdCliente = table.Column<int>(type: "INTEGER", nullable: true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    ClienteDocumento = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteNombre = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteApellido = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteTelefono = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteCorreo = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteDireccion = table.Column<string>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    IdFormaPago = table.Column<int>(type: "INTEGER", nullable: false),
                    FormaPagoId = table.Column<int>(type: "INTEGER", nullable: true),
                    IdCondicionVenta = table.Column<int>(type: "INTEGER", nullable: false),
                    CondicionVentaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Subtotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    PorcentajeAjuste = table.Column<decimal>(type: "TEXT", nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", nullable: false),
                    NumeroPresupuesto = table.Column<int>(type: "INTEGER", nullable: false),
                    DescontarStock = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaDescontadoStock = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdUsuarioDescontoStock = table.Column<int>(type: "INTEGER", nullable: true),
                    UsuarioDescontoStockId = table.Column<int>(type: "INTEGER", nullable: true),
                    IdFacturaGenerada = table.Column<int>(type: "INTEGER", nullable: true),
                    FacturaGeneradaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    Creado_porId = table.Column<int>(type: "INTEGER", nullable: true),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true),
                    Eliminado_porId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Presupuestos_CondicionesVenta_CondicionVentaId",
                        column: x => x.CondicionVentaId,
                        principalTable: "CondicionesVenta",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Presupuestos_Facturas_FacturaGeneradaId",
                        column: x => x.FacturaGeneradaId,
                        principalTable: "Facturas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Presupuestos_FormasPago_FormaPagoId",
                        column: x => x.FormaPagoId,
                        principalTable: "FormasPago",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Presupuestos_Usuarios_Creado_porId",
                        column: x => x.Creado_porId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Presupuestos_Usuarios_Eliminado_porId",
                        column: x => x.Eliminado_porId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Presupuestos_Usuarios_UsuarioDescontoStockId",
                        column: x => x.UsuarioDescontoStockId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetallesPresupuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdPresupuesto = table.Column<int>(type: "INTEGER", nullable: false),
                    PresupuestoId = table.Column<int>(type: "INTEGER", nullable: true),
                    IdProducto = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ProductoNombre = table.Column<string>(type: "TEXT", nullable: false),
                    ProductoCodigo = table.Column<string>(type: "TEXT", nullable: true),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", nullable: false),
                    PorcentajeFormaPago = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPresupuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesPresupuesto_Presupuestos_PresupuestoId",
                        column: x => x.PresupuestoId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DetallesPresupuesto_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPresupuesto_PresupuestoId",
                table: "DetallesPresupuesto",
                column: "PresupuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPresupuesto_ProductoId",
                table: "DetallesPresupuesto",
                column: "ProductoId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesPresupuesto");

            migrationBuilder.DropTable(
                name: "Presupuestos");
        }
    }
}
