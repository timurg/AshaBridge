using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
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
    public async Task CoreUserGetUsersByField_FindsOrCreatesConfiguredUser()
    {
        var userId = await ResolveUserIdAsync();
        Assert.True(userId > 0, "Moodle test user should exist or be created.");

        var response = await fixture.InvokeAsync(new MoodleGetUsersByFieldRequest(MoodleUserLookupField.Id, [userId.ToString()]));

        Assert.NotEmpty(response.Users);
    }

    [Fact]
    public async Task CoreUserGetUsers_SearchesConfiguredUser()
    {
        var response = await fixture.InvokeAsync(new MoodleGetUserRequest("email", fixture.Config.MoodleUserLookupValue));

        Assert.NotNull(response.User);
    }

    [Fact]
    public async Task CoreEnrolGetUsersCourses_UsesConfiguredToken_WhenUserIdIsConfigured()
    {
        var userId = await ResolveUserIdAsync();
        Assert.True(userId > 0, "Moodle test user should exist or be created.");

        await EnsureUserEnrolledAsync(userId);

        await fixture.InvokeAsync(new MoodleGetUsersCoursesRequest(userId));
    }

    [Fact]
    public async Task CoreCourseGetCourses_GetsConfiguredCourse()
    {
        var courseId = fixture.Config.MoodleCourseId;
        Assert.True(courseId > 0, "integrationTests:moodle:courseId must be configured.");

        var response = await fixture.InvokeAsync(new MoodleGetCoursesRequest([courseId]));

        var courses = response.Data?["items"]?.AsArray();
        Assert.NotNull(courses);
        Assert.Contains(courses, course => course?["id"]?.GetValue<long>() == courseId);
    }

    [Fact]
    public async Task CoreCourseGetCoursesByField_GetsConfiguredCourse()
    {
        var courseId = fixture.Config.MoodleCourseId;
        Assert.True(courseId > 0, "integrationTests:moodle:courseId must be configured.");

        var response = await fixture.InvokeAsync(new MoodleGetCoursesByFieldRequest("id", courseId.ToString()));

        var courses = response.Data?["courses"]?.AsArray();
        Assert.NotNull(courses);
        Assert.Contains(courses, course => course?["id"]?.GetValue<long>() == courseId);
    }

    [Fact]
    public async Task CoreCourseGetContents_GetsConfiguredCourseContents()
    {
        var courseId = fixture.Config.MoodleCourseId;
        Assert.True(courseId > 0, "integrationTests:moodle:courseId must be configured.");

        var response = await fixture.InvokeAsync(new MoodleGetCourseContentsRequest(courseId, null));

        Assert.NotNull(response.Data?["items"]?.AsArray());
    }

    [Fact]
    public async Task CoreCompletionGetActivitiesCompletionStatus_UsesConfiguredToken_WhenCourseAndUserAreConfigured()
    {
        var userId = await ResolveUserIdAsync();
        await EnsureUserEnrolledAsync(userId);

        await fixture.InvokeAsync(new MoodleGetActivitiesCompletionStatusRequest(fixture.Config.MoodleCourseId, userId));
    }

    [Fact]
    public async Task CoreCompletionGetCourseCompletionStatus_UsesConfiguredToken_WhenCourseAndUserAreConfigured()
    {
        var userId = await ResolveUserIdAsync();
        await EnsureUserEnrolledAsync(userId);

        await fixture.InvokeAsync(new MoodleGetCourseCompletionStatusRequest(fixture.Config.MoodleCourseId, userId));
    }

    [Fact]
    public async Task GradereportUserGetGradeItems_UsesConfiguredToken_WhenCourseAndUserAreConfigured()
    {
        var userId = await ResolveUserIdAsync();
        await EnsureUserEnrolledAsync(userId);

        await fixture.InvokeAsync(new MoodleGetGradeItemsRequest(fixture.Config.MoodleCourseId, userId));
    }

    private async Task<long> ResolveUserIdAsync()
    {
        if (fixture.Config.MoodleUserId > 0)
        {
            return fixture.Config.MoodleUserId;
        }

        var email = fixture.Config.MoodleUserLookupValue;
        Assert.False(string.IsNullOrWhiteSpace(email), "integrationTests:moodle:userLookupValue must contain the test user's email.");

        var existing = await fixture.InvokeAsync(new MoodleGetUsersByFieldRequest(MoodleUserLookupField.Email, [email]));

        var found = existing.Users.FirstOrDefault()?.Id ?? 0;
        if (found > 0)
        {
            return found;
        }

        var created = await fixture.InvokeAsync(new MoodleCreateUserRequest(
                Email: email,
                Password: CreateRandomPassword(),
                FirstName: "AshaBridge",
                LastName: "Integration Test"));

        Assert.True(created.Id > 0, "Moodle user should be created through AshaBridge contract.");
        return created.Id;
    }

    private async Task EnsureUserEnrolledAsync(long userId)
    {
        Assert.True(fixture.Config.MoodleCourseId > 0, "integrationTests:moodle:courseId must be configured.");
        Assert.True(userId > 0, "Moodle user id must be resolved before enrolment.");

        var courses = await fixture.InvokeAsync(new MoodleGetUsersCoursesRequest(userId));

        if (courses.Courses.Any(course => course.Id == fixture.Config.MoodleCourseId))
        {
            return;
        }

        await fixture.InvokeAsync(new MoodleManualEnrolUserRequest(
                RoleId: fixture.Config.MoodleStudentRoleId,
                UserId: userId,
                CourseId: fixture.Config.MoodleCourseId,
                TimeStart: null,
                TimeEnd: null,
                Suspend: null));
    }

    private static string CreateRandomPassword()
    {
        var suffix = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[^a-zA-Z0-9]", "");
        return $"Asha1!{suffix}";
    }
}


