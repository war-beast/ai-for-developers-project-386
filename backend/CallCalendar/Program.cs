using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Interface;
using CallCalendar.DAL.Repository;

var builder = WebApplication.CreateBuilder(args);

// Слой View бекенда — REST-контроллеры в api/. Пользовательский интерфейс живёт
// в frontend/ (React SPA).
builder.Services.AddControllers();

// Данные хранятся в памяти процесса и сбрасываются при рестарте.
builder.Services.AddSingleton<IEventTypeRepository, InMemoryEventTypeRepository>();
builder.Services.AddSingleton<IEventTypeService, EventTypeService>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => "ok");

app.Run();

// Нужен интеграционным тестам API (WebApplicationFactory<Program>).
public partial class Program;
