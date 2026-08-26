using System.Threading.Tasks;
using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using NamependingConnector.Models;
using NamependingConnector.Web;

namespace NamependingConnector.Command;

[CommandHandler(typeof(ClientCommandHandler))]
public class Link : ICommand
{
    protected async Task StartLink(PlayerCommandSender sender)
    {
        var startLink = await WebClientHandler.LinkAccount(new PlayerGetRequest(sender.ReferenceHub.authManager.UserId));

        if (!Player.TryGet(sender.ReferenceHub.PlayerId, out var player))
        {
            Logger.Info("Player not found when sending link token.");
            return;
        }
        
        if (!startLink.Success)
        {
            player.SendConsoleMessage("Something went wrong, please try again later.");
            return;
        }
        player.SendConsoleMessage($"Your link token is {startLink.LinkToken}. Link your account on the panel, code expires in 15 minutes.");
    }
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender {ReferenceHub: var hub} commandSender)
        {
            response = "Only a player can execute this command.";
            return false;
        }

        response = "Please wait...";
        StartLink(commandSender).ConfigureAwait(false);
        return true;
    }
    
    public string Command { get; } = "link"; // The command used in the console.
    public string[] Aliases { get; } = []; // The desired aliases.
    public string Description { get; } = "Link to your panel account"; // A small description.
}