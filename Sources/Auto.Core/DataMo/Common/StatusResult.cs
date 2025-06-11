using System;

namespace Auto.Core.DataMo.Common;

/// <summary>
/// مدل استاندارد وضعیت نتیجه یک عملیات یا سرویس (نسخه غیر جنریک)
/// </summary>
public class StatusResult
{
    public StatusType Status { get; set; } = StatusType.Unknown;
    public string Message { get; set; } = "وضعیت مشخص نشده است.";
    public int? Code { get; set; }
    public object? Data { get; set; }

    public bool IsSuccess => Status == StatusType.Success;
    public bool IsWarning => Status == StatusType.Warning;
    public bool IsFailed => Status == StatusType.Failed;

    public static StatusResult Success(string message = "عملیات موفقیت‌آمیز بود.", object? data = null, int? code = 0) =>
        new() { Status = StatusType.Success, Message = message, Data = data, Code = code };

    public static StatusResult Failed(string message = "عملیات با شکست مواجه شد.", object? data = null, int? code = null) =>
        new() { Status = StatusType.Failed, Message = message, Data = data, Code = code };

    public static StatusResult Warning(string message = "هشدار در عملیات.", object? data = null, int? code = null) =>
        new() { Status = StatusType.Warning, Message = message, Data = data, Code = code };

    public static StatusResult Unknown(string message = "وضعیت نامشخص است.", object? data = null, int? code = null) =>
        new() { Status = StatusType.Unknown, Message = message, Data = data, Code = code };
}

/// <summary>
/// نسخه جنریک از StatusResult برای عملیات با خروجی تایپ‌شده
/// </summary>
public class StatusResult<T>
{
    public StatusType Status { get; set; } = StatusType.Unknown;
    public string Message { get; set; } = "وضعیت مشخص نشده است.";
    public int? Code { get; set; }
    public T? Data { get; set; }

    public bool IsSuccess => Status == StatusType.Success;
    public bool IsWarning => Status == StatusType.Warning;
    public bool IsFailed => Status == StatusType.Failed;

    public static StatusResult<T> Success(T data, string message = "عملیات موفقیت‌آمیز بود.", int? code = 0) =>
        new() { Status = StatusType.Success, Message = message, Data = data, Code = code };

    public static StatusResult<T> Failed(string message = "عملیات با شکست مواجه شد.", T? data = default, int? code = null) =>
        new() { Status = StatusType.Failed, Message = message, Data = data, Code = code };

    public static StatusResult<T> Warning(string message = "هشدار در عملیات.", T? data = default, int? code = null) =>
        new() { Status = StatusType.Warning, Message = message, Data = data, Code = code };

    public static StatusResult<T> Unknown(string message = "وضعیت نامشخص است.", T? data = default, int? code = null) =>
        new() { Status = StatusType.Unknown, Message = message, Data = data, Code = code };

    /// <summary>
    /// تبدیل از StatusResult عمومی به نسخه جنریک (در صورت امکان)
    /// </summary>
    public static StatusResult<T> FromBase(StatusResult baseResult)
    {
        return new StatusResult<T>
        {
            Status = baseResult.Status,
            Message = baseResult.Message,
            Code = baseResult.Code,
            Data = baseResult.Data is T d ? d : default
        };
    }
}

/// <summary>
/// انواع وضعیت‌های منطقی
/// </summary>
public enum StatusType
{
    Success,
    Failed,
    Warning,
    Unknown
}