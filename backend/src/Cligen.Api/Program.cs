using System.Text.Json.Serialization;
using Cligen.Api.Configuracao;
using Cligen.Api.Middlewares;
using Cligen.Aplicacao.Interfaces.Servicos;
using Cligen.Aplicacao.Servicos;
using Cligen.Infraestrutura;

var builder = WebApplication.CreateBuilder(args);

// Camadas (composition root — único lugar que conhece a Infraestrutura).
builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<ExameCatalogoService>();
builder.Services.AddScoped<ExameService>();
builder.Services.AddScoped<AcolhimentoService>();
builder.Services.AddScoped<LaudoService>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddSingleton<IUrlDefinicaoSenha, UrlDefinicaoSenha>();

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExcecaoDeDominioMiddleware>();
app.UseMiddleware<ChaveDeServicoMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

await SeedUsuarioInicial.ExecutarAsync(app.Services, app.Configuration, app.Logger);
await SeedCatalogoInicial.ExecutarAsync(app.Services, app.Logger);

app.Run();
