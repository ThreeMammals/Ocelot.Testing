using System.Collections;

namespace Ocelot.Testing;

public static class Box
{
    public static Box<T> In<T>(T instance) => new(instance);
    public static Box<T> With<T>(T instance) => In(instance);
}

public class Box<T>
{
    private readonly T _route;
    private readonly Type _type;
    public Box(T route)
    {
        if (route?.GetType().FullName != "Ocelot.Configuration.File.FileRoute")
            throw new ArgumentException("Is not type of Ocelot.Configuration.File.FileRoute", nameof(route));
        _route = route;
        _type = typeof(T); //route.GetType();
    }

    public T Out() => _route;
    public T Unbox() => Out();

    public Box<T> Hosts(params object[] hosts)
    {
        //route.DownstreamHostAndPorts.AddRange(hosts);
        var property = _type.GetProperty("DownstreamHostAndPorts");
        IList? downstreamHostAndPorts = property?.GetValue(_route) as IList;
        ArgumentNullException.ThrowIfNull(downstreamHostAndPorts);
        for (int i = 0; i < hosts.Length; i++)
        {
            var host = hosts[i];
            if (host?.GetType().FullName != "Ocelot.Configuration.File.FileHostAndPort")
                throw new ArgumentException($"Argument at index {i} is not type of Ocelot.Configuration.File.FileHostAndPort", nameof(hosts));
            downstreamHostAndPorts.Add(host);
        }
        return this;
    }

    public Box<T> Priority(int priority)
    {
        //route.Priority = priority;
        var property = _type.GetProperty("Priority");
        property?.SetValue(_route, priority);
        return this;
    }

    public Box<T> Methods(params string[] methods)
    {
        //route.UpstreamHttpMethod = [.. methods];
        var property = _type.GetProperty("UpstreamHttpMethod");
        IList upstreamHttpMethod = property?.GetValue(_route) as IList
            ?? throw new ArgumentNullException(nameof(upstreamHttpMethod));
        for (int i = 0; i < methods.Length; i++)
        {
            string method = methods[i];
            upstreamHttpMethod.Add(method);
        }
        return this;
    }

    public Box<T> UpstreamHeaderTransform(params KeyValuePair<string, string>[] pairs)
    {
        //route.UpstreamHeaderTransform = new Dictionary<string, string>(pairs);
        var property = _type.GetProperty("UpstreamHeaderTransform");
        IDictionary<string, string> upstreamHeaderTransform = property?.GetValue(_route) as IDictionary<string, string>
            ?? throw new ArgumentNullException(nameof(upstreamHeaderTransform));
        for (int i = 0; i < pairs.Length; i++)
        {
            var kv = pairs[i];
            upstreamHeaderTransform.Add(kv.Key, kv.Value);
        }
        return this;
    }

    public Box<T> UpstreamHeaderTransform(string key, string value)
    {
        //route.UpstreamHeaderTransform.TryAdd(key, value);
        var property = _type.GetProperty("UpstreamHeaderTransform");
        IDictionary<string, string> upstreamHeaderTransform = property?.GetValue(_route) as IDictionary<string, string>
            ?? throw new ArgumentNullException(nameof(upstreamHeaderTransform));
        upstreamHeaderTransform.TryAdd(key, value);
        return this;
    }

    public Box<T> HttpHandlerOptions(/*FileHttpHandlerOptions*/object options)
    {
        //route.HttpHandlerOptions = options;
        if (options?.GetType().FullName != "Ocelot.Configuration.File.FileHttpHandlerOptions")
            throw new ArgumentException($"Is not type of Ocelot.Configuration.File.FileHttpHandlerOptions", nameof(options));
        var property = _type.GetProperty("HttpHandlerOptions");
        property?.SetValue(_route, options);
        return this;
    }

    public Box<T> Key(string? key)
    {
        //route.Key = key;
        var property = _type.GetProperty("Key");
        property?.SetValue(_route, key);
        return this;
    }

    public Box<T> UpstreamHost(string? upstreamHost)
    {
        //route.UpstreamHost = upstreamHost;
        var property = _type.GetProperty("UpstreamHost");
        property?.SetValue(_route, upstreamHost);
        return this;
    }
}
