using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddIdAfipTipoDocumentoToCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AfipTipoDocumentoId",
                table: "Clientes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdAfipTipoDocumento",
                table: "Clientes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_AfipTipoDocumentoId",
                table: "Clientes",
                column: "AfipTipoDocumentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_AfipTiposDocumentos_AfipTipoDocumentoId",
                table: "Clientes",
                column: "AfipTipoDocumentoId",
                principalTable: "AfipTiposDocumentos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_AfipTiposDocumentos_AfipTipoDocumentoId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_AfipTipoDocumentoId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "AfipTipoDocumentoId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IdAfipTipoDocumento",
                table: "Clientes");
        }
    }
}
