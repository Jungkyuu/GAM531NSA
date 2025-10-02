using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TextureCube
{
    static class Program
    {
        static int Main()
        {
            Console.WriteLine("=====================================");
            Console.WriteLine(" Assignment 4 - Textured Cube (OpenTK)");
            Console.WriteLine(" Controls: ");
            Console.WriteLine("   ← / →  : Rotate faster left / right");
            Console.WriteLine("   ESC    : Exit program");
            Console.WriteLine("=====================================\n");

            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60;

            var nws = new NativeWindowSettings
            {
                Title = "Assignment 4 - Textured Cube",
                ClientSize = new Vector2i(960, 600),
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            };

            using var win = new TextureCubeWindow(gws, nws);
            win.Run();

            Console.WriteLine("Program finished. Press any key to exit...");
            Console.ReadKey(); 
            return 0;
        }
    }
}
