using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFacturaToComprobante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AfipTiposComprobantesHabilitados_TiposFacturas_IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Facturas_IdFacturaGenerada",
                table: "Presupuestos");

            migrationBuilder.RenameTable(
                name: "Facturas",
                newName: "Comprobantes");

            migrationBuilder.RenameTable(
                name: "DetallesFactura",
                newName: "DetallesComprobante");

            migrationBuilder.RenameTable(
                name: "TiposFacturas",
                newName: "TiposComprobantes");

            migrationBuilder.RenameColumn(
                name: "IdTipoFactura",
                table: "Comprobantes",
                newName: "IdTipoComprobante");

            migrationBuilder.RenameColumn(
                name: "IdFactura",
                table: "DetallesComprobante",
                newName: "IdComprobante");

            migrationBuilder.RenameColumn(
                name: "IdFacturaGenerada",
                table: "Presupuestos",
                newName: "IdComprobanteGenerado");

            migrationBuilder.RenameIndex(
                name: "IX_Presupuestos_IdFacturaGenerada",
                table: "Presupuestos",
                newName: "IX_Presupuestos_IdComprobanteGenerado");

            migrationBuilder.RenameColumn(
                name: "IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados",
                newName: "IdTipoComprobante");

            migrationBuilder.RenameIndex(
                name: "IX_AfipTiposComprobantesHabilitados_IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados",
                newName: "IX_AfipTiposComprobantesHabilitados_IdTipoComprobante");

            migrationBuilder.RenameIndex(
                name: "IX_AfipTiposComprobantesHabilitados_IdAfipConfiguracion_IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados",
                newName: "IX_AfipTiposComprobantesHabilitados_IdAfipConfiguracion_IdTipoComprobante");

            migrationBuilder.AddForeignKey(
                name: "FK_AfipTiposComprobantesHabilitados_TiposComprobantes_IdTipoComprobante",
                table: "AfipTiposComprobantesHabilitados",
                column: "IdTipoComprobante",
                principalTable: "TiposComprobantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Comprobantes_IdComprobanteGenerado",
                table: "Presupuestos",
                column: "IdComprobanteGenerado",
                principalTable: "Comprobantes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AfipTiposComprobantesHabilitados_TiposComprobantes_IdTipoComprobante",
                table: "AfipTiposComprobantesHabilitados");

            migrationBuilder.DropForeignKey(
                name: "FK_Presupuestos_Comprobantes_IdComprobanteGenerado",
                table: "Presupuestos");

            migrationBuilder.DropTable(
                name: "DetallesComprobante");

            migrationBuilder.DropTable(
                name: "Comprobantes");

            migrationBuilder.DropTable(
                name: "TiposComprobantes");

            migrationBuilder.RenameColumn(
                name: "IdComprobanteGenerado",
                table: "Presupuestos",
                newName: "IdFacturaGenerada");

            migrationBuilder.RenameIndex(
                name: "IX_Presupuestos_IdComprobanteGenerado",
                table: "Presupuestos",
                newName: "IX_Presupuestos_IdFacturaGenerada");

            migrationBuilder.RenameColumn(
                name: "IdTipoComprobante",
                table: "AfipTiposComprobantesHabilitados",
                newName: "IdTipoFactura");

            migrationBuilder.RenameIndex(
                name: "IX_AfipTiposComprobantesHabilitados_IdTipoComprobante",
                table: "AfipTiposComprobantesHabilitados",
                newName: "IX_AfipTiposComprobantesHabilitados_IdTipoFactura");

            migrationBuilder.RenameIndex(
                name: "IX_AfipTiposComprobantesHabilitados_IdAfipConfiguracion_IdTipoComprobante",
                table: "AfipTiposComprobantesHabilitados",
                newName: "IX_AfipTiposComprobantesHabilitados_IdAfipConfiguracion_IdTipoFactura");

            migrationBuilder.CreateTable(
                name: "TiposFacturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CodigoAfip = table.Column<int>(type: "INTEGER", nullable: false),
                    DescripcionAfip = table.Column<string>(type: "TEXT", nullable: true),
                    FechaDesdeAfip = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaHastaAfip = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UltimaActualizacionAfip = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposFacturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdAfipTipoDocumento = table.Column<int>(type: "INTEGER", nullable: true),
                    IdCliente = table.Column<int>(type: "INTEGER", nullable: true),
                    IdCondicionVenta = table.Column<int>(type: "INTEGER", nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true),
                    IdFormaPago = table.Column<int>(type: "INTEGER", nullable: false),
                    IdTipoFactura = table.Column<int>(type: "INTEGER", nullable: false),
                    CAE = table.Column<string>(type: "TEXT", nullable: true),
                    CAEVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClienteApellido = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteCorreo = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteDireccion = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteDocumento = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteNombre = table.Column<string>(type: "TEXT", nullable: true),
                    ClienteTelefono = table.Column<string>(type: "TEXT", nullable: true),
                    CodigoConcepto = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoMoneda = table.Column<string>(type: "TEXT", nullable: false),
                    CotizacionMoneda = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaServicioDesde = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaServicioHasta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaVencimientoPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImporteExento = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImporteIVA = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImporteNetoGravado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImporteTributos = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    NumeroComprobante = table.Column<long>(type: "INTEGER", nullable: true),
                    PorcentajeAjuste = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PuntoVenta = table.Column<int>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturas_AfipTiposDocumentos_IdAfipTipoDocumento",
                        column: x => x.IdAfipTipoDocumento,
                        principalTable: "AfipTiposDocumentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_CondicionesVenta_IdCondicionVenta",
                        column: x => x.IdCondicionVenta,
                        principalTable: "CondicionesVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_FormasPago_IdFormaPago",
                        column: x => x.IdFormaPago,
                        principalTable: "FormasPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_TiposFacturas_IdTipoFactura",
                        column: x => x.IdTipoFactura,
                        principalTable: "TiposFacturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Usuarios_IdCreado_por",
                        column: x => x.IdCreado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Usuarios_IdEliminado_por",
                        column: x => x.IdEliminado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesFactura",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdFactura = table.Column<int>(type: "INTEGER", nullable: false),
                    IdProducto = table.Column<int>(type: "INTEGER", nullable: true),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PorcentajeFormaPago = table.Column<decimal>(type: "TEXT", nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ProductoCodigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ProductoNombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_IdFactura",
                        column: x => x.IdFactura,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_IdFactura",
                table: "DetallesFactura",
                column: "IdFactura");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_IdProducto",
                table: "DetallesFactura",
                column: "IdProducto");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_CAE",
                table: "Facturas",
                column: "CAE",
                filter: "[CAE] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdAfipTipoDocumento",
                table: "Facturas",
                column: "IdAfipTipoDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdCliente",
                table: "Facturas",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdCondicionVenta",
                table: "Facturas",
                column: "IdCondicionVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdCreado_por",
                table: "Facturas",
                column: "IdCreado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdEliminado_por",
                table: "Facturas",
                column: "IdEliminado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdFormaPago",
                table: "Facturas",
                column: "IdFormaPago");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_IdTipoFactura",
                table: "Facturas",
                column: "IdTipoFactura");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_PuntoVenta_NumeroComprobante_IdTipoFactura",
                table: "Facturas",
                columns: new[] { "PuntoVenta", "NumeroComprobante", "IdTipoFactura" },
                unique: true,
                filter: "[NumeroComprobante] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TiposFacturas_CodigoAfip",
                table: "TiposFacturas",
                column: "CodigoAfip",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposFacturas_Nombre",
                table: "TiposFacturas",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AfipTiposComprobantesHabilitados_TiposFacturas_IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados",
                column: "IdTipoFactura",
                principalTable: "TiposFacturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Presupuestos_Facturas_IdFacturaGenerada",
                table: "Presupuestos",
                column: "IdFacturaGenerada",
                principalTable: "Facturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
