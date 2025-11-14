using System.Windows.Input;

namespace Labb_3.Command
{
    public static class CustomCommands
    {
        public static RoutedUICommand ToggleFullscreen { get; } = new RoutedUICommand(
            "Toggle Fullscreen",
            nameof(ToggleFullscreen),
            typeof(CustomCommands));
    }
}

