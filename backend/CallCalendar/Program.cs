using CallCalendar.Api;
using CallCalendar.BLL.Interface;
using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Interface;
using CallCalendar.DAL.Repository;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<IEventTypeService, EventTypeService>();
builder.Services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
builder.Services.AddSingleton<IBookingService, BookingService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("Owner").Get<OwnerOptions>() ?? new OwnerOptions());
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ISlotService, SlotService>();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => "ok");

app.Run();

// Нужен интеграционным тестам API (WebApplicationFactory<Program>).
public partial class Program;
