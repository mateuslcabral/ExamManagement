using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cligen.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarAmostra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataLiberacaoPrevista",
                table: "Exame",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Amostra",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataAcolhimento = table.Column<DateOnly>(type: "date", nullable: false),
                    PrazoExecucaoDias = table.Column<int>(type: "int", nullable: false),
                    DiasRevisao = table.Column<int>(type: "int", nullable: false),
                    DataLiberacaoPrevista = table.Column<DateOnly>(type: "date", nullable: false),
                    Recoleta = table.Column<bool>(type: "bit", nullable: false),
                    RegistradoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RejeitadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejeitadaPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amostra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amostra_Exame_ExameId",
                        column: x => x.ExameId,
                        principalTable: "Exame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Amostra_Usuario_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Amostra_Usuario_RejeitadaPorId",
                        column: x => x.RejeitadaPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amostra_ExameId",
                table: "Amostra",
                column: "ExameId");

            migrationBuilder.CreateIndex(
                name: "IX_Amostra_RegistradoPorId",
                table: "Amostra",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Amostra_RejeitadaPorId",
                table: "Amostra",
                column: "RejeitadaPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amostra");

            migrationBuilder.DropColumn(
                name: "DataLiberacaoPrevista",
                table: "Exame");
        }
    }
}
