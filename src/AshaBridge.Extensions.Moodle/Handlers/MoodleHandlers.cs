using System.Text.Json.Nodes;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Moodle.Handlers;

public sealed class MoodleGetUsersByFieldHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse>
{
    public async Task<MoodleGetUsersByFieldResponse> HandleAsync(MoodleGetUsersByFieldRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        await client.CallAsync("core_user_get_users_by_field", new JsonObject { ["field"] = ToMoodleField(request.Field), ["values"] = new JsonArray(request.Values.Select(v => JsonValue.Create(v)).ToArray()) }, ct);
        return new MoodleGetUsersByFieldResponse([]);
    }

    private static string ToMoodleField(MoodleUserLookupField field) => field switch
    {
        MoodleUserLookupField.Id => "id",
        MoodleUserLookupField.IdNumber => "idnumber",
        MoodleUserLookupField.Username => "username",
        MoodleUserLookupField.Email => "email",
        _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
    };
}

public sealed class MoodleGetUsersCoursesHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse>
{
    public async Task<MoodleGetUsersCoursesResponse> HandleAsync(MoodleGetUsersCoursesRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        await client.CallAsync("core_enrol_get_users_courses", new JsonObject { ["userid"] = request.UserId }, ct);
        return new MoodleGetUsersCoursesResponse([]);
    }
}

public sealed class MoodleGetActivitiesCompletionStatusHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetActivitiesCompletionStatusRequest, MoodleGetActivitiesCompletionStatusResponse>
{
    public async Task<MoodleGetActivitiesCompletionStatusResponse> HandleAsync(MoodleGetActivitiesCompletionStatusRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        await client.CallAsync("core_completion_get_activities_completion_status", new JsonObject { ["courseid"] = request.CourseId, ["userid"] = request.UserId }, ct);
        return new MoodleGetActivitiesCompletionStatusResponse([]);
    }
}

public sealed class MoodleGetGradeItemsHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetGradeItemsRequest, MoodleGetGradeItemsResponse>
{
    public async Task<MoodleGetGradeItemsResponse> HandleAsync(MoodleGetGradeItemsRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        await client.CallAsync("gradereport_user_get_grade_items", new JsonObject { ["courseid"] = request.CourseId, ["userid"] = request.UserId }, ct);
        return new MoodleGetGradeItemsResponse([]);
    }
}
