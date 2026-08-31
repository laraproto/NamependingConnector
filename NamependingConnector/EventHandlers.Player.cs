using System.Linq;
using System.Threading.Tasks;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;

namespace NamependingConnector;

public sealed class PlayerEventsHandler : CustomEventsHandler
{

    private const long PermanentBan = 50 * 365 * 24 * 60 * 60;

    public override void OnPlayerPreAuthenticating(PlayerPreAuthenticatingEventArgs ev) =>
        LoadPlayerData(ev.UserId, ev.Flags).ConfigureAwait(false);

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        var p = ev.Player;
        if (PlayerProperties.ToBan.TryGetValue(p.UserId, out var time) && Time.time - time < 30f)
        {
            p.Kick(PlayerProperties.BannedMessage);
            PlayerProperties.ToBan.Remove(p.UserId);
            return;
        }

        if (!PlayerProperties.Info.TryGetValue(p.UserId, out var data))
        {
            _ = WebClient.CreatePlayer(p.UserId, p.Nickname, p.DoNotTrack)
                .ConfigureAwait(false);
            return;
        }
        
        if (data.Player.User?.Group.GameGroup.Id != null)
        {
            var group = ServerStatic.PermissionsHandler.GetGroup($"RANK-{data.Player.User.Group.GameGroup.Id}");
            p.ReferenceHub.serverRoles.SetGroup(group);
            ServerStatic.PermissionsHandler.Members[p.UserId] = group.Name;
        }
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        if (ev.Player is null)
            return;

        var id = ev.Player.UserId;
        if (string.IsNullOrEmpty(id) || !PlayerProperties.Info.Remove(id, out var data))
            return;
        
        var roundDuration = Round.Duration.TotalSeconds;
        var sessionTime = (DateTimeOffset.UtcNow - data.Player.ConnectTime).TotalSeconds;
        
        _ = WebClient.UpdatePlayer(id, ev.Player.DoNotTrack, ev.Player.Nickname, (int)Math.Min(sessionTime, roundDuration)).ConfigureAwait(false);
    }

    public override void OnPlayerBanning(PlayerBanningEventArgs ev)
    {
        ev.IsAllowed = false;

        if (ev.Player is null)
        {
            ev.Reason = "Could not find player to ban.";
            return;
        }
        
        if (string.IsNullOrEmpty(ev.Reason))
            ev.Reason = "No reason provided.";
        
        _ = WebClient.CreateBan(ev.Issuer.UserId, ev.Player.UserId, (int)ev.Duration, ev.Reason, ev.Duration == PermanentBan).ConfigureAwait(false);
        ev.Player.Kick($"You have been banned: {ev.Reason}");
    }
    
    private static async Task LoadPlayerData(string userId, CentralAuthPreauthFlags flags)
    {
        var data = await WebClient.GetPlayer(userId);
        if (data is null)
            return;
        PlayerProperties.Info[userId] = data;

        var isBanned = data.Player.Bans.Any(bansContent => bansContent.Active);

        if (isBanned && (flags & CentralAuthPreauthFlags.IgnoreBans) == 0)
        {
            KickOrQueueBan(userId);
        }
    }

    public static void KickOrQueueBan(string userId)
    {
        if (Player.TryGet(userId, out var player) && player.IsReady)
            player.Kick(PlayerProperties.BannedMessage);
        else
            PlayerProperties.ToBan[userId] = Time.time;
    }
}