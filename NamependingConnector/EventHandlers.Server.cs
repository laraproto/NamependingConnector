using System.Threading.Tasks;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Permissions;
using LabApi.Features.Permissions.Providers;
using static PlayerPermissions;

namespace NamependingConnector;

public sealed class ServerEventHandlers : CustomEventsHandler
{
    public override void OnServerWaitingForPlayers()
    {
        if (WebClient.Client == null)
        {
            return;
        }

        LoadRoleData().ConfigureAwait(true);
    }

    private static async Task LoadRoleData()
    {
        var data = await WebClient.GetRoles();
        
        if (data == null)
        {
            Logger.Error("Failed to load role data from API.");
            return;
        }
        
        foreach (var roleContent in data.Roles)
        {
            ulong perm = 0;

            foreach (var permission in roleContent.GameGroup.Permissions)
            {
                if (!Enum.TryParse<PlayerPermissions>(permission, out var permissionValue))
                {
                    Logger.Warn($"Failed to parse permission '{permission}' for role '{roleContent.GameGroup.Name}'.");
                    continue;
                }
                
                perm |= (ulong)permissionValue;
            }

            var group = new UserGroup
            {
                Name = $"RANK-{roleContent.GameGroup.Id}",
                Permissions = perm,
                BadgeColor = roleContent.GameGroup.Color,
                BadgeText = roleContent.GameGroup.Name
            };

            if (!ServerStatic.PermissionsHandler.Groups.TryAdd(group.Name, group))
            {
                ServerStatic.PermissionsHandler.Groups[group.Name] = group; //dumb shite, allows refresh
            }
        }
    }
}