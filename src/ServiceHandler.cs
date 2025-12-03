using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace Ocelot.Testing;

// TODO 1. Refactor in future to make this class a base class of acceptance steps
// TODO 2. Develop async versions for each sync method
public class ServiceHandler : IDisposable
{
    private readonly ConcurrentDictionary<string, IWebHost> _hosts = new();

    public void Dispose()
    {
        foreach (var kv in _hosts)
        {
            kv.Value?.Dispose();
        }
        _hosts.Clear();
        GC.SuppressFinalize(this);
    }

    protected Task AddOrStopAsync(string key, IWebHost host)
    {
        if (_hosts.TryAdd(key, host))
            return Task.CompletedTask;

        var h = _hosts.GetOrAdd(key, host);
        _hosts[key] = host;

        // Shutdown old host
        return h.StopAsync().ContinueWith(t => h.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
    }

    public Task ReleasePortAsync(params int[] ports)
    {
        List<Task> tasks = new(ports.Length);
        foreach (int port in ports)
        {
            var kv = _hosts.SingleOrDefault(x => new Uri(x.Key).Port == port);
            if (kv.Key is null || kv.Value is null)
                continue;

            var host = kv.Value;
            var task = host.StopAsync()
                .ContinueWith(t =>
                {
                    host.Dispose();
                    _hosts.TryRemove(kv);
                });
            tasks.Add(task);
        }
        return Task.WhenAll(tasks);
    }

    public IWebHost GivenThereIsAServiceRunningOn(string baseUrl, RequestDelegate handler)
    {
        var host = TestHostBuilder.Create()
            .UseUrls(baseUrl)
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .Configure(app => app.Run(handler))
            .Build();
        AddOrStopAsync(baseUrl, host).Wait();
        host.Start();
        return host;
    }

    public void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, RequestDelegate handler)
    {
        var host = TestHostBuilder.Create()
            .UseUrls(baseUrl)
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .Configure(app => app.UsePathBase(basePath).Run(handler))
            .Build();
        AddOrStopAsync(baseUrl, host).Wait();
        host.Start();
    }

    public void GivenThereIsAServiceRunningOn(string baseUrl, string basePath, Action<IServiceCollection> configureServices, RequestDelegate handler)
    {
        var host = TestHostBuilder.Create()
            .UseUrls(baseUrl)
            .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .ConfigureServices(configureServices)
            .Configure(app => app.UsePathBase(basePath).Run(handler))
            .Build();
        AddOrStopAsync(baseUrl, host).Wait();
        host.Start();
    }

    public void GivenThereIsAServiceRunningOnWithKestrelOptions(string baseUrl, string basePath, Action<KestrelServerOptions> options, RequestDelegate handler)
    {
        var host = TestHostBuilder.Create()
            .UseUrls(baseUrl)
            .UseKestrel()
            .ConfigureKestrel(options ?? WithDefaultKestrelServerOptions) // !
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
            .Configure(app => app.UsePathBase(basePath).Run(handler))
            .Build();
        AddOrStopAsync(baseUrl, host).Wait();
        host.Start();
    }

    internal void WithDefaultKestrelServerOptions(KestrelServerOptions options)
    {
    }

    public void GivenThereIsAHttpsServiceRunningOn(string baseUrl, string basePath, string fileName, string password, int port, RequestDelegate handler)
    {
        void WithKestrelOptions(KestrelServerOptions options)
        {
            options.Listen(IPAddress.Loopback, port, o => o.UseHttps(fileName, password));
        }
        var host = TestHostBuilder.Create()
            .UseUrls(baseUrl)
            .UseKestrel(WithKestrelOptions)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .Configure(app => app.UsePathBase(basePath).Run(handler))
            .Build();
        AddOrStopAsync(baseUrl, host).Wait();
        host.Start();
    }

    #region Advanced helpers

    public static string Localhost(int port) => $"{Uri.UriSchemeHttp}://localhost:{port}";

    public void GivenThereIsAServiceRunningOn(int port, RequestDelegate handler)
        => GivenThereIsAServiceRunningOn(Localhost(port), handler);

    public void GivenThereIsAServiceRunningOn(int port, string path, RequestDelegate handler)
        => GivenThereIsAServiceRunningOn(Localhost(port), path, handler);

    #endregion

    public IWebHost GivenThereIsAServiceRunningOn(int port,
        Action<WebHostBuilderContext, IConfigurationBuilder>? configureDelegate,
        Action<WebHostBuilderContext, ILoggingBuilder>? configureLogging,
        Action<IServiceCollection>? configureServices,
        Action<IApplicationBuilder>? configureApp,
        Action<IWebHostBuilder>? configureWebHost)
        => GivenThereIsAServiceRunningOn(Localhost(port), configureDelegate, configureLogging, configureServices, configureApp, configureWebHost);
    public IWebHost GivenThereIsAServiceRunningOn(string baseUrl,
        Action<WebHostBuilderContext, IConfigurationBuilder>? configureDelegate,
        Action<WebHostBuilderContext, ILoggingBuilder>? configureLogging,
        Action<IServiceCollection>? configureServices,
        Action<IApplicationBuilder>? configureApp,
        Action<IWebHostBuilder>? configureWebHost)
    {
        var builder = TestHostBuilder.Create().UseUrls(baseUrl).UseKestrel();
        if (configureDelegate != null) builder.ConfigureAppConfiguration(configureDelegate);
        if (configureLogging != null) builder.ConfigureLogging(configureLogging);
        if (configureServices != null) builder.ConfigureServices(configureServices);
        if (configureApp != null) builder.Configure(configureApp);
        configureWebHost?.Invoke(builder);
        var host = builder.Build();
        AddOrStopAsync(baseUrl, host).Wait();
        host.Start();
        return host;
    }

    public Task<IWebHost> GivenThereIsAServiceRunningOnAsync(int port,
        Action<WebHostBuilderContext, IConfigurationBuilder>? configureDelegate,
        Action<WebHostBuilderContext, ILoggingBuilder>? configureLogging,
        Action<IServiceCollection>? configureServices,
        Action<IApplicationBuilder>? configureApp,
        Action<IWebHostBuilder>? configureWebHost)
        => GivenThereIsAServiceRunningOnAsync(Localhost(port), configureDelegate, configureLogging, configureServices, configureApp, configureWebHost);
    public Task<IWebHost> GivenThereIsAServiceRunningOnAsync(string baseUrl,
        Action<WebHostBuilderContext, IConfigurationBuilder>? configureDelegate,
        Action<WebHostBuilderContext, ILoggingBuilder>? configureLogging,
        Action<IServiceCollection>? configureServices,
        Action<IApplicationBuilder>? configureApp,
        Action<IWebHostBuilder>? configureWebHost)
    {
        var builder = TestHostBuilder.Create().UseUrls(baseUrl).UseKestrel();
        if (configureDelegate != null) builder.ConfigureAppConfiguration(configureDelegate);
        if (configureLogging != null) builder.ConfigureLogging(configureLogging);
        if (configureServices != null) builder.ConfigureServices(configureServices);
        if (configureApp != null) builder.Configure(configureApp);
        configureWebHost?.Invoke(builder);
        var host = builder.Build();
        return AddOrStopAsync(baseUrl, host)
            .ContinueWith(t => host.StartAsync())
            .ContinueWith(t => host, TaskContinuationOptions.ExecuteSynchronously);
    }
}
