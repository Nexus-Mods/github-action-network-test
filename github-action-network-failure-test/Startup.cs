using Microsoft.Extensions.DependencyInjection;

namespace github_action_network_failure_test;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<LocalHttpServer>();
    }
}

