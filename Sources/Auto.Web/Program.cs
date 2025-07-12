using Auto.Core.DataMo.ServiceInfo;
using Auto.Core.Services.Docker;
using Auto.Core.Services.StatusChecker;
using Auto.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);
{


    builder.Services.AddControllersWithViews();


    builder.Services.AddHttpClient();
    builder.Services.AddSignalR();


    builder.Services.AddScoped<IStatusCheckerService, StatusCheckerService>();
    builder.Services.AddSingleton<IDockerService, DockerService>();


}
var app = builder.Build();
{
    #region Environment Config

    if (app.Environment.IsDevelopment())
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkBlue;

        Console.WriteLine("Environment: IsDevelopment");
        app.UseDeveloperExceptionPage();
    }
    else if (app.Environment.IsStaging())
    {
        Console.WriteLine("Environment: IsStaging");
    }
    else if (app.Environment.IsProduction())
    {
        Console.WriteLine("Environment: IsProduction");

        app.UseHsts();
    }

    #endregion

    app.Lifetime.ApplicationStarted.Register(async () =>
    {
        var scope = app.Services.CreateScope();
        var checker = scope.ServiceProvider.GetRequiredService<IStatusCheckerService>();

        var services = new List<ServiceInfo>
    {
        new() { Name = "n8n", Url = "http://localhost:5678" },
        new() { Name = "portainer", Url = "http://localhost:9000" },
        new() { Name = "grafana", Url = "http://localhost:3000" },
        new() { Name = "prometheus", Url = "http://localhost:9090" },
        new() { Name = "traefik", Url = "http://localhost:8080" },
        new() { Name = "minio", Url = "http://localhost:9001" },
        new() { Name = "redis-commander", Url = "http://localhost:8081" },
    };

        foreach (var service in services)
        {
            await checker.RegisterServiceAsync(service);
        }
    });

    app.UseStaticFiles();
    app.UseRouting();

    //app.UseCors("AllowAll");
    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();


    app.MapControllers();

    app.MapHub<DockerHub>("/dockerhub");
}
app.Run();
