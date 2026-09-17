using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cligen.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CriarExameEAnexo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exame",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PacienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExameCatalogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Origem = table.Column<int>(type: "int", nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoMedico = table.Column<int>(type: "int", nullable: false),
                    NomeMedicoExterno = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Preco = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DataEntrada = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExcluidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExcluidoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MotivoExclusao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exame", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Exame_ExameCatalogo_ExameCatalogoId",
                        column: x => x.ExameCatalogoId,
                        principalTable: "ExameCatalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exame_Paciente_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Paciente",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exame_Usuario_AtualizadoPorId",
                        column: x => x.AtualizadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exame_Usuario_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exame_Usuario_ExcluidoPorId",
                        column: x => x.ExcluidoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Anexo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomeOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TipoConteudo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    HashSha256 = table.Column<string>(type: "char(64)", unicode: false, fixedLength: true, maxLength: 64, nullable: false),
                    Caminho = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EnviadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnviadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RemovidoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemovidoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anexo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anexo_Exame_ExameId",
                        column: x => x.ExameId,
                        principalTable: "Exame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anexo_Usuario_EnviadoPorId",
                        column: x => x.EnviadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Anexo_Usuario_RemovidoPorId",
                        column: x => x.RemovidoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anexo_EnviadoPorId",
                table: "Anexo",
                column: "EnviadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Anexo_ExameId",
                table: "Anexo",
                column: "ExameId");

            migrationBuilder.CreateIndex(
                name: "IX_Anexo_RemovidoPorId",
                table: "Anexo",
                column: "RemovidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_AtualizadoPorId",
                table: "Exame",
                column: "AtualizadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_CriadoPorId",
                table: "Exame",
                column: "CriadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_Estado",
                table: "Exame",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_ExameCatalogoId",
                table: "Exame",
                column: "ExameCatalogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_ExcluidoPorId",
                table: "Exame",
                column: "ExcluidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_PacienteId",
                table: "Exame",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Exame_Seq",
                table: "Exame",
                column: "Seq",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Anexo");

            migrationBuilder.DropTable(
                name: "Exame");
        }
    }
}
