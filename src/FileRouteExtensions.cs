//using Ocelot.Configuration.File;
namespace Ocelot.Testing;

public static class FileRouteExtensions
{
    public static /*FileRoute*/T WithHosts<T>(this /*FileRoute*/T route, params /*FileHostAndPort*/object[] hosts)
        => Box.In(route).Hosts(hosts).Out(); //route.DownstreamHostAndPorts.AddRange(hosts);

    public static /*FileRoute*/T WithPriority<T>(this /*FileRoute*/T route, int priority)
        => Box.In(route).Priority(priority).Out(); //route.Priority = priority;

    public static /*FileRoute*/T WithMethods<T>(this /*FileRoute*/T route, params string[] methods)
        => Box.In(route).Methods(methods).Out(); //route.UpstreamHttpMethod = [.. methods];

    public static /*FileRoute*/T WithUpstreamHeaderTransform<T>(this /*FileRoute*/T route, params KeyValuePair<string, string>[] pairs)
        => Box.In(route).UpstreamHeaderTransform(pairs).Out(); //route.UpstreamHeaderTransform = new Dictionary<string, string>(pairs);

    public static /*FileRoute*/T WithUpstreamHeaderTransform<T>(this /*FileRoute*/T route, string key, string value)
        => Box.In(route).UpstreamHeaderTransform(key, value).Out(); //route.UpstreamHeaderTransform.TryAdd(key, value);

    public static /*FileRoute*/T WithHttpHandlerOptions<T>(this /*FileRoute*/T route, /*FileHttpHandlerOptions*/object options)
        => Box.In(route).HttpHandlerOptions(options).Out(); //route.HttpHandlerOptions = options;

    public static /*FileRoute*/T WithKey<T>(this /*FileRoute*/T route, string? key)
        => Box.In(route).Key(key).Out(); //route.Key = key;

    public static /*FileRoute*/T WithUpstreamHost<T>(this /*FileRoute*/T route, string? upstreamHost)
        => Box.In(route).UpstreamHost(upstreamHost).Out(); //route.UpstreamHost = upstreamHost;
}
