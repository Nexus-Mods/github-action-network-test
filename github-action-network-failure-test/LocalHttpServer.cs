using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace github_action_network_failure_test;

public class LocalHttpServer : IDisposable
{
    private readonly ILogger<LocalHttpServer> _logger;
    private readonly HttpListener _listener;
    private readonly string _prefix;

    public byte[] LargeData { get; set; }
    public byte[] LargeDataHash { get; set; }


    public LocalHttpServer(ILogger<LocalHttpServer> logger)
    {
        _logger = logger;
        (_listener, _prefix) = CreateNewListener();
        _listener.Start();

        LargeData = GenerateLargeData();
        LargeDataHash = SHA256.HashData(LargeData);

        StartLoop();
    }



    private byte[] GenerateLargeData()
    {
        var data = new byte[10 * 1024 * 1024];
        // Bit of a hack because .NextBytes is *extremely* slow
        Random.Shared.NextBytes(data);

        return data;
    }

    private void StartLoop()
    {
        Task.Run(async () =>
        {
            while (_listener.IsListening)
            {
                var context = await _listener.GetContextAsync();
                _logger.LogInformation("Got connection");
                using var resp = context.Response;
                resp.StatusCode = 200;
                resp.StatusDescription = "OK";
                resp.ProtocolVersion = HttpVersion.Version11;
                resp.ContentLength64 = LargeData.Length;
                await using var ros = resp.OutputStream;
                await ros.WriteAsync(LargeData);
            }
        });
    }
    
    public Uri Uri => new(_prefix);

    private (HttpListener Listener, string Prefix) CreateNewListener()
    {
        HttpListener mListener;
        while (true)
        {
            mListener = new HttpListener();
            var newPort = Random.Shared.Next(49152, 65535);
            mListener.Prefixes.Add($"http://127.0.0.1:{newPort}/");
            try
            {
                mListener.Start();
            }
            catch
            {
                continue;
            }
            break;
        }

        return (mListener, mListener.Prefixes.First());
    }

    public void Dispose()
    {
        _listener.Stop();
    }
}
