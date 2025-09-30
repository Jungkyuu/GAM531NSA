using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKRectangle
{
    class RectangleWindow : GameWindow
    {
        private int _program;
        private int _vao, _vbo, _ebo;
        private int _uMvp;            // uniform location for transformation matrix

        // Rectangle vertices: 2D positions
        private readonly float[] _vertices = {
            -0.5f, -0.5f, // 0
             0.5f, -0.5f, // 1
             0.5f,  0.5f, // 2
            -0.5f,  0.5f  // 3
        };

        // Indices for two triangles forming a rectangle
        private readonly uint[] _indices = {
            0, 1, 2,
            2, 3, 0
        };

        private float _angle; // rotation angle (radians)

        public RectangleWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.12f, 0.15f, 1.0f);
            GL.Viewport(0, 0, Size.X, Size.Y);

            // Create VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // Create VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Create EBO
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Vertex + fragment shaders with transformation matrix
            var vertexShaderSource = @"#version 330 core
            layout(location = 0) in vec2 aPosition;
            uniform mat4 uMVP;
            void main()
            {
                gl_Position = uMVP * vec4(aPosition, 0.0, 1.0);
            }";
                        var fragmentShaderSource = @"#version 330 core
            out vec4 FragColor;
            uniform vec3 uColor;
            void main()
            {
                FragColor = vec4(uColor, 1.0);
            }";

            // Compile shaders
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexShaderSource);
            GL.CompileShader(vs);
            CheckShaderCompile(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentShaderSource);
            GL.CompileShader(fs);
            CheckShaderCompile(fs);

            // Link shaders into program
            _program = GL.CreateProgram();
            GL.AttachShader(_program, vs);
            GL.AttachShader(_program, fs);
            GL.LinkProgram(_program);
            CheckProgramLink(_program);

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            // Vertex attribute (2D positions only)
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Get uniform location
            _uMvp = GL.GetUniformLocation(_program, "uMVP");

            // Unbind for safety
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Auto rotation speed
            float speedRadPerSec = MathHelper.DegreesToRadians(60f);
            _angle += speedRadPerSec * (float)e.Time;

            // Keyboard controls
            var k = KeyboardState;
            if (k.IsKeyDown(Keys.Left))  _angle -= speedRadPerSec * 2f * (float)e.Time;
            if (k.IsKeyDown(Keys.Right)) _angle += speedRadPerSec * 2f * (float)e.Time;
            if (k.IsKeyDown(Keys.Escape)) Close();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_program);

            // === Matrix transformations ===
            Matrix4 I = Matrix4.Identity;                               // Identity
            Matrix4 S = Matrix4.CreateScale(1.2f, 0.8f, 1f);            // Scaling
            Matrix4 R = Matrix4.CreateRotationZ(_angle);                // Rotation (Z-axis)
            Matrix4 T = Matrix4.CreateTranslation(0.25f, 0.15f, 0f);    // Translation

            // Combine transformations (T ∘ R ∘ S ∘ I)
            Matrix4 M = S;
            M = R * M;
            M = T * M;

            // Upload MVP matrix
            GL.UniformMatrix4(_uMvp, false, ref M);

            // Set rectangle color
            int locColor = GL.GetUniformLocation(_program, "uColor");
            GL.Uniform3(locColor, 0.9f, 0.4f, 0.2f);

            // Draw rectangle
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

        private static void CheckShaderCompile(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
                throw new Exception($"Shader compile error: {GL.GetShaderInfoLog(shader)}");
        }

        private static void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
                throw new Exception($"Program link error: {GL.GetProgramInfoLog(program)}");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.WriteLine("Assignment 2 – Rectangle with Identity, Scaling, Rotation, Translation, Multiplication");

            var gws = GameWindowSettings.Default;
            gws.UpdateFrequency = 60;

            var nws = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Assignment 2 – Rectangle with Matrix Transforms",
                APIVersion = new Version(3, 3),
                Profile = ContextProfile.Core,
                Flags = ContextFlags.ForwardCompatible
            };

            using (var win = new RectangleWindow(gws, nws))
                win.Run();
        }
    }
}
