using System.Text.Json.Nodes;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Moodle.Handlers;

public sealed class MoodleGetUsersByFieldHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetUsersByFieldRequest, MoodleGetUsersByFieldResponse>
{
    public async Task<MoodleGetUsersByFieldResponse> HandleAsync(MoodleGetUsersByFieldRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_user_get_users_by_field", new JsonObject { ["field"] = ToMoodleField(request.Field), ["values"] = new JsonArray(request.Values.Select(v => JsonValue.Create(v)).ToArray()) }, ct);
        var users = result["items"]?.AsArray()
            .Select(user => new MoodleUser(
                user?["id"]?.GetValue<long>() ?? 0,
                user?["username"]?.GetValue<string>(),
                user?["email"]?.GetValue<string>(),
                user?["fullname"]?.GetValue<string>()))
            .Where(user => user.Id > 0)
            .ToArray() ?? [];

        return new MoodleGetUsersByFieldResponse(users);
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

public sealed class MoodleCreateUserHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleCreateUserRequest, MoodleCreateUserResponse>
{
    public async Task<MoodleCreateUserResponse> HandleAsync(MoodleCreateUserRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_user_create_users", new JsonObject
        {
            ["users"] = new JsonArray
            {
                new JsonObject
                {
                    ["username"] = request.Email,
                    ["email"] = request.Email,
                    ["password"] = request.Password,
                    ["firstname"] = request.FirstName,
                    ["lastname"] = request.LastName
                }
            }
        }, ct);

        var created = result["items"]?.AsArray().FirstOrDefault()?.AsObject()
            ?? throw new InvalidOperationException("Moodle did not return created user details.");

        return new MoodleCreateUserResponse(
            created["id"]?.GetValue<long>() ?? 0,
            created["username"]?.GetValue<string>() ?? request.Email);
    }
}

public sealed class MoodleUpdateUserHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUpdateUserRequest, MoodleUpdateUserResponse>
{
    public async Task<MoodleUpdateUserResponse> HandleAsync(MoodleUpdateUserRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var user = new JsonObject { ["id"] = request.Id };
        AddIfNotNull(user, "username", request.Username);
        AddIfNotNull(user, "auth", request.Auth);
        AddIfNotNull(user, "suspended", request.Suspended);
        AddIfNotNull(user, "password", request.Password);
        AddIfNotNull(user, "firstname", request.FirstName);
        AddIfNotNull(user, "lastname", request.LastName);
        AddIfNotNull(user, "email", request.Email);
        AddIfNotNull(user, "maildisplay", request.MailDisplay);
        AddIfNotNull(user, "city", request.City);
        AddIfNotNull(user, "country", request.Country);
        AddIfNotNull(user, "timezone", request.Timezone);
        AddIfNotNull(user, "description", request.Description);
        AddIfNotNull(user, "idnumber", request.IdNumber);
        AddIfNotNull(user, "institution", request.Institution);
        AddIfNotNull(user, "department", request.Department);
        AddIfNotNull(user, "phone1", request.Phone1);
        AddIfNotNull(user, "phone2", request.Phone2);
        AddIfNotNull(user, "address", request.Address);
        AddIfNotNull(user, "lang", request.Lang);

        await client.CallAsync("core_user_update_users", new JsonObject
        {
            ["users"] = new JsonArray { user }
        }, ct);

        return new MoodleUpdateUserResponse(true);
    }

    private static void AddIfNotNull(JsonObject target, string name, string? value)
    {
        if (value is not null)
        {
            target[name] = value;
        }
    }

    private static void AddIfNotNull(JsonObject target, string name, int? value)
    {
        if (value is not null)
        {
            target[name] = value;
        }
    }

    private static void AddIfNotNull(JsonObject target, string name, bool? value)
    {
        if (value is not null)
        {
            target[name] = value.Value ? 1 : 0;
        }
    }
}

public sealed class MoodleGetUserHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetUserRequest, MoodleGetUserResponse>
{
    public async Task<MoodleGetUserResponse> HandleAsync(MoodleGetUserRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_user_get_users", new JsonObject
        {
            ["criteria"] = new JsonArray
            {
                new JsonObject
                {
                    ["key"] = request.Key,
                    ["value"] = request.Value
                }
            }
        }, ct);

        var user = result["users"]?.AsArray()
            .Select(item => new MoodleUser(
                item?["id"]?.GetValue<long>() ?? 0,
                item?["username"]?.GetValue<string>(),
                item?["email"]?.GetValue<string>(),
                item?["fullname"]?.GetValue<string>()))
            .FirstOrDefault(item => item.Id > 0);

        return new MoodleGetUserResponse(user);
    }
}

public sealed class MoodleRequestPasswordResetHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleRequestPasswordResetRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleRequestPasswordResetRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var payload = new JsonObject();
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            payload["username"] = request.Username;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            payload["email"] = request.Email;
        }

        var result = await client.CallAsync("core_auth_request_password_reset", payload, ct);
        return new MoodleRawResponse(result.DeepClone());
    }
}

public sealed class MoodleGetUsersCoursesHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetUsersCoursesRequest, MoodleGetUsersCoursesResponse>
{
    public async Task<MoodleGetUsersCoursesResponse> HandleAsync(MoodleGetUsersCoursesRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_enrol_get_users_courses", new JsonObject { ["userid"] = request.UserId }, ct);
        var courses = result["items"]?.AsArray()
            .Select(course => new MoodleCourse(
                course?["id"]?.GetValue<long>() ?? 0,
                course?["shortname"]?.GetValue<string>(),
                course?["fullname"]?.GetValue<string>()))
            .Where(course => course.Id > 0)
            .ToArray() ?? [];

        return new MoodleGetUsersCoursesResponse(courses);
    }
}

public sealed class MoodleManualEnrolUserHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleManualEnrolUserRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleManualEnrolUserRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var enrolment = new JsonObject
        {
            ["roleid"] = request.RoleId,
            ["userid"] = request.UserId,
            ["courseid"] = request.CourseId
        };

        AddIfNotNull(enrolment, "timestart", request.TimeStart);
        AddIfNotNull(enrolment, "timeend", request.TimeEnd);
        AddIfNotNull(enrolment, "suspend", request.Suspend);

        var result = await client.CallAsync("enrol_manual_enrol_users", new JsonObject
        {
            ["enrolments"] = new JsonArray { enrolment }
        }, ct);

        return new MoodleRawResponse(result.DeepClone());
    }

    private static void AddIfNotNull(JsonObject target, string name, long? value)
    {
        if (value is not null)
        {
            target[name] = value;
        }
    }

    private static void AddIfNotNull(JsonObject target, string name, int? value)
    {
        if (value is not null)
        {
            target[name] = value;
        }
    }
}

public sealed class MoodleGetActivitiesCompletionStatusHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetActivitiesCompletionStatusRequest, MoodleGetActivitiesCompletionStatusResponse>
{
    public async Task<MoodleGetActivitiesCompletionStatusResponse> HandleAsync(MoodleGetActivitiesCompletionStatusRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_completion_get_activities_completion_status", new JsonObject { ["courseid"] = request.CourseId, ["userid"] = request.UserId }, ct);
        var completions = result["statuses"]?.AsArray()
            .Select(status => new MoodleActivityCompletion(
                status?["cmid"]?.GetValue<long>() ?? 0,
                status?["state"]?.GetValue<int>() ?? 0,
                status?["timecompleted"]?.ToString()))
            .Where(status => status.Cmid > 0)
            .ToArray() ?? [];

        return new MoodleGetActivitiesCompletionStatusResponse(completions);
    }
}

public sealed class MoodleGetCourseCompletionStatusHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetCourseCompletionStatusRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleGetCourseCompletionStatusRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_completion_get_course_completion_status", new JsonObject { ["courseid"] = request.CourseId, ["userid"] = request.UserId }, ct);
        return new MoodleRawResponse(result.DeepClone());
    }
}

public sealed class MoodleListUserPlansHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleListUserPlansRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleListUserPlansRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("core_competency_list_user_plans", new JsonObject { ["userid"] = request.UserId }, ct);
        return new MoodleRawResponse(result.DeepClone());
    }
}

public sealed class MoodleGetGradeItemsHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetGradeItemsRequest, MoodleGetGradeItemsResponse>
{
    public async Task<MoodleGetGradeItemsResponse> HandleAsync(MoodleGetGradeItemsRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var result = await client.CallAsync("gradereport_user_get_grade_items", new JsonObject { ["courseid"] = request.CourseId, ["userid"] = request.UserId }, ct);
        var items = result["usergrades"]?.AsArray()
            .SelectMany(userGrade => userGrade?["gradeitems"]?.AsArray() ?? [])
            .Select(item => new MoodleGradeItem(
                item?["itemname"]?.GetValue<string>(),
                item?["graderaw"]?.ToString(),
                item?["gradeformatted"]?.GetValue<string>()))
            .ToArray() ?? [];

        return new MoodleGetGradeItemsResponse(items);
    }
}

public sealed class MoodleGetCoursesHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetCoursesRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleGetCoursesRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var options = new JsonObject();
        if (request.Ids is { Count: > 0 })
        {
            options["ids"] = new JsonArray(request.Ids.Select(id => JsonValue.Create(id)).ToArray());
        }

        var result = await client.CallAsync("core_course_get_courses", new JsonObject { ["options"] = options }, ct);
        return new MoodleRawResponse(result.DeepClone());
    }
}

public sealed class MoodleGetCoursesByFieldHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetCoursesByFieldRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleGetCoursesByFieldRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var payload = new JsonObject();
        if (!string.IsNullOrWhiteSpace(request.Field))
        {
            payload["field"] = request.Field;
        }

        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            payload["value"] = request.Value;
        }

        var result = await client.CallAsync("core_course_get_courses_by_field", payload, ct);
        return new MoodleRawResponse(result.DeepClone());
    }
}

public sealed class MoodleGetCourseContentsHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleGetCourseContentsRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleGetCourseContentsRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct)
    {
        var payload = new JsonObject { ["courseid"] = request.CourseId };
        if (request.Options is { Count: > 0 })
        {
            payload["options"] = new JsonArray(request.Options.Select(o => new JsonObject
            {
                ["name"] = o.Name,
                ["value"] = o.Value
            }).ToArray());
        }

        var result = await client.CallAsync("core_course_get_contents", payload, ct);
        return new MoodleRawResponse(result.DeepClone());
    }
}
