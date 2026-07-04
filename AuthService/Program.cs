using Microsoft.ServiceFabric.Services.Runtime;

namespace AuthService;

internal static class Program
{
    private static void Main()
    {
        ServiceRuntime.RegisterServiceAsync("AuthServiceType",
            context => new AuthStatefulService(context)).GetAwaiter().GetResult();

        Thread.Sleep(Timeout.Infinite);
    }
}
