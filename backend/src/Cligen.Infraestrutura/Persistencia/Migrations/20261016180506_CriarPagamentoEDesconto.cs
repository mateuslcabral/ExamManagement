using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cligen.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarPagamentoEDesconto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Desconto",
                table: "Exame",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDesconto",
                table: "Exame",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Metodo = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataRecebimento = table.Column<DateOnly>(type: "date", nullable: true),
                    Convenio = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NumeroGuia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Parcelas = table.Column<int>(type: "int", nullable: true),
                    ValorParcela = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExcluidoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoExclusao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MotivoGlosa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamento", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Pagamento_Exame_ExameId",
                        column: x => x.ExameId,
                        principalTable: "Exame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagamento_Usuario_AtualizadoPorId",
                        column: x => x.AtualizadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagamento_Usuario_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagamento_Usuario_ExcluidoPorId",
                        column: x => x.ExcluidoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_AtualizadoPorId",
                table: "Pagamento",
                column: "AtualizadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_CriadoPorId",
                table: "Pagamento",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_ExameId",
                table: "Pagamento",
                column: "ExameId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_ExcluidoPorId",
                table: "Pagamento",
                column: "ExcluidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_Seq",
                table: "Pagamento",
                column: "Seq",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagamento_Status_DataRecebimento",
                table: "Pagamento",
                columns: new[] { "Status", "DataRecebimento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagamento");

            migrationBuilder.DropColumn(
                name: "Desconto",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "MotivoDesconto",
                table: "Exame");
        }
    }
}
