using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Assignment6_FPSCamera
{
    static class Program
    {
        static int Main()
        {
            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60;

            var client = new Vector2i(1280, 720);

            var primary = Monitors.GetPrimaryMonitor();

            
            var screenWidth = primary.HorizontalResolution;
            var screenHeight = primary.VerticalResolution;

            
            var x = (screenWidth - client.X) / 2;
            var y = (screenHeight - client.Y) / 2;

            
            Console.WriteLine($"Monitor Resolution: {screenWidth}x{screenHeight}");
            Console.WriteLine($"Calculated Position: X={x}, Y={y}");

            var nws = new NativeWindowSettings
            {
                Title = "Assignment 6 - FPS Camera (WASD + Mouse + Scroll Zoom)",
                ClientSize = client,
                Location = new Vector2i(x, y), 
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible,
                StartFocused = true,
                StartVisible = true
            };

            using var win = new Game(gws, nws);

           
            win.Run();
            return 0;
        }
    }
}
