using LowiskoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<BazaDanych>();
builder.Services.AddSingleton<StanowiskaService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<LowiskoWeb.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
