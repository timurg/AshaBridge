namespace AshaBridge.Sdk.Contracts;

public abstract record AshaBridgeInvocationEvent
{
    public required string CorrelationId { get; init; }

    public required string MethodName { get; init; }

    public required DateTimeOffset Timestamp { get; init; }
}

public sealed record MethodStartedEvent : AshaBridgeInvocationEvent;

public sealed record ProgressEvent : AshaBridgeInvocationEvent
{
    public required string Message { get; init; }

    public double? Progress { get; init; }
}

public sealed record LogEvent : AshaBridgeInvocationEvent
{
    public required string Level { get; init; }

    public required string Message { get; init; }
}

public sealed record CacheHitEvent : AshaBridgeInvocationEvent;

public sealed record ExternalCallStartedEvent : AshaBridgeInvocationEvent
{
    public required string System { get; init; }

    public required string Operation { get; init; }
}

public sealed record ExternalCallCompletedEvent : AshaBridgeInvocationEvent;

public sealed record MethodCompletedEvent<TResponse> : AshaBridgeInvocationEvent
{
    public required TResponse Response { get; init; }
}

public sealed record MethodFailedEvent : AshaBridgeInvocationEvent
{
    public required AshaBridgeError Error { get; init; }
}

public sealed record AshaBridgeError(
    string Code,
    string Message,
    bool Retryable,
    string CorrelationId);
