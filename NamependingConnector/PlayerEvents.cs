using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Wrappers;
using NamependingConnector.Models;

namespace NamependingConnector;

public sealed class PlayerEventsHandler : CustomEventsHandler
{
    public const string BannedMessage = "You have been banned.";

    private const long PermanentBan = 50 * 365 * 24 * 60 * 60;
    public static Dictionary<string, GetPlayerResponse> Info { get; } = [];
    public static Dictionary<string, float> ToBan { get; } = [];

    public override void OnPlayerPreAuthenticating(PlayerPreAuthenticatingEventArgs ev) =>
        LoadPlayerData(ev.UserId, ev.Flags).ConfigureAwait(false);

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev)
    {
        var p = ev.Player;
        if (ToBan.TryGetValue(p.UserId, out var time) && Time.time - time < 30f)
        {
            p.Kick(BannedMessage);
            ToBan.Remove(p.UserId);
            return;
        }

        if (!Info.TryGetValue(p.UserId, out var data))
        {
            return;
        }
        
        if (data == null)
        {
            /*WebClientHandler.CreatePlayer(new PlayerCreateRequest(p.Nickname, p.UserId, p.DoNotTrack))
                .ConfigureAwait(false);*/
        }
    }

    public override void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        if (ev.Player is null)
            return;

        var id = ev.Player.UserId;
        if (string.IsNullOrEmpty(id) || !Info.Remove(id, out var data))
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
            ev.Reason = "No reason provided. Please contact a Head Administrator for further details.";
        
        /*var banRequest = new CreateBanRequest(ev.Issuer.UserId, ev.Duration, ev.Player.UserId, ev.Reason, ev.Duration == PermanentBan);
        
        WebClientHandler.CreateBan(banRequest).ConfigureAwait(false);*/
        ev.Player?.Kick("You have been banned: " + ev.Reason);
    }
    
    private static async Task LoadPlayerData(string userId, CentralAuthPreauthFlags flags)
    {
        var data = await WebClient.GetPlayer(userId);
        if (data is null)
            return;
        Info[userId] = data;

        Logger.Info($"Loaded player data for {userId}: {data.Player.Id}");

        var isBanned = data.Player.Bans.Any(bansContent => bansContent.Active);

        if (isBanned && (flags & CentralAuthPreauthFlags.IgnoreBans) == 0)
        {
            KickOrQueueBan(userId);
        }
    }

    public static void KickOrQueueBan(string userId)
    {
        if (Player.TryGet(userId, out var player) && player.IsReady)
            player.Kick(BannedMessage);
        else
            ToBan[userId] = Time.time;
    }
}