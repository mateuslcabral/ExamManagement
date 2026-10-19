using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cligen.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class CadastrosCatalogoEAjustesCliente : Migration
    {
        /// <inheritdoc />
        /// <remarks>
        /// Reordenado à mão (reunião de 17/10/2026): cria os cadastros, semeia as 4 origens que eram lista fixa,
        /// copia destinos e convênios digitados como texto, converte as FKs e só então apaga as colunas antigas.
        /// </remarks>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Cadastros auxiliares e comentários
            migrationBuilder.CreateTable(
                name: "Origem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origem", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Origem_Usuario_AtualizadoPorId",
                        column: x => x.AtualizadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Origem_Usuario_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Origem_AtualizadoPorId", table: "Origem", column: "AtualizadoPorId");
            migrationBuilder.CreateIndex(name: "IX_Origem_CriadoPorId", table: "Origem", column: "CriadoPorId");
            migrationBuilder.CreateIndex(name: "IX_Origem_Nome", table: "Origem", column: "Nome", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Origem_Seq", table: "Origem", column: "Seq", unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateTable(
                name: "Destino",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Destino", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Destino_Usuario_AtualizadoPorId",
                        column: x => x.AtualizadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Destino_Usuario_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Destino_AtualizadoPorId", table: "Destino", column: "AtualizadoPorId");
            migrationBuilder.CreateIndex(name: "IX_Destino_CriadoPorId", table: "Destino", column: "CriadoPorId");
            migrationBuilder.CreateIndex(name: "IX_Destino_Nome", table: "Destino", column: "Nome", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Destino_Seq", table: "Destino", column: "Seq", unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateTable(
                name: "Convenio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Seq = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AtualizadoPorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convenio", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Convenio_Usuario_AtualizadoPorId",
                        column: x => x.AtualizadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Convenio_Usuario_CriadoPorId",
                        column: x => x.CriadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_Convenio_AtualizadoPorId", table: "Convenio", column: "AtualizadoPorId");
            migrationBuilder.CreateIndex(name: "IX_Convenio_CriadoPorId", table: "Convenio", column: "CriadoPorId");
            migrationBuilder.CreateIndex(name: "IX_Convenio_Nome", table: "Convenio", column: "Nome", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Convenio_Seq", table: "Convenio", column: "Seq", unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateTable(
                name: "Comentario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AutorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExameId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentario_Exame_ExameId",
                        column: x => x.ExameId,
                        principalTable: "Exame",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentario_Usuario_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            migrationBuilder.CreateIndex(name: "IX_Comentario_AutorId", table: "Comentario", column: "AutorId");
            migrationBuilder.CreateIndex(name: "IX_Comentario_ExameId", table: "Comentario", column: "ExameId");

            // 2. Dados: origens que eram enum (1..4), destinos e convênios que eram texto livre
            migrationBuilder.Sql("""
                INSERT INTO Origem (Id, Nome, Ativo, CriadoEm) VALUES
                    ('11111111-0000-4000-8000-000000000001', N'Cligen', 1, SYSUTCDATETIME()),
                    ('11111111-0000-4000-8000-000000000002', N'Clínica parceira', 1, SYSUTCDATETIME()),
                    ('11111111-0000-4000-8000-000000000003', N'Site Cligen', 1, SYSUTCDATETIME()),
                    ('11111111-0000-4000-8000-000000000004', N'Plataforma', 1, SYSUTCDATETIME());

                INSERT INTO Destino (Id, Nome, Ativo, CriadoEm)
                SELECT NEWID(), d.Nome, 1, SYSUTCDATETIME()
                FROM (SELECT DISTINCT LTRIM(RTRIM(Destino)) AS Nome FROM Exame WHERE Destino IS NOT NULL AND LTRIM(RTRIM(Destino)) <> '') d;

                INSERT INTO Convenio (Id, Nome, Ativo, CriadoEm)
                SELECT NEWID(), c.Nome, 1, SYSUTCDATETIME()
                FROM (SELECT DISTINCT LTRIM(RTRIM(Convenio)) AS Nome FROM Pagamento WHERE Convenio IS NOT NULL AND LTRIM(RTRIM(Convenio)) <> '') c;
                """);

            // 3. Novas colunas (OrigemId nasce nula para receber a conversão)
            migrationBuilder.AddColumn<Guid>(name: "OrigemId", table: "Exame", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "DestinoId", table: "Exame", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ConvenioId", table: "Pagamento", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<string>(name: "CrmMedico", table: "Exame", type: "nvarchar(20)", maxLength: 20, nullable: true);
            migrationBuilder.AddColumn<string>(name: "UfCrm", table: "Exame", type: "nchar(2)", fixedLength: true, maxLength: 2, nullable: true);
            migrationBuilder.AddColumn<DateOnly>(name: "DataAcolhimento", table: "Exame", type: "date", nullable: true);
            migrationBuilder.AlterColumn<string>(
                name: "MotivoDesconto", table: "Exame", type: "nvarchar(300)", maxLength: 300, nullable: true,
                oldClrType: typeof(string), oldType: "nvarchar(max)", oldNullable: true);
            // Anexos existentes não tinham tipo: ficam como "Outro" (3).
            migrationBuilder.AddColumn<int>(name: "Tipo", table: "Anexo", type: "int", nullable: false, defaultValue: 3);
            migrationBuilder.AddColumn<string>(name: "Cidade", table: "Paciente", type: "nvarchar(100)", maxLength: 100, nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "AtualizadoPorId", table: "ExameCatalogo", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "CriadoPorId", table: "ExameCatalogo", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "DestinoId", table: "ExameCatalogo", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<string>(name: "Genes", table: "ExameCatalogo", type: "nvarchar(4000)", maxLength: 4000, nullable: true);
            migrationBuilder.AddColumn<string>(name: "InformacoesTecnicas", table: "ExameCatalogo", type: "nvarchar(4000)", maxLength: 4000, nullable: true);
            migrationBuilder.AddColumn<int>(name: "NumeroGenes", table: "ExameCatalogo", type: "int", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "PrecoLimite", table: "ExameCatalogo", type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m);

            // 4. Conversão dos dados existentes
            migrationBuilder.Sql("""
                UPDATE Exame SET OrigemId = CASE Origem
                    WHEN 1 THEN '11111111-0000-4000-8000-000000000001'
                    WHEN 2 THEN '11111111-0000-4000-8000-000000000002'
                    WHEN 3 THEN '11111111-0000-4000-8000-000000000003'
                    WHEN 4 THEN '11111111-0000-4000-8000-000000000004'
                    ELSE '11111111-0000-4000-8000-000000000001' END;

                UPDATE e SET e.DestinoId = d.Id
                FROM Exame e JOIN Destino d ON d.Nome = LTRIM(RTRIM(e.Destino));

                UPDATE p SET p.ConvenioId = c.Id
                FROM Pagamento p JOIN Convenio c ON c.Nome = LTRIM(RTRIM(p.Convenio));

                UPDATE e SET e.DataAcolhimento = a.DataAcolhimento
                FROM Exame e JOIN Amostra a ON a.ExameId = e.Id AND a.RejeitadaEm IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrigemId", table: "Exame", type: "uniqueidentifier", nullable: false,
                oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            // 5. Fora com o que foi substituído: lista fixa de origem, textos livres e o parâmetro de dias de revisão
            migrationBuilder.DropColumn(name: "Origem", table: "Exame");
            migrationBuilder.DropColumn(name: "Destino", table: "Exame");
            migrationBuilder.DropColumn(name: "Convenio", table: "Pagamento");
            migrationBuilder.DropColumn(name: "DiasRevisao", table: "Amostra");
            migrationBuilder.DropTable(name: "Parametro");
            migrationBuilder.RenameColumn(name: "PrazoExecucaoDias", table: "ExameCatalogo", newName: "PrazoDias");
            migrationBuilder.RenameColumn(name: "PrazoExecucaoDias", table: "Amostra", newName: "PrazoDias");

            // 6. Índices e chaves estrangeiras
            migrationBuilder.CreateIndex(name: "IX_Pagamento_ConvenioId", table: "Pagamento", column: "ConvenioId");
            migrationBuilder.CreateIndex(name: "IX_ExameCatalogo_AtualizadoPorId", table: "ExameCatalogo", column: "AtualizadoPorId");
            migrationBuilder.CreateIndex(name: "IX_ExameCatalogo_CriadoPorId", table: "ExameCatalogo", column: "CriadoPorId");
            migrationBuilder.CreateIndex(name: "IX_ExameCatalogo_DestinoId", table: "ExameCatalogo", column: "DestinoId");
            migrationBuilder.CreateIndex(name: "IX_Exame_DataLiberacaoPrevista", table: "Exame", column: "DataLiberacaoPrevista");
            migrationBuilder.CreateIndex(name: "IX_Exame_DestinoId", table: "Exame", column: "DestinoId");
            migrationBuilder.CreateIndex(name: "IX_Exame_OrigemId", table: "Exame", column: "OrigemId");

            migrationBuilder.AddForeignKey(name: "FK_Exame_Destino_DestinoId", table: "Exame", column: "DestinoId", principalTable: "Destino", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Exame_Origem_OrigemId", table: "Exame", column: "OrigemId", principalTable: "Origem", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ExameCatalogo_Destino_DestinoId", table: "ExameCatalogo", column: "DestinoId", principalTable: "Destino", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ExameCatalogo_Usuario_AtualizadoPorId", table: "ExameCatalogo", column: "AtualizadoPorId", principalTable: "Usuario", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ExameCatalogo_Usuario_CriadoPorId", table: "ExameCatalogo", column: "CriadoPorId", principalTable: "Usuario", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_Pagamento_Convenio_ConvenioId", table: "Pagamento", column: "ConvenioId", principalTable: "Convenio", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exame_Destino_DestinoId",
                table: "Exame");

            migrationBuilder.DropForeignKey(
                name: "FK_Exame_Origem_OrigemId",
                table: "Exame");

            migrationBuilder.DropForeignKey(
                name: "FK_ExameCatalogo_Destino_DestinoId",
                table: "ExameCatalogo");

            migrationBuilder.DropForeignKey(
                name: "FK_ExameCatalogo_Usuario_AtualizadoPorId",
                table: "ExameCatalogo");

            migrationBuilder.DropForeignKey(
                name: "FK_ExameCatalogo_Usuario_CriadoPorId",
                table: "ExameCatalogo");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagamento_Convenio_ConvenioId",
                table: "Pagamento");

            migrationBuilder.DropTable(
                name: "Comentario");

            migrationBuilder.DropTable(
                name: "Convenio");

            migrationBuilder.DropTable(
                name: "Destino");

            migrationBuilder.DropTable(
                name: "Origem");

            migrationBuilder.DropIndex(
                name: "IX_Pagamento_ConvenioId",
                table: "Pagamento");

            migrationBuilder.DropIndex(
                name: "IX_ExameCatalogo_AtualizadoPorId",
                table: "ExameCatalogo");

            migrationBuilder.DropIndex(
                name: "IX_ExameCatalogo_CriadoPorId",
                table: "ExameCatalogo");

            migrationBuilder.DropIndex(
                name: "IX_ExameCatalogo_DestinoId",
                table: "ExameCatalogo");

            migrationBuilder.DropIndex(
                name: "IX_Exame_DataLiberacaoPrevista",
                table: "Exame");

            migrationBuilder.DropIndex(
                name: "IX_Exame_DestinoId",
                table: "Exame");

            migrationBuilder.DropIndex(
                name: "IX_Exame_OrigemId",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "ConvenioId",
                table: "Pagamento");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Paciente");

            migrationBuilder.DropColumn(
                name: "AtualizadoPorId",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "CriadoPorId",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "DestinoId",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "Genes",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "InformacoesTecnicas",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "NumeroGenes",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "PrecoLimite",
                table: "ExameCatalogo");

            migrationBuilder.DropColumn(
                name: "CrmMedico",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "DataAcolhimento",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "DestinoId",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "OrigemId",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "UfCrm",
                table: "Exame");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Anexo");

            migrationBuilder.RenameColumn(
                name: "PrazoDias",
                table: "ExameCatalogo",
                newName: "PrazoExecucaoDias");

            migrationBuilder.RenameColumn(
                name: "PrazoDias",
                table: "Amostra",
                newName: "PrazoExecucaoDias");

            migrationBuilder.AddColumn<string>(
                name: "Convenio",
                table: "Pagamento",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MotivoDesconto",
                table: "Exame",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Destino",
                table: "Exame",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Origem",
                table: "Exame",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DiasRevisao",
                table: "Amostra",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Parametro",
                columns: table => new
                {
                    Chave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Valor = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parametro", x => x.Chave);
                });

            migrationBuilder.InsertData(
                table: "Parametro",
                columns: new[] { "Chave", "AtualizadoEm", "Valor" },
                values: new object[] { "DiasRevisao", null, "3" });
        }
    }
}
