using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace Gateway;

internal sealed class TripStatefulService : StatefulService
{
    public TripStatefulService(StatefulServiceContext context)
        : base(context)
    {
    }
}
