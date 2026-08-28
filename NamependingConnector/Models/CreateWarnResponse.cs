using System.Collections.Generic;

namespace NamependingConnector.Models;

public class CreateWarnResponse
{
    public static readonly Dictionary<string, string> WarnEnum = new()
    {
        { "tempminor", "TemporaryMinor" },
        { "tempmajor", "TemporaryMajor" },
        { "minor", "PermanentMinor" },
        { "major", "PermanentMajor" },
    };
    
    public bool CreateWarn;
}