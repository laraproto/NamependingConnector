using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using NamependingConnector.Models;
using RemoteAdmin;

namespace NamependingConnector;

public static class PlayerProperties
{
    public const string BannedMessage = "You have been banned.";

    public static Dictionary<string, GetPlayerResponse> Info { get; } = [];
    public static Dictionary<string, float> ToBan { get; } = [];
    
    public static bool CheckPermission(this ICommandSender sender, Permissions permissions) => sender switch
    {
        CommandSender {FullPermissions: true} => true,
        PlayerCommandSender {ReferenceHub: var hub} => hub.CheckPermission(permissions),
        _ => false
    };

    private static bool CheckPermission(this ReferenceHub hub, Permissions permissions)
        => Info.TryGetValue(hub.authManager.UserId, out var data) && data.Player.User is { Group: not null } && data.Player.User.Group.Permissions.HasFlagFast(permissions);

    public static bool CheckPermission(this ICommandSender sender, Permissions[] anyMatch) => anyMatch.Any(sender.CheckPermission);
}