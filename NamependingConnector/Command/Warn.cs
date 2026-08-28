using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;

namespace NamependingConnector.Command;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class Warn: ICommand
{
    public string[] WarnTypes { get; } = ["minor", "major", "tempminor", "tempmajor"];
    
    public string Command { get; } = "warn";
    public string[] Aliases { get; } = ["addwarn", "w"];
    public string Description { get; } = "Warns a player.";
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender {ReferenceHub: var hub} commandSender)
        {
            response = "This command can only be executed by a player.";
            return false;
        }
        
        if (arguments.Count < 3)
        {
            response = "Usage: warn <player> <type> [duration if applicable] <reason>";
            return false;
        }

        if (Player.TryGet(arguments.At(0), out var player))
        {
            response = "Player not found.";
            return false;
        }
        
        var type = arguments.At(1);
        
        if (!WarnTypes.Contains(type))
        {
            response = $"Invalid warn type. Valid types are: {string.Join(", ", WarnTypes)}";
            return false;
        }
        
        var duration = 0;
        
        if (type.StartsWith("temp"))
        {
            if (arguments.Count < 4 || !int.TryParse(arguments.At(2), out duration))
            {
                response = "Usage: warn <player> <type> [duration] <reason>";
                return false;
            }
        }
        
        var reason = string.Join(" ", arguments.Skip(type.StartsWith("temp") ? 3 : 2));
        
        _ = WebClient.CreateWarn(hub.authManager.UserId, player.UserId, reason, type, duration).ConfigureAwait(false);
        response = $"Warned {player.Nickname} for {reason}.";
        return true;
    }
}
