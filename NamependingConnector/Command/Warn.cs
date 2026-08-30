using System.ComponentModel;
using System.Globalization;
using System.Linq;
using CommandSystem;
using LabApi.Features.Wrappers;
using NamependingConnector.Models;
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

        if (!sender.CheckPermission(Permissions.ViewWarnings))
        {
            response = "You do not have permission to use this command.";
            return false;
        }
        
        if (arguments.Count < 3)
        {
            response = "Usage: warn <player> <type> [duration if applicable] <reason>";
            return false;
        }

        if (!Player.TryGet(int.Parse(arguments.At(0)), out var player))
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
        
        var duration = TimeSpan.FromSeconds(0);
        
        if (type.StartsWith("temp"))
        {
            if (arguments.Count < 4 || !TryParseDuration(arguments.At(2), out duration))
            {
                response = "Usage: warn <player> <type> [duration if applicable] <reason>";
                return false;
            }
        }
        
        var reason = string.Join(" ", arguments.Skip(type.StartsWith("temp") ? 3 : 2));
        
        _ = WebClient.CreateWarn(hub.authManager.UserId, player.UserId, reason, type, (int)duration.TotalSeconds).ConfigureAwait(false);
        response = $"Warned {player.Nickname} for {reason}.";
        return true;
    }

    private static bool TryParseDuration(string input, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (TimeSpan.TryParse(input, CultureInfo.InvariantCulture, out duration))
        {
            return true;
        }

        var suffixes = new[]
        {
            ("mo", TimeSpan.FromDays(30)),
            ("y", TimeSpan.FromDays(365)),
            ("w", TimeSpan.FromDays(7)),
            ("d", TimeSpan.FromDays(1)),
            ("h", TimeSpan.FromHours(1)),
            ("m", TimeSpan.FromMinutes(1)),
            ("s", TimeSpan.FromSeconds(1))
        };

        foreach (var (suffix, multiplier) in suffixes)
        {
            if (!input.EndsWith(suffix))
            {
                continue;
            }

            if (!double.TryParse(input[..^suffix.Length], NumberStyles.Float, CultureInfo.InvariantCulture, out var amount))
            {
                return false;
            }

            duration = TimeSpan.FromSeconds(amount * multiplier.TotalSeconds);
            return true;
        }

        return false;
    }
}
