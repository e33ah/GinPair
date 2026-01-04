namespace GinPair.Models;

public class ServiceResult {
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public BsColor BsColor { get; init; }

    public static ServiceResult Success(string message) {
        return new ServiceResult {
            IsSuccess = true,
            Message = message,
            BsColor = BsColor.Success
        };
    }

    public static ServiceResult Warning(string message) {
        return new ServiceResult {
            IsSuccess = false,
            Message = message,
            BsColor = BsColor.Warning
        };
    }

    public static ServiceResult Failure(string message) {
        return new ServiceResult {
            IsSuccess = false,
            Message = message,
            BsColor = BsColor.Danger
        };
    }
}

public class ServiceResult<T> : ServiceResult {
    public T? Data { get; init; }

    public static ServiceResult<T> Success(string message, T data) {
        return new ServiceResult<T> {
            IsSuccess = true,
            Message = message,
            BsColor = BsColor.Success,
            Data = data
        };
    }

    public static new ServiceResult<T> Warning(string message) {
        return new ServiceResult<T> {
            IsSuccess = false,
            Message = message,
            BsColor = BsColor.Warning
        };
    }

    public static new ServiceResult<T> Failure(string message) {
        return new ServiceResult<T> {
            IsSuccess = false,
            Message = message,
            BsColor = BsColor.Danger
        };
    }
}
