using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cligen.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarCatalogoExamesEParametro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExameCatalogo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrazoExecucaoDias = table.Column<int>(type: "int", nullable: false),
                    PrecoReferencia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExameCatalogo", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Parametro",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametro", x => x.Chave);
                });

            migrationBuilder.InsertData(
                table: "Parametro",
                columns: new[] { "Chave", "AtualizadoEm", "Valor" },
                values: new object[] { "DiasRevisao", null, "3" });

            migrationBuilder.CreateIndex(
                name: "IX_ExameCatalogo_Nome",
                table: "ExameCatalogo",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExameCatalogo_Seq",
                table: "ExameCatalogo",
                column: "Seq",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExameCatalogo");

            migrationBuilder.DropTable(
                name: "Parametro");
        }
    }
}
