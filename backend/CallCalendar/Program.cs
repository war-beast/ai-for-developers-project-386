using CallCalendar.Api;
using CallCalendar.Api.ProblemResults;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Interface;
using CallCalendar.DAL.Repository;

var builder = WebApplication.CreateBuilder(args);

// Хостинг (Render) передаёт порт в PORT; локально работает порт по умолчанию.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Слой View бекенда — REST-контроллеры в api/. Пользовательский интерфейс живёт
// в frontend/ (React SPA).
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        // Иначе [ApiController] отдаёт ProblemDetails без code и с ключами вида $.start.
        options.InvalidModelStateResponseFactory = context =>
            Problems.ValidationFailed(context.ModelState));

// Данные хранятся в памяти процесса и сбрасываются при рестарте.
builder.Services.AddSingleton<IEventTypeRepository, InMemoryEventTypeRepository>();
builder.Services.AddTransient<IEventTypeService, EventTypeService>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddTransient<IBookingService, BookingService>();
builder.Services.Configure<OwnerOptions>(builder.Configuration.GetSection("Owner"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddTransient<ISlotService, SlotService>();
builder.Services.AddTransient<IProblemResultFactory, ProblemResultFactory>();

var app = builder.Build();

// В Docker фронтенд собирается в wwwroot и отдаётся с того же origin, что API.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapGet("/health", () => "ok");
app.MapFallbackToFile("index.html");

app.Run();

// Нужен интеграционным тестам API (WebApplicationFactory<Program>).
public partial class Program;
