using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace PhongLighting
{
    static class Program
    {
        static int Main()
        {
            Console.WriteLine("Assignment 5 – Phong Lighting (OpenTK)");
            Console.WriteLine("Mouse: Left drag=orbit, Wheel=zoom");
            Console.WriteLine("Keys : Arrows=rotate object, I/K/J/L/U/O=move light");
            Console.WriteLine("       [ / ]=light intensity -/+, = / -=shininess +/- , R=reset, ESC=quit\n");

            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60;

            var nws = new NativeWindowSettings
            {
                Title = "Assignment 5 – Phong Lighting (with Sun Marker)",
                ClientSize = new Vector2i(960, 600),
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            };

            using var win = new Game(gws, nws);
            win.Run();

            Console.WriteLine("\nProgram finished. Press any key to exit...");
            Console.ReadKey();
            return 0;
        }
    }
}
