namespace SteamPrefill.Models.Enums
{
    [Intellenum(typeof(string))]
    public sealed partial class OperatingSystem
    {
        // Looks like there is currently one game that is android : Walkabout Mini Golf
        public static readonly OperatingSystem Android = new("android");

        public static readonly OperatingSystem Windows = new("windows");
        public static readonly OperatingSystem MacOS = new("macos");
        public static readonly OperatingSystem Linux = new("linux");
    }
}