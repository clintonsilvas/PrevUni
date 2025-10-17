using Front.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddRazorPages()
       .AddSessionStateTempDataProvider();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<FavoritoService>();
builder.Services.AddScoped<Curso>();

builder.Services.AddSession();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCors();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", context =>
{
    context.Response.Redirect("/cursos");
    return Task.CompletedTask;
});

app.MapRazorPages();

app.Run();
