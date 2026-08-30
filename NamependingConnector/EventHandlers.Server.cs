using System.Threading.Tasks;
using LabApi.Events.CustomHandlers;

namespace NamependingConnector;

public sealed class ServerEventHandlers : CustomEventsHandler
{
    public override void OnServerWaitingForPlayers()
    {
        if (WebClient.Client == null)
        {
            return;
        }

        LoadRoleData().ConfigureAwait(false);
    }

    private static async Task LoadRoleData()
    {
        var data = await WebClient.GetRoles();
    }
}