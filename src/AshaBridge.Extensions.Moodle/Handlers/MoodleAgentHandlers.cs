using System.Text.Json.Nodes;
using AshaBridge.Extensions.Moodle.Contracts;
using AshaBridge.Sdk.Contracts;

namespace AshaBridge.Extensions.Moodle.Handlers;

public sealed class MoodleFindUserByEmailHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleFindUserByEmailRequest, MoodleFindUserResponse>
{
    public Task<MoodleFindUserResponse> HandleAsync(MoodleFindUserByEmailRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindUserAsync(client, "email", request.Email, ct);
}

public sealed class MoodleFindUserByIdHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleFindUserByIdRequest, MoodleFindUserResponse>
{
    public Task<MoodleFindUserResponse> HandleAsync(MoodleFindUserByIdRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindUserAsync(client, "id", request.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), ct);
}

public sealed class MoodleFindUserByUsernameHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleFindUserByUsernameRequest, MoodleFindUserResponse>
{
    public Task<MoodleFindUserResponse> HandleAsync(MoodleFindUserByUsernameRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindUserAsync(client, "username", request.Username, ct);
}

public sealed class MoodleUserUpdateNameHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserUpdateNameRequest, MoodleUpdateUserResponse>
{
    public Task<MoodleUpdateUserResponse> HandleAsync(MoodleUserUpdateNameRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.UpdateUserAsync(client, request.Id, new JsonObject { ["firstname"] = request.FirstName, ["lastname"] = request.LastName }, ct);
}

public sealed class MoodleUserUpdateEmailHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserUpdateEmailRequest, MoodleUpdateUserResponse>
{
    public Task<MoodleUpdateUserResponse> HandleAsync(MoodleUserUpdateEmailRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.UpdateUserAsync(client, request.Id, new JsonObject { ["email"] = request.Email }, ct);
}

public sealed class MoodleUserUpdateUsernameHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserUpdateUsernameRequest, MoodleUpdateUserResponse>
{
    public Task<MoodleUpdateUserResponse> HandleAsync(MoodleUserUpdateUsernameRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.UpdateUserAsync(client, request.Id, new JsonObject { ["username"] = request.Username }, ct);
}

public sealed class MoodleUserUpdatePasswordHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserUpdatePasswordRequest, MoodleUpdateUserResponse>
{
    public Task<MoodleUpdateUserResponse> HandleAsync(MoodleUserUpdatePasswordRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.UpdateUserAsync(client, request.Id, new JsonObject { ["password"] = request.Password }, ct);
}

public sealed class MoodleUserSuspendHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserSuspendRequest, MoodleUpdateUserResponse>
{
    public Task<MoodleUpdateUserResponse> HandleAsync(MoodleUserSuspendRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.UpdateUserAsync(client, request.Id, new JsonObject { ["suspended"] = 1 }, ct);
}

public sealed class MoodleUserUnsuspendHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserUnsuspendRequest, MoodleUpdateUserResponse>
{
    public Task<MoodleUpdateUserResponse> HandleAsync(MoodleUserUnsuspendRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.UpdateUserAsync(client, request.Id, new JsonObject { ["suspended"] = 0 }, ct);
}

public sealed class MoodleRequestPasswordResetByEmailHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleRequestPasswordResetByEmailRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleRequestPasswordResetByEmailRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        new((await client.CallAsync("core_auth_request_password_reset", new JsonObject { ["email"] = request.Email }, ct).ConfigureAwait(false)).DeepClone());
}

public sealed class MoodleRequestPasswordResetByUsernameHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleRequestPasswordResetByUsernameRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleRequestPasswordResetByUsernameRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        new((await client.CallAsync("core_auth_request_password_reset", new JsonObject { ["username"] = request.Username }, ct).ConfigureAwait(false)).DeepClone());
}

public sealed class MoodleUserEnrolHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserEnrolRequest, MoodleRawResponse>
{
    public Task<MoodleRawResponse> HandleAsync(MoodleUserEnrolRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.EnrolUserAsync(client, request.RoleId, request.UserId, request.CourseId, ct);
}

public sealed class MoodleUserEnrolAsStudentHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleUserEnrolAsStudentRequest, MoodleRawResponse>
{
    private const long StudentRoleId = 5;

    public Task<MoodleRawResponse> HandleAsync(MoodleUserEnrolAsStudentRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.EnrolUserAsync(client, StudentRoleId, request.UserId, request.CourseId, ct);
}

public sealed class MoodleCourseGetByIdHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleCourseGetByIdRequest, MoodleRawResponse>
{
    public Task<MoodleRawResponse> HandleAsync(MoodleCourseGetByIdRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindCoursesByFieldAsync(
            client,
            "id",
            request.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ct);
}

public sealed class MoodleCourseFindByShortNameHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleCourseFindByShortNameRequest, MoodleRawResponse>
{
    public Task<MoodleRawResponse> HandleAsync(MoodleCourseFindByShortNameRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindCoursesByFieldAsync(client, "shortname", request.ShortName, ct);
}

public sealed class MoodleCourseFindByIdNumberHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleCourseFindByIdNumberRequest, MoodleRawResponse>
{
    public Task<MoodleRawResponse> HandleAsync(MoodleCourseFindByIdNumberRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindCoursesByFieldAsync(client, "idnumber", request.IdNumber, ct);
}

public sealed class MoodleCoursesFindByCategoryHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleCoursesFindByCategoryRequest, MoodleRawResponse>
{
    public Task<MoodleRawResponse> HandleAsync(MoodleCoursesFindByCategoryRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        MoodleAgentHandlerHelpers.FindCoursesByFieldAsync(client, "category", request.Category, ct);
}

public sealed class MoodleCourseGetContentsHandler(MoodleWebServiceClient client)
    : IMcpMethodHandler<MoodleCourseGetContentsRequest, MoodleRawResponse>
{
    public async Task<MoodleRawResponse> HandleAsync(MoodleCourseGetContentsRequest request, IAshaBridgeExecutionContext execution, CancellationToken ct) =>
        new((await client.CallAsync("core_course_get_contents", new JsonObject { ["courseid"] = request.CourseId }, ct).ConfigureAwait(false)).DeepClone());
}

internal static class MoodleAgentHandlerHelpers
{
    public static async Task<MoodleFindUserResponse> FindUserAsync(MoodleWebServiceClient client, string field, string value, CancellationToken ct)
    {
        var result = await client.CallAsync("core_user_get_users", new JsonObject
        {
            ["criteria"] = new JsonArray { new JsonObject { ["key"] = field, ["value"] = value } }
        }, ct).ConfigureAwait(false);

        var user = result["users"]?.AsArray()
            .Select(item => new MoodleUser(
                item?["id"]?.GetValue<long>() ?? 0,
                item?["username"]?.GetValue<string>(),
                item?["email"]?.GetValue<string>(),
                item?["fullname"]?.GetValue<string>()))
            .FirstOrDefault(item => item.Id > 0);

        return new MoodleFindUserResponse(user is not null, field, value, user);
    }

    public static async Task<MoodleUpdateUserResponse> UpdateUserAsync(MoodleWebServiceClient client, long id, JsonObject fields, CancellationToken ct)
    {
        fields["id"] = id;
        await client.CallAsync("core_user_update_users", new JsonObject
        {
            ["users"] = new JsonArray { fields }
        }, ct).ConfigureAwait(false);

        return new MoodleUpdateUserResponse(true);
    }

    public static async Task<MoodleRawResponse> EnrolUserAsync(MoodleWebServiceClient client, long roleId, long userId, long courseId, CancellationToken ct)
    {
        var result = await client.CallAsync("enrol_manual_enrol_users", new JsonObject
        {
            ["enrolments"] = new JsonArray
            {
                new JsonObject
                {
                    ["roleid"] = roleId,
                    ["userid"] = userId,
                    ["courseid"] = courseId
                }
            }
        }, ct).ConfigureAwait(false);

        return new MoodleRawResponse(result.DeepClone());
    }

    public static async Task<MoodleRawResponse> FindCoursesByFieldAsync(MoodleWebServiceClient client, string field, string value, CancellationToken ct)
    {
        var result = await client.CallAsync("core_course_get_courses_by_field", new JsonObject
        {
            ["field"] = field,
            ["value"] = value
        }, ct).ConfigureAwait(false);

        return new MoodleRawResponse(result.DeepClone());
    }
}
