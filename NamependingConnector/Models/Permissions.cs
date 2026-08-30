namespace NamependingConnector.Models;

[Flags]
public enum Permissions: long
{
    ViewWarnings = 1 << 3,
    CreateWarnings = 1 << 5,
    ViewBans = 1 << 9,
    CreateBans = 1 << 10,
    BanTwelveHours = 1 << 23,
    BanOneDay = 1 << 24,
    BanOneWeek = 1 << 25,
    BanTwoWeeks = 1 << 26,
    BanOneMonth = 1 << 27,
    BanPermanently = 1 << 28,
}