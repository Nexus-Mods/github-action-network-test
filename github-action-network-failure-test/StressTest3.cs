using System.Security.Cryptography;
using FluentAssertions;

namespace github_action_network_failure_test;

public class StressTest3
{
    private readonly LocalHttpServer _server;
    private readonly HttpClient _client;

    public StressTest3(LocalHttpServer server)
    {
        _server = server;
        _client = new HttpClient();
    }
    
    [Fact]
    public async Task TestManyConnections()
    {
        for (var x = 0; x < 1000; x++)
        {
            using var res = await _client.GetAsync(_server.Uri + Random.Shared.Next().ToString(), HttpCompletionOption.ResponseHeadersRead);
            res.IsSuccessStatusCode.Should().BeTrue();

            await using var stream = await res.Content.ReadAsStreamAsync();
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            SHA256.HashData(ms.ToArray()).Should().BeEquivalentTo(_server.LargeDataHash);
        }
    }
}