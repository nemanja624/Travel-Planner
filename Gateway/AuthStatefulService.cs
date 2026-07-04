using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace Gateway;

internal sealed class AuthStatefulService : StatefulService
{
    public AuthStatefulService(StatefulServiceContext context)
        : base(context)
    {
    }
}
