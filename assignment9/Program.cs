using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

class Program
{
    static void Main(string[] args)
    {
        var gws = GameWindowSettings.Default;

        var nws = new NativeWindowSettings
        {
            ClientSize = new Vector2i(1280, 720),
            Title = "Assignment 9 - 3D Collision Detection"
        };

        using var game = new MyGame(gws, nws);
        game.Run();
    }
}
