using Backend.Services;
using Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure IHttpClientFactory e defina um timeout para o cliente nomeado
builder.Services.AddHttpClient("MoodleApiClient", client =>
{
    client.Timeout = TimeSpan.FromMinutes(25); // Defina um timeout generoso aqui
});

// Injete IHttpClientFactory no ApiService
builder.Services.AddSingleton<ApiService>(); 
builder.Services.AddSingleton<MongoService>();
builder.Services.AddHttpClient<GeminiService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


