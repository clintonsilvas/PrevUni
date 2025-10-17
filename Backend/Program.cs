using Backend.Services;
using MongoDB.Driver;
using static Backend.Services.MongoService;

var builder = WebApplication.CreateBuilder(args);

// -------------------- CONFIGURAÇÃO --------------------

// Configuração de arquivos (config.json + appsettings.json)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("config.json", optional: false, reloadOnChange: true);

// -------------------- MONGO CONFIG --------------------

var mongoSettings = builder.Configuration.GetSection("MongoSettings");
var connectionString = mongoSettings.GetValue<string>("ConnectionString");
var databaseName = mongoSettings.GetValue<string>("Database");

// MongoDB: Singleton para cliente e banco
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// -------------------- DEPENDÊNCIAS PERSONALIZADAS --------------------

// Serviços da aplicação
builder.Services.AddSingleton<MongoService>();
builder.Services.AddSingleton<UnifenasService>();    
builder.Services.AddSingleton<ImportacaoService>();
builder.Services.AddScoped<EngajamentoService>();
builder.Services.AddHttpClient("MoodleApiClient");     
builder.Services.AddHttpClient<GeminiService>();        

// -------------------- API / CORS / SWAGGER --------------------

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------- APP CONFIG --------------------

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/", () => "Backend do Prev Uni rodando");
app.Run();
