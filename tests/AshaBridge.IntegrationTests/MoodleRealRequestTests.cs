using AshaBridge.Extensions.Moodle.Contracts;

namespace AshaBridge.IntegrationTests;

public sealed class MoodleRealRequestTests : IClassFixture<AshaBridgeRuntimeFixture>
{
    private readonly AshaBridgeRuntimeFixture fixture;

    public MoodleRealRequestTests(AshaBridgeRuntimeFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public async Task UserFindById_FindsConfiguredUser()
    {
        var userId = await ResolveUserIdAsync();

        var response = await fixture.InvokeAsync(new MoodleFindUserByIdRequest(userId));

        Assert.True(response.Found);
        Assert.Equal(userId, response.User?.Id);
    }

    [Fact]
    public async Task UserFindByEmail_FindsConfiguredUser_WhenEmailIsConfigured()
    {
        var email = fixture.Config.MoodleUserLookupValue;
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var response = await fixture.InvokeAsync(new MoodleFindUserByEmailRequest(email));

        Assert.True(response.Found);
    }

    [Fact]
    public async Task CourseGetById_GetsConfiguredCourse()
    {
        var courseId = fixture.Config.MoodleCourseId;
        Assert.True(courseId > 0, "integrationTests:moodle:courseId must be configured.");

        var response = await fixture.InvokeAsync(new MoodleCourseGetByIdRequest(courseId));

        var courses = response.Data?["courses"]?.AsArray();
        Assert.NotNull(courses);
        Assert.Contains(courses, course => course?["id"]?.GetValue<long>() == courseId);
    }

    [Fact]
    public async Task CourseGetContents_GetsConfiguredCourseContents()
    {
        var courseId = fixture.Config.MoodleCourseId;
        Assert.True(courseId > 0, "integrationTests:moodle:courseId must be configured.");

        var response = await fixture.InvokeAsync(new MoodleCourseGetContentsRequest(courseId));

        Assert.NotNull(response.Data?["items"]?.AsArray());
    }

    [Fact]
    public async Task UserEnrol_UsesConfiguredRole_WhenWritesAreEnabled()
    {
        if (!fixture.Config.AllowWrites)
        {
            return;
        }

        var userId = await ResolveUserIdAsync();
        var courseId = fixture.Config.MoodleCourseId;
        Assert.True(courseId > 0, "integrationTests:moodle:courseId must be configured.");

        await fixture.InvokeAsync(new MoodleUserEnrolRequest(
            fixture.Config.MoodleStudentRoleId,
            userId,
            courseId));
    }

    private async Task<long> ResolveUserIdAsync()
    {
        if (fixture.Config.MoodleUserId > 0)
        {
            return fixture.Config.MoodleUserId;
        }

        var email = fixture.Config.MoodleUserLookupValue;
        Assert.False(string.IsNullOrWhiteSpace(email), "integrationTests:moodle:userLookupValue must contain an existing user's email.");

        var response = await fixture.InvokeAsync(new MoodleFindUserByEmailRequest(email));
        Assert.True(response.Found, "The configured Moodle test user must already exist.");
        return response.User!.Id;
    }
}
