using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cligen.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarEtapaLaudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataLiberacaoEfetiva",
                table: "Exame",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EtapaAndamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Caminho = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    HashSha256 = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    RegistradoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Substituicoes = table.Column<int>(type: "int", nullable: false),
                    SubstituidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubstituidoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EtapaAndamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EtapaAndamento_Exame_ExameId",
                        column: x => x.ExameId,
                        principalTable: "Exame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EtapaAndamento_Usuario_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EtapaAndamento_Usuario_SubstituidoPorId",
                        column: x => x.SubstituidoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EtapaAndamento_ExameId_Tipo",
                table: "EtapaAndamento",
                columns: new[] { "ExameId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EtapaAndamento_RegistradoPorId",
                table: "EtapaAndamento",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EtapaAndamento_SubstituidoPorId",
                table: "EtapaAndamento",
                column: "SubstituidoPorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EtapaAndamento");

            migrationBuilder.DropColumn(
                name: "DataLiberacaoEfetiva",
                table: "Exame");
        }
    }
}
