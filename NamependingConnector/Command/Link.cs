using System.Threading.Tasks;
using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using NamependingConnector.Models;

namespace NamependingConnector.Command;

[CommandHandler(typeof(ClientCommandHandler))]
public class Link : ICommand
{
    protected async Task StartLink(PlayerCommandSender sender)
    {
        if (!Player.TryGet(sender.ReferenceHub.PlayerId, out var player))
        {
            Logger.Info("Player not found when sending link token.");
            return;
        }
        
        var startLink = await WebClient.CreateLink(sender.ReferenceHub.authManager.UserId);
        
        if (startLink.CreateAccountLink is null)
        {
            player.SendConsoleMessage("Something went wrong, please try again later.");
            return;
        }
        
        var now = new DateTimeOffset(DateTime.UtcNow);
        var interval = startLink.CreateAccountLink.Expires - now;
        
        player.SendConsoleMessage($"Your link token is {startLink.CreateAccountLink.Key}. Link your account on the panel in settings, code expires in {Math.Round(interval.TotalMinutes)} minutes.");
    }
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender {ReferenceHub: var hub} commandSender)
        {
            response = "Only a player can execute this command.";
            return false;
        }

        if (PlayerProperties.Info.TryGetValue(hub.authManager.UserId, out var data) && data?.Player.UserId != null)
        {
            response = "You already have a linked panel account. Linking another one will overwrite the previous one.";
        }
        else
        {
            response = "Please wait...";
        }
        
        StartLink(commandSender).ConfigureAwait(false);
        return true;
    }
    
    public string Command { get; } = "link"; // The command used in the console.
    public string[] Aliases { get; } = []; // The desired aliases.
    public string Description { get; } = "Link to your panel account"; // A small description.
}