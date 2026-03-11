using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AfipCondicionesIva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfipCondicionesIva", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AfipTiposDocumentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FechaDesde = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaHasta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfipTiposDocumentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AfipTiposIva",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Porcentaje = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    FechaDesde = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaHasta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfipTiposIva", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposFacturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CodigoAfip = table.Column<int>(type: "INTEGER", nullable: false),
                    DescripcionAfip = table.Column<string>(type: "TEXT", nullable: true),
                    FechaDesdeAfip = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaHastaAfip = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UltimaActualizacionAfip = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposFacturas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AfipConfiguraciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Cuit = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IdAfipCondicionIva = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoriaMonotributo = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    IngresosBrutosNumero = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    InicioActividades = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TieneFacturaElectronica = table.Column<bool>(type: "INTEGER", nullable: false),
                    LimiteMontoFactura = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    PuntoVentaPorDefecto = table.Column<int>(type: "INTEGER", nullable: false),
                    DireccionFiscal = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EmailContacto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfipConfiguraciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AfipConfiguraciones_AfipCondicionesIva_IdAfipCondicionIva",
                        column: x => x.IdAfipCondicionIva,
                        principalTable: "AfipCondicionesIva",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Url_imagen = table.Column<string>(type: "TEXT", nullable: false),
                    IdRol = table.Column<int>(type: "INTEGER", nullable: false),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_IdRol",
                        column: x => x.IdRol,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AfipTiposComprobantesHabilitados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdAfipConfiguracion = table.Column<int>(type: "INTEGER", nullable: false),
                    IdTipoFactura = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaDesde = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaHasta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Habilitado = table.Column<bool>(type: "INTEGER", nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfipTiposComprobantesHabilitados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AfipTiposComprobantesHabilitados_AfipConfiguraciones_IdAfipConfiguracion",
                        column: x => x.IdAfipConfiguracion,
                        principalTable: "AfipConfiguraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AfipTiposComprobantesHabilitados_TiposFacturas_IdTipoFactura",
                        column: x => x.IdTipoFactura,
                        principalTable: "TiposFacturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Documento = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IdAfipCondicionIva = table.Column<int>(type: "INTEGER", nullable: false),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_AfipCondicionesIva_IdAfipCondicionIva",
                        column: x => x.IdAfipCondicionIva,
                        principalTable: "AfipCondicionesIva",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_IdCreado_por",
                        column: x => x.IdCreado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_IdEliminado_por",
                        column: x => x.IdEliminado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CondicionesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DiasVencimiento = table.Column<int>(type: "INTEGER", nullable: false),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondicionesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CondicionesVenta_Usuarios_IdCreado_por",
                        column: x => x.IdCreado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CondicionesVenta_Usuarios_IdEliminado_por",
                        column: x => x.IdEliminado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormasPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PorcentajeAjuste = table.Column<decimal>(type: "TEXT", nullable: true),
                    EsEditable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormasPago_Usuarios_IdCreado_por",
                        column: x => x.IdCreado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FormasPago_Usuarios_IdEliminado_por",
                        column: x => x.IdEliminado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    Proveedor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Usuarios_IdCreado_por",
                        column: x => x.IdCreado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Usuarios_IdEliminado_por",
                        column: x => x.IdEliminado_por,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdCliente = table.Column<int>(type: "INTEGER", nullable: true),
                    ClienteDocumento = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true),
                    ClienteNombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClienteApellido = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClienteTelefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ClienteCorreo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ClienteDireccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    IdTipoFactura = table.Column<int>(type: "INTEGER", nullable: false),
                    IdFormaPago = table.Column<int>(type: "INTEGER", nullable: false),
                    IdCondicionVenta = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", nullable: false),
                    PorcentajeAjuste = table.Column<decimal>(type: "TEXT", nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", nullable: false),
                    PuntoVenta = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroComprobante = table.Column<long>(type: "INTEGER", nullable: true),
                    CAE = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true),
                    CAEVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CodigoConcepto = table.Column<int>(type: "INTEGER", nullable: false),
                    IdAfipTipoDocumento = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaServicioDesde = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaServicioHasta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaVencimientoPago = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CodigoMoneda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    CotizacionMoneda = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    ImporteNetoGravado = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImporteIVA = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImporteExento = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ImporteTributos = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Creado_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IdCreado_por = table.Column<int>(type: "INTEGER", nullable: false),
                    Eliminado_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IdEliminado_por = table.Column<int>(type: "INTEGER", nullable: true)
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
                    ProductoNombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProductoCodigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_IdFactura",
                        column: x => x.IdFactura,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Productos_IdProducto",
                        column: x => x.IdProducto,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AfipConfiguraciones_Cuit",
                table: "AfipConfiguraciones",
                column: "Cuit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AfipConfiguraciones_IdAfipCondicionIva",
                table: "AfipConfiguraciones",
                column: "IdAfipCondicionIva");

            migrationBuilder.CreateIndex(
                name: "IX_AfipTiposComprobantesHabilitados_IdAfipConfiguracion_IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados",
                columns: new[] { "IdAfipConfiguracion", "IdTipoFactura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AfipTiposComprobantesHabilitados_IdTipoFactura",
                table: "AfipTiposComprobantesHabilitados",
                column: "IdTipoFactura");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdAfipCondicionIva",
                table: "Clientes",
                column: "IdAfipCondicionIva");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdCreado_por",
                table: "Clientes",
                column: "IdCreado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdEliminado_por",
                table: "Clientes",
                column: "IdEliminado_por");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesVenta_IdCreado_por",
                table: "CondicionesVenta",
                column: "IdCreado_por");

            migrationBuilder.CreateIndex(
                name: "IX_CondicionesVenta_IdEliminado_por",
                table: "CondicionesVenta",
                column: "IdEliminado_por");

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
                name: "IX_FormasPago_IdCreado_por",
                table: "FormasPago",
                column: "IdCreado_por");

            migrationBuilder.CreateIndex(
                name: "IX_FormasPago_IdEliminado_por",
                table: "FormasPago",
                column: "IdEliminado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdCreado_por",
                table: "Productos",
                column: "IdCreado_por");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_IdEliminado_por",
                table: "Productos",
                column: "IdEliminado_por");

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

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdRol",
                table: "Usuarios",
                column: "IdRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AfipTiposComprobantesHabilitados");

            migrationBuilder.DropTable(
                name: "AfipTiposIva");

            migrationBuilder.DropTable(
                name: "DetallesFactura");

            migrationBuilder.DropTable(
                name: "AfipConfiguraciones");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "AfipTiposDocumentos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "CondicionesVenta");

            migrationBuilder.DropTable(
                name: "FormasPago");

            migrationBuilder.DropTable(
                name: "TiposFacturas");

            migrationBuilder.DropTable(
                name: "AfipCondicionesIva");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
