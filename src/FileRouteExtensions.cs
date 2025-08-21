namespace Ocelot.Testing;

/// <summary>
/// For type: Ocelot.Configuration.File.FileRoute
/// </summary>
public static class FileRouteExtensions
{
    public static R WithHosts<R, H>(this R route, params H[] hosts)
        where R : class // FileRoute
        where H : class // FileHostAndPort
        => Box.In<FileRouteBox<R>, R>(route).Hosts(hosts).Out(); //route.DownstreamHostAndPorts.AddRange(hosts);

    public static R WithPriority<R>(this R route, int priority)
        where R : class // FileRoute
        => FileRouteBox<R>.In(route).Priority(priority).Out(); //route.Priority = priority;

    public static R WithMethods<R>(this R route, params string[] methods)
        where R : class // FileRoute
        => FileRouteBox.In(route).Methods(methods).Out(); //route.UpstreamHttpMethod = [.. methods];

    public static R WithUpstreamHeaderTransform<R>(this R route, params KeyValuePair<string, string>[] pairs)
        where R : class // FileRoute
        => new FileRouteBox<R>(route).UpstreamHeaderTransform(pairs).Out(); //route.UpstreamHeaderTransform = new Dictionary<string, string>(pairs);

    public static R WithUpstreamHeaderTransform<R>(this R route, string key, string value)
        where R : class // FileRoute
        => new FileRouteBox<R>(route).UpstreamHeaderTransform(key, value).Out(); //route.UpstreamHeaderTransform.TryAdd(key, value);

    public static R WithDownstreamHeaderTransform<R>(this R route, string key, string value)
        where R : class // FileRoute
        => new FileRouteBox<R>(route).DownstreamHeaderTransform(key, value).Out(); //route.DownstreamHeaderTransform.TryAdd(key, value);

    public static R WithHttpHandlerOptions<R, O>(this R route, O options)
        where R : class // FileRoute
        where O : class // FileHttpHandlerOptions
        => new FileRouteBox<R>(route).HandlerOptions(options).Out(); //route.HttpHandlerOptions = options;

    public static R WithKey<R>(this R route, string? key)
        where R : class // FileRoute
        => new FileRouteBox<R>(route).Key(key).Out(); //route.Key = key;

    public static R WithUpstreamHost<R>(this R route, string? upstreamHost)
        where R : class // FileRoute
        => new FileRouteBox<R>(route).UpstreamHost(upstreamHost).Out(); //route.UpstreamHost = upstreamHost;
}
