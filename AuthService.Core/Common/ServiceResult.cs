namespace AuthService.Core.Common;

public sealed record ServiceResult(bool Succeeded, string? Error)
{
    public static ServiceResult Success()
    {
        return new ServiceResult(true, null);
    }

    public static ServiceResult Failure(string error)
    {
        return new ServiceResult(false, error);
    }
}

public sealed record ServiceResult<T>(bool Succeeded, T? Value, string? Error)
{
    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(true, value, null);
    }

    public static ServiceResult<T> Failure(string error)
    {
        return new ServiceResult<T>(false, default, error);
    }
}
