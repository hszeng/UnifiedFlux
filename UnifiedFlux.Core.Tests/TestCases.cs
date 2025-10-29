using UnifiedFlux.Core;
using System.Threading;
using System.Threading.Tasks;

// --- Request/Response Example ---

public class PingRequest : IUnifiedRequest<string>
{
    public string Message { get; set; }
}

public class PingRequestHandler : IUnifiedRequestHandler<PingRequest, string>
{
    public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Pong: {request.Message}");
    }
}

// --- Notification Example ---

public class UserCreatedNotification : IUnifiedNotification
{
    public int UserId { get; set; }
}

public class UserLogHandler : IUnifiedNotificationHandler<UserCreatedNotification>
{
    // Used to record whether it was called
    public bool WasCalled { get; private set; } 
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.CompletedTask;
    }
}

public class UserEmailHandler : IUnifiedNotificationHandler<UserCreatedNotification>
{
    // Used to record whether it was called
    public bool WasCalled { get; private set; } 
    public Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.CompletedTask;
    }
}

// Asynchronous notification handler (used to test parallelism)
public class AsyncNotificationHandler : IUnifiedNotificationHandler<UserCreatedNotification>
{
    private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
    public TaskCompletionSource<bool> Tcs => _tcs;

    public async Task Handle(UserCreatedNotification notification, CancellationToken cancellationToken)
    {
        await Task.Delay(50); // Simulate a time-consuming operation
        _tcs.SetResult(true);
    }
}