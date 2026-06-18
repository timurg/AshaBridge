using System.Runtime.CompilerServices;
using AshaBridge.Core.Registry;
using AshaBridge.Sdk.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AshaBridge.Core.Runtime;

public sealed class StreamingInvocationRuntime(
    MethodRegistry methods,
    ILogger<StreamingInvocationRuntime> logger)
{
    public async IAsyncEnumerable<AshaBridgeInvocationEvent> InvokeAsync(
        string methodName,
        object request,
        IAshaBridgeExecutionContext execution,
        [EnumeratorCancellation] CancellationToken ct)
    {
        if (!methods.TryGet(methodName, out var method))
        {
            yield return Failed(methodName, execution.CorrelationId, "METHOD_NOT_FOUND", $"MCP method '{methodName}' was not found.");
            yield break;
        }

        yield return Started(methodName, execution.CorrelationId);

        foreach (var permission in method.Permissions)
        {
            if (!execution.Permissions.Contains(permission, StringComparer.Ordinal))
            {
                yield return Failed(methodName, execution.CorrelationId, "PERMISSION_DENIED", $"Permission is required: {permission}");
                yield break;
            }
        }

        if (method.RequiresIdempotency && execution.IdempotencyKey is null)
        {
            yield return Failed(methodName, execution.CorrelationId, "IDEMPOTENCY_REQUIRED", "Idempotency key is required for this method.");
            yield break;
        }

        if (method.IsStreaming)
        {
            var stream = InvokeStreamingHandler(method, request, execution, ct);
            await foreach (var @event in stream.WithCancellation(ct))
            {
                yield return @event;
            }

            yield break;
        }

        object? response = null;
        AshaBridgeInvocationEvent? failure = null;
        try
        {
            response = await InvokeHandlerAsync(method, request, execution, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "AshaBridge method {MethodName} was cancelled. CorrelationId={CorrelationId}; RequestType={RequestType}",
                methodName,
                execution.CorrelationId,
                request.GetType().Name);
            failure = Failed(methodName, execution.CorrelationId, "TIMEOUT", "Invocation was cancelled.");
        }
        catch (ExternalServiceException ex)
        {
            logger.LogError(
                ex,
                "External service call failed. MethodName={MethodName}; CorrelationId={CorrelationId}; Source={Source}; Operation={Operation}; ErrorKind={ErrorKind}; StatusCode={StatusCode}; Retryable={Retryable}",
                methodName,
                execution.CorrelationId,
                ex.Service,
                ex.Operation,
                ex.Kind,
                ex.StatusCode,
                ex.Retryable);

            failure = Failed(
                methodName,
                execution.CorrelationId,
                $"UPSTREAM_{ToErrorCode(ex.Kind)}",
                ex.Message,
                ex.Retryable,
                ex.Service,
                ex.Operation,
                ex.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "AshaBridge method {MethodName} threw an exception. CorrelationId={CorrelationId}; RequestType={RequestType}",
                methodName,
                execution.CorrelationId,
                request.GetType().Name);
            failure = Failed(methodName, execution.CorrelationId, "UNKNOWN_ERROR", ex.Message);
        }

        if (failure is not null)
        {
            yield return failure;
            yield break;
        }

        yield return Completed(method, execution.CorrelationId, response!);
    }

    private static Task<object> InvokeHandlerAsync(
        McpMethodDescriptor method,
        object request,
        IAshaBridgeExecutionContext execution,
        CancellationToken ct)
    {
        var handler = execution.Services.GetRequiredService(method.HandlerType);
        var interfaceType = typeof(IMcpMethodHandler<,>).MakeGenericType(method.RequestType, method.ResponseType);
        var handle = interfaceType.GetMethod(nameof(IMcpMethodHandler<IMcpRequest<object>, object>.HandleAsync))!;
        var task = (Task)handle.Invoke(handler, [request, execution, ct])!;
        return AwaitBoxedAsync(task);
    }

    private static IAsyncEnumerable<AshaBridgeInvocationEvent> InvokeStreamingHandler(
        McpMethodDescriptor method,
        object request,
        IAshaBridgeExecutionContext execution,
        CancellationToken ct)
    {
        var handler = execution.Services.GetRequiredService(method.HandlerType);
        var interfaceType = typeof(IStreamingMcpMethodHandler<,>).MakeGenericType(method.RequestType, method.ResponseType);
        var handle = interfaceType.GetMethod(nameof(IStreamingMcpMethodHandler<IMcpRequest<object>, object>.HandleStreamAsync))!;
        return (IAsyncEnumerable<AshaBridgeInvocationEvent>)handle.Invoke(handler, [request, execution, ct])!;
    }

    private static async Task<object> AwaitBoxedAsync(Task task)
    {
        await task.ConfigureAwait(false);
        return task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private static MethodStartedEvent Started(string methodName, string correlationId) =>
        new()
        {
            CorrelationId = correlationId,
            MethodName = methodName,
            Timestamp = DateTimeOffset.UtcNow
        };

    private static AshaBridgeInvocationEvent Completed(McpMethodDescriptor method, string correlationId, object response)
    {
        var eventType = typeof(MethodCompletedEvent<>).MakeGenericType(method.ResponseType);
        var completed = (AshaBridgeInvocationEvent)Activator.CreateInstance(eventType)!;
        eventType.GetProperty(nameof(AshaBridgeInvocationEvent.CorrelationId))!.SetValue(completed, correlationId);
        eventType.GetProperty(nameof(AshaBridgeInvocationEvent.MethodName))!.SetValue(completed, method.Name);
        eventType.GetProperty(nameof(AshaBridgeInvocationEvent.Timestamp))!.SetValue(completed, DateTimeOffset.UtcNow);
        eventType.GetProperty("Response")!.SetValue(completed, response);
        return completed;
    }

    private static MethodFailedEvent Failed(
        string methodName,
        string correlationId,
        string code,
        string message,
        bool retryable = false,
        string? source = null,
        string? operation = null,
        int? statusCode = null) =>
        new()
        {
            CorrelationId = correlationId,
            MethodName = methodName,
            Timestamp = DateTimeOffset.UtcNow,
            Error = new AshaBridgeError(code, message, retryable, correlationId, source, operation, statusCode)
        };

    private static string ToErrorCode(ExternalServiceErrorKind kind) => kind switch
    {
        ExternalServiceErrorKind.Transport => "TRANSPORT_ERROR",
        ExternalServiceErrorKind.Http => "HTTP_ERROR",
        ExternalServiceErrorKind.Api => "API_ERROR",
        ExternalServiceErrorKind.InvalidResponse => "INVALID_RESPONSE",
        _ => "ERROR"
    };
}
