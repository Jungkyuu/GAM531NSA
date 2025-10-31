using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace Game;

internal static class Program
{
    public static void Main()
    {
        var nativeSettings = new NativeWindowSettings
        {
            Title = "Mini 3D Explorer",
            ClientSize = new Vector2i(1600, 900),
            WindowState = WindowState.Normal
        };

        using var game = new GameWindow3D(GameWindowSettings.Default, nativeSettings);
        game.Run();
    }
}