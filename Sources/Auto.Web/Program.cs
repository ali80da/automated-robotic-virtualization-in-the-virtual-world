using Auto.Core.DataMo.ServiceInfo;
using Auto.Core.Services.Docker;
using Auto.Core.Services.StatusChecker;
using Auto.Web.Hubs;
using Auto.Web.Middlewares.Docker;

var builder = WebApplication.CreateBuilder(args);
{
    Console.WriteLine("217 133 216 172 216 167 216 178 219 140 226 128 140 216 179 216 167 216 178 219 140 32 216 174 217 136 216 175 218 169 216 167 216 177 32 216 177 216 168 216 167 216 170 219 140 218 169 32 216 175 216 177 32 216 175 217 134 219 140 216 167 219 140 32 217 133 216 172 216 167 216 178 219 140 ");

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

    app.UseMiddleware<DockerStatusMiddleware>();


    app.UseAuthentication();
    app.UseAuthorization();


    app.MapControllers();

    app.MapHub<DockerHub>("/dockerhub");
}
app.Run();
