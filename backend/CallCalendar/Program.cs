var builder = WebApplication.CreateBuilder(args);

// Слой View бекенда — REST-контроллеры в api/. Пользовательский интерфейс живёт
// в frontend/ (React SPA).
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => "ok");

app.Run();

// Нужен интеграционным тестам API (WebApplicationFactory<Program>).
public partial class Program;
