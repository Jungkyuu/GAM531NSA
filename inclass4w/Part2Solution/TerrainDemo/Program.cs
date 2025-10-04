using System;
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
            var native = new NativeWindowSettings
            {
                ClientSize = new Vector2i(1280, 720),
                Title = "OpenTK – Exercise 6 Terrain",
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
                Flags = ContextFlags.Default
            };


            using var window = new GameWindow(GameWindowSettings.Default, native);
            var game = new Game(1280, 720);


            window.Load += () =>
            {
                game.Init();
                game.OnResize(window.Size.X, window.Size.Y);
            };


            window.RenderFrame += (FrameEventArgs e) =>
            {
                game.Tick((float)e.Time);
                window.SwapBuffers();
            };


            window.Resize += (ResizeEventArgs e) =>
            {
                game.OnResize(e.Width, e.Height);
            };


            window.UpdateFrame += (FrameEventArgs e) =>
            {
                if (window.KeyboardState.IsKeyDown(Keys.Escape)) window.Close();
            };


            window.Run();
        }
    }
}