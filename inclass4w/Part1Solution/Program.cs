using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace WindowEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings
            {
                ClientSize = new Vector2i(800, 600),
                Title = "OpenTK Graphics Tutorial",
                WindowBorder = WindowBorder.Fixed,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3)
            };

            using var window = new GameWindow(GameWindowSettings.Default, nativeWindowSettings);
            var game = new Game(800, 600);

            window.Load += () =>
            {
                var fb = window.FramebufferSize;
                GL.Viewport(0, 0, fb.X, fb.Y);
                game.Init();
            };

            window.UpdateFrame += (FrameEventArgs e) =>
            {
                game.HandleInput(window.KeyboardState, e.Time);
                if (window.KeyboardState.IsKeyDown(Keys.Escape)) window.Close();
            };

            window.RenderFrame += (FrameEventArgs e) =>
            {
                game.Tick(e.Time);
                window.SwapBuffers();
            };

            window.FramebufferResize += (FramebufferResizeEventArgs e) =>
            {
                var fb = window.FramebufferSize;
                GL.Viewport(0, 0, fb.X, fb.Y);
            };

            window.Run();
        }
    }
}
