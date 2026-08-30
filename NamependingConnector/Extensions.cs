using NamependingConnector.Models;

namespace NamependingConnector;

public static class Extensions
{
    public static bool HasFlagFast(this Permissions permissions, Permissions flag) => (permissions & flag) != 0;
}