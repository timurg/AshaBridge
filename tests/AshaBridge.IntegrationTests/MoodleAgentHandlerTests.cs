using AshaBridge.Extensions.Moodle;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Extensions.Moodle.Handlers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AshaBridge.IntegrationTests;

public sealed class MoodleAgentHandlerTests
{
    [Fact]
    public async Task CourseGetById_UsesInternalIdField()
    {
        using var http = new HttpClient();
        var options = Options.Create(new MoodleExtensionOptions());
        var client = new MoodleWebServiceClient(http, options, NullLogger<MoodleWebServiceClient>.Instance);
        var handler = new MoodleCourseGetByIdHandler(client);

        var response = await handler.HandleAsync(
            new MoodleCourseGetByIdRequest(46),
            null!,
            CancellationToken.None);

        Assert.Equal("core_course_get_courses_by_field", response.Data?["function"]?.GetValue<string>());
        Assert.Equal("id", response.Data?["payload"]?["field"]?.GetValue<string>());
        Assert.Equal("46", response.Data?["payload"]?["value"]?.GetValue<string>());
    }

    [Fact]
    public async Task CourseFindByIdNumber_UsesIdNumberField()
    {
        using var http = new HttpClient();
        var options = Options.Create(new MoodleExtensionOptions());
        var client = new MoodleWebServiceClient(http, options, NullLogger<MoodleWebServiceClient>.Instance);
        var handler = new MoodleCourseFindByIdNumberHandler(client);

        var response = await handler.HandleAsync(
            new MoodleCourseFindByIdNumberRequest("46"),
            null!,
            CancellationToken.None);

        Assert.Equal("core_course_get_courses_by_field", response.Data?["function"]?.GetValue<string>());
        Assert.Equal("idnumber", response.Data?["payload"]?["field"]?.GetValue<string>());
        Assert.Equal("46", response.Data?["payload"]?["value"]?.GetValue<string>());
    }
}
