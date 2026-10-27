using CallCalendar.BLL.Model;
using CallCalendar.BLL.Service;
using CallCalendar.DAL.Repository;

namespace CallCalendar.Tests;

public class EventTypeServiceTests
{
    private readonly EventTypeService _service = new(new InMemoryEventTypeRepository());

    [Fact]
    public void CreateStoresTrimmedEventType()
    {
        var result = _service.Create(
            new CreateEventTypeCommand("intro-call", "  Знакомство  ", "Короткий звонок", 30));

        Assert.NotNull(result.Created);
        Assert.Equal("intro-call", result.Created.Id);
        Assert.Equal("Знакомство", result.Created.Title);
        Assert.Equal("Короткий звонок", result.Created.Description);
        Assert.Equal(30, result.Created.DurationMinutes);
        Assert.Empty(result.Errors);
        Assert.False(result.SlugTaken);
    }

    [Theory]
    [InlineData("Bad Slug", "Знакомство", 30, "id")]
    [InlineData("", "Знакомство", 30, "id")]
    [InlineData("intro-call", "   ", 30, "title")]
    [InlineData("intro-call", "Знакомство", 20, "durationMinutes")]
    [InlineData("intro-call", "Знакомство", 0, "durationMinutes")]
    [InlineData("intro-call", "Знакомство", 255, "durationMinutes")]
    public void CreateRejectsInvalidFields(string id, string title, int durationMinutes, string expectedError)
    {
        var result = _service.Create(new CreateEventTypeCommand(id, title, "", durationMinutes));

        Assert.Null(result.Created);
        Assert.Contains(expectedError, result.Errors.Keys);
        Assert.False(result.SlugTaken);
    }

    [Fact]
    public void CreateRejectsTooLongDescription()
    {
        var result = _service.Create(
            new CreateEventTypeCommand("intro-call", "Знакомство", new string('x', 1001), 30));

        Assert.Null(result.Created);
        Assert.Contains("description", result.Errors.Keys);
    }

    [Fact]
    public void CreateAcceptsEmptyDescription()
    {
        var result = _service.Create(new CreateEventTypeCommand("intro-call", "Знакомство", null, 15));

        Assert.NotNull(result.Created);
        Assert.Equal(string.Empty, result.Created.Description);
    }

    [Fact]
    public void CreateRejectsDuplicateSlug()
    {
        _service.Create(new CreateEventTypeCommand("intro-call", "Знакомство", "", 30));

        var result = _service.Create(new CreateEventTypeCommand("intro-call", "Другое", "", 60));

        Assert.Null(result.Created);
        Assert.True(result.SlugTaken);
        Assert.Single(_service.GetAll());
    }

    [Fact]
    public void GetAllReturnsCreatedTypesInInsertionOrder()
    {
        _service.Create(new CreateEventTypeCommand("first", "Первый", "", 30));
        _service.Create(new CreateEventTypeCommand("second", "Второй", "", 45));

        Assert.Equal(["first", "second"], _service.GetAll()
            .Select(eventType => eventType.Id));
    }
}
