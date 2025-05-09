using PrevUni.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<AlunoService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient();


// Adiciona o Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // Adiciona logs no console

var app = builder.Build();

// Habilita o Swagger no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
app.Run();