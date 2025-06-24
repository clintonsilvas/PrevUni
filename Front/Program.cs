using Front.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
// Adicione esta linha para registrar IHttpClientFactory
builder.Services.AddHttpClient();
builder.Services.AddSingleton<FavoritoService>();

builder.Services.AddSession();
builder.Services
       .AddRazorPages()
       .AddSessionStateTempDataProvider();   // mantém TempData em sessão se quiser


var app = builder.Build();
app.UseSession();
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


app.MapPost("/api/favoritos/adicionar_curso", (HttpRequest request) =>
{
    var curso = request.Query["nomeCurso"].ToString();
    FavoritoService.AdicionarCurso(curso);
    return Results.Ok();
});

app.MapPost("/api/favoritos/remover_curso", (HttpRequest request) =>
{
    var curso = request.Query["nomeCurso"].ToString();
    FavoritoService.RemoverCurso(curso);
    return Results.Ok();
});


app.MapPost("/api/favoritos/adicionar_aluno", (HttpRequest request) =>
{
    var nome = request.Query["name"].ToString();
    var id = request.Query["id"].ToString();
    FavoritoService.AdicionarAluno(nome, id);
    return Results.Ok();
});

app.MapPost("/api/favoritos/remover_aluno", (HttpRequest request) =>
{
    var nome = request.Query["name"].ToString();
    var id = request.Query["id"].ToString();
    FavoritoService.RemoverAluno(nome, id);
    return Results.Ok();
});




app.Run();
