using System.Collections.Generic;
using NamependingConnector.Models;

namespace NamependingConnector;

public static class PlayerProperties
{
    public const string BannedMessage = "You have been banned.";

    public static Dictionary<string, GetPlayerResponse> Info { get; } = [];
    public static Dictionary<string, float> ToBan { get; } = [];
}