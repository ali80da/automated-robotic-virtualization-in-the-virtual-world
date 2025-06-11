using Auto.Core.DataMo.Common;
using Auto.Core.DataMo.ServiceInfo;
using System.Net.Http;

namespace Auto.Core.Services.StatusChecker;

public interface IStatusCheckerService
{
    /// <summary>
    /// بررسی وضعیت یک سرویس.
    /// </summary>
    Task<StatusResult<ServiceStatusInfo>> CheckStatusAsync(ServiceInfo service);

    /// <summary>
    /// بررسی وضعیت چند سرویس به‌صورت موازی.
    /// </summary>
    Task<List<StatusResult<ServiceStatusInfo>>> CheckMultipleAsync(List<ServiceInfo> services);

    /// <summary>
    /// بررسی اینکه آیا سرویس مورد نظر در حال حاضر فعال است یا نه.
    /// </summary>
    Task<bool> IsServiceAvailableAsync(string serviceName);

    /// <summary>
    /// دریافت آخرین وضعیت ذخیره‌شده از حافظه (بدون فراخوانی مجدد).
    /// </summary>
    StatusResult<ServiceStatusInfo>? TryGetLastStatus(string serviceName);

    /// <summary>
    /// بررسی مجدد (Refresh) یک سرویس با نام مشخص.
    /// </summary>
    Task<StatusResult<ServiceStatusInfo>> RefreshAsync(string serviceName);

    /// <summary>
    /// دریافت لیست نام همه سرویس‌های ثبت‌شده.
    /// </summary>
    Task<List<string>> GetAllServiceNames();

    /// <summary>
    /// ثبت یک سرویس جدید به لیست مدیریت (به‌صورت غیرهمگام).
    /// </summary>
    Task RegisterServiceAsync(ServiceInfo info);


}

public class StatusCheckerService : IStatusCheckerService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly Dictionary<string, ServiceInfo> _registeredServices = new();
    private readonly Dictionary<string, StatusResult<ServiceStatusInfo>> _statusCache = new();
    public StatusCheckerService(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;

        // ثبت سرویس‌های پیش‌فرض
        var defaults = new List<ServiceInfo>
        {
            new() { Name = "n8n", Url = "http://localhost:5678" },
            new() { Name = "portainer", Url = "http://localhost:9000" },
            new() { Name = "grafana", Url = "http://localhost:3000" },
            new() { Name = "prometheus", Url = "http://localhost:9090" },
            new() { Name = "traefik", Url = "http://localhost:8080" },
            new() { Name = "minio", Url = "http://localhost:9001" },
            new() { Name = "redis-commander", Url = "http://localhost:8081" }
        };

        foreach (var service in defaults)
            _registeredServices[service.Name.ToLower()] = service;
    }


    /// <inheritdoc/>
    public async Task<StatusResult<ServiceStatusInfo>> CheckStatusAsync(ServiceInfo service)
    {
        if (service == null || string.IsNullOrWhiteSpace(service.Url))
            return StatusResult<ServiceStatusInfo>.Failed("اطلاعات سرویس ناقص است.");

        var client = httpClientFactory.CreateClient();

        try
        {
            using var response = await client.GetAsync(service.Url);

            var info = new ServiceStatusInfo
            {
                Name = service.Name,
                Url = service.Url,
                StatusCode = (int)response.StatusCode
            };

            var result = response.IsSuccessStatusCode
                ? StatusResult<ServiceStatusInfo>.Success(info, $"سرویس «{service.Name}» فعال است.", (int)response.StatusCode)
                : StatusResult<ServiceStatusInfo>.Warning($"سرویس «{service.Name}» پاسخ نداد.", info, (int)response.StatusCode);

            _statusCache[service.Name.ToLower()] = result;
            return result;
        }
        catch
        {
            var errorResult = StatusResult<ServiceStatusInfo>.Failed($"سرویس «{service.Name}» در دسترس نیست.", code: 503);
            _statusCache[service.Name.ToLower()] = errorResult;
            return errorResult;
        }
    }

    /// <inheritdoc/>
    public async Task<List<StatusResult<ServiceStatusInfo>>> CheckMultipleAsync(List<ServiceInfo> services)
    {
        var tasks = services.Select(CheckStatusAsync);
        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    /// <inheritdoc/>
    public async Task<bool> IsServiceAvailableAsync(string serviceName)
    {
        var status = await RefreshAsync(serviceName);
        return status.IsSuccess;
    }

    /// <inheritdoc/>
    public StatusResult<ServiceStatusInfo>? TryGetLastStatus(string serviceName)
    {
        _statusCache.TryGetValue(serviceName.ToLower(), out var result);
        return result;
    }

    /// <inheritdoc/>
    public async Task<StatusResult<ServiceStatusInfo>> RefreshAsync(string serviceName)
    {
        if (!_registeredServices.TryGetValue(serviceName.ToLower(), out var service))
            return StatusResult<ServiceStatusInfo>.Failed("سرویس ثبت نشده است.");

        return await CheckStatusAsync(service);
    }

    /// <inheritdoc/>
    public Task<List<string>> GetAllServiceNames()
    {
        return Task.FromResult(_registeredServices.Keys.ToList());
    }

    /// <summary>
    /// ثبت یک سرویس جدید به‌صورت غیرهمگام.
    /// در آینده می‌تواند شامل ذخیره در دیتابیس یا لاگ باشد.
    /// </summary>
    public async Task RegisterServiceAsync(ServiceInfo info)
    {
        if (info == null || string.IsNullOrWhiteSpace(info.Name) || string.IsNullOrWhiteSpace(info.Url))
            throw new ArgumentException("اطلاعات سرویس معتبر نیست.");

        // عملیات حافظه‌ای سریع
        _registeredServices[info.Name.ToLower()] = info;

        // در آینده:
        // await _repository.SaveAsync(info);
        // await File.WriteAllTextAsync("services.json", ...);
        // یا لاگ یا اعمال امنیتی

        await Task.CompletedTask; // برای حفظ async signature
    }



}
