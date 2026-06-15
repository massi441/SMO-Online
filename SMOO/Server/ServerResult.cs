namespace SMOO.Server;

internal readonly struct ServerResult
{
    private readonly ServerError? _error;

    public ServerError? Error => _error;

    public bool IsSuccess => _error == null;
    public bool IsFailed => _error != null;

    private ServerResult(ServerError? error)
    {
        _error = error;
    }

    public static ServerResult Success() => new ServerResult(null);
    public static ServerResult Failure(ServerError error) => new ServerResult(error);
}

internal readonly struct ServerResult<T>
{
    private readonly T? _data;
    private readonly ServerError? _error;

    public T? Data => _data;
    public ServerError? Error => _error;

    public bool IsSuccess => _error == null;
    public bool IsFailed => _error != null;

    private ServerResult(T data)
    {
        _data = data;
        _error = null;
    }

    private ServerResult(ServerError error)
    {
        _error = error;
        _data = default;
    }

    public static ServerResult<T> Success(T data) => new ServerResult<T>(data);
    public static ServerResult<T> Failure(ServerError error) => new ServerResult<T>(error);
}
