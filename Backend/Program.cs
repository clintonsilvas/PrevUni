using Backend.Services;
using MongoDB.Driver;
using static Backend.Services.MongoService;

var builder = WebApplication.CreateBuilder(args);

// L� configura��es MongoDB do appsettings.json
var mongoSettings = builder.Configuration.GetSection("MongoSettings");
var connectionString = mongoSettings.GetValue<string>("ConnectionString");
var databaseName = mongoSettings.GetValue<string>("Database");

// Configura MongoClient e IMongoDatabase no DI container
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// Registra seus servi�os
builder.Services.AddSingleton<MongoService>();
builder.Services.AddScoped<EngajamentoService>();

// Servi�os padr�o
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Exemplo: adicionando GeminiService e ApiService, se quiser
builder.Services.AddSingleton<ApiService>();
builder.Services.AddHttpClient<GeminiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
