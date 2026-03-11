using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCajaModuleEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_SesionesCaja_SesionCajaId",
                table: "Comprobantes");

            migrationBuilder.DropForeignKey(
                name: "FK_SesionesCaja_Cajas_CajaId",
                table: "SesionesCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_SesionesCaja_Usuarios_UsuarioId",
                table: "SesionesCaja");

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoCierreSistema",
                table: "SesionesCaja",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoCierreReal",
                table: "SesionesCaja",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoApertura",
                table: "SesionesCaja",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "MovimientosCaja",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "Activa",
                table: "Cajas",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_SesionesCaja_SesionCajaId",
                table: "Comprobantes",
                column: "SesionCajaId",
                principalTable: "SesionesCaja",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SesionesCaja_Cajas_CajaId",
                table: "SesionesCaja",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SesionesCaja_Usuarios_UsuarioId",
                table: "SesionesCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comprobantes_SesionesCaja_SesionCajaId",
                table: "Comprobantes");

            migrationBuilder.DropForeignKey(
                name: "FK_SesionesCaja_Cajas_CajaId",
                table: "SesionesCaja");

            migrationBuilder.DropForeignKey(
                name: "FK_SesionesCaja_Usuarios_UsuarioId",
                table: "SesionesCaja");

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoCierreSistema",
                table: "SesionesCaja",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoCierreReal",
                table: "SesionesCaja",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MontoApertura",
                table: "SesionesCaja",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Monto",
                table: "MovimientosCaja",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "Activa",
                table: "Cajas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comprobantes_SesionesCaja_SesionCajaId",
                table: "Comprobantes",
                column: "SesionCajaId",
                principalTable: "SesionesCaja",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SesionesCaja_Cajas_CajaId",
                table: "SesionesCaja",
                column: "CajaId",
                principalTable: "Cajas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SesionesCaja_Usuarios_UsuarioId",
                table: "SesionesCaja",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
