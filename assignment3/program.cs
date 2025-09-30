using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKCube
{
    class CubeWindow : GameWindow
    {
        private int _vao, _vbo, _ebo, _program;

        // position (x,y,z) + color (r,g,b)
        private readonly float[] _vertices =
        {
            // 8 unique cube corners with colors per-vertex
            // Front face (z = +0.5)
            -0.5f, -0.5f,  0.5f, 1f, 0f, 0f, // 0: red
             0.5f, -0.5f,  0.5f, 0f, 1f, 0f, // 1: green
             0.5f,  0.5f,  0.5f, 0f, 0f, 1f, // 2: blue
            -0.5f,  0.5f,  0.5f, 1f, 1f, 0f, // 3: yellow

            // Back face (z = -0.5)
            -0.5f, -0.5f, -0.5f, 1f, 0f, 1f, // 4: magenta
             0.5f, -0.5f, -0.5f, 0f, 1f, 1f, // 5: cyan
             0.5f,  0.5f, -0.5f, 1f, 1f, 1f, // 6: white
            -0.5f,  0.5f, -0.5f, 0.3f, 0.3f, 0.3f // 7: gray
        };

        // 12 triangles (two per face) -> 36 indices
        private readonly uint[] _indices =
        {
            // Front
            0,1,2,  2,3,0,
            // Right
            1,5,6,  6,2,1,
            // Back
            5,4,7,  7,6,5,
            // Left
            4,0,3,  3,7,4,
            // Top
            3,2,6,  6,7,3,
            // Bottom
            4,5,1,  1,0,4
        };

        private int _uMvp;                // uniform location
        private Matrix4 _proj, _view;     // static per-frame matrices
        private float _angle;             // rotation angle (radians)

        public CubeWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            // GL state
            GL.ClearColor(0.07f, 0.08f, 0.10f, 1f);
            GL.Enable(EnableCap.DepthTest); // IMPORTANT for 3D

             GL.Viewport(0, 0, Size.X, Size.Y);

            // Build shader program
            const string vs = @"#version 330 core
            layout(location = 0) in vec3 aPos;
            layout(location = 1) in vec3 aColor;
            uniform mat4 uMVP;
            out vec3 vColor;
            void main()
            {
                vColor = aColor;
                gl_Position = uMVP * vec4(aPos, 1.0);
            }";
            const string fs = @"#version 330 core
            in vec3 vColor;
            out vec4 FragColor;
            void main()
            {
                FragColor = vec4(vColor, 1.0);
            }";
            var v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            CheckShader(v);

            var f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            CheckShader(f);

            _program = GL.CreateProgram();
            GL.AttachShader(_program, v);
            GL.AttachShader(_program, f);
            GL.LinkProgram(_program);
            CheckProgram(_program);
            GL.DeleteShader(v);
            GL.DeleteShader(f);

            _uMvp = GL.GetUniformLocation(_program, "uMVP");

            // VAO + VBO + EBO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Vertex layout: position (3 floats) then color (3 floats)
            int stride = 6 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Camera (view) and projection
            _view = Matrix4.LookAt(new Vector3(2f, 2f, 3f), Vector3.Zero, Vector3.UnitY);
            UpdateProjection(); // sets _proj based on window size

            // Unbind safety
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            float aspect = Size.X / (float)Size.Y;
            _proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            
            float baseSpeed = MathHelper.DegreesToRadians(60f);
            _angle += baseSpeed * (float)e.Time;

            
            var k = KeyboardState;
            if (k.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left))
                _angle -= baseSpeed * 2f * (float)e.Time;
            if (k.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right))
                _angle += baseSpeed * 2f * (float)e.Time;

            if (k.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape))
                Close();
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var model = Matrix4.CreateRotationY(_angle) * Matrix4.CreateRotationX(_angle * 0.4f);

            
            var mvp = model * _view * _proj;

            GL.UseProgram(_program);
            
            GL.UniformMatrix4(_uMvp, false, ref mvp);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
            SwapBuffers();
        }



        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_program);
        }

        private static void CheckShader(int s)
        {
            GL.GetShader(s, ShaderParameter.CompileStatus, out var ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(s));
        }
        private static void CheckProgram(int p)
        {
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out var ok);
            if (ok == 0) throw new Exception(GL.GetProgramInfoLog(p));
        }
    }

    static class Program
    {
        static int Main()
        {
            Console.WriteLine("GAM531 Assignment 3 (Console App) — starting OpenTK window...");
            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60;

            var nws = new NativeWindowSettings
            {
                Title = "OpenTK – 3D Cube (Perspective + Depth)",
                ClientSize = new Vector2i(960, 600),
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            };

            using var win = new CubeWindow(gws, nws);
            win.Run();

            Console.WriteLine("Window closed. Exiting cleanly.");
            return 0;
        }
    }
}
