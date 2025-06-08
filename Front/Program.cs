using Front.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Adicione esta linha para registrar IHttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FavoritoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/cursos");
    return Task.CompletedTask;
});

app.MapRazorPages();


app.MapPost("/api/favoritos/adicionar", (HttpRequest request) =>
{
    var curso = request.Query["curso"].ToString();
    FavoritoService.AdicionarCurso(curso);
    return Results.Ok();
});

app.MapPost("/api/favoritos/remover", (HttpRequest request) =>
{
    var curso = request.Query["curso"].ToString();
    FavoritoService.RemoverCurso(curso);
    return Results.Ok();
});

app.Run();
