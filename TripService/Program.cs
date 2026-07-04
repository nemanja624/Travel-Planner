using Microsoft.ServiceFabric.Services.Runtime;

namespace TripService;

internal static class Program
{
    private static void Main()
    {
        ServiceRuntime.RegisterServiceAsync("TripServiceType",
            context => new TripStatefulService(context)).GetAwaiter().GetResult();

        Thread.Sleep(Timeout.Infinite);
    }
}
