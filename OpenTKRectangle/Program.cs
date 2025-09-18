using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;

namespace OpenTKRectangle
{
    class RectangleWindow : GameWindow
    {
        private int _program;
        private int _vao;
        private int _vbo;
        private int _ebo;

        private readonly float[] _vertices = {
            // x, y
            -0.5f, -0.5f, // 0
             0.5f, -0.5f, // 1
             0.5f,  0.5f, // 2
            -0.5f,  0.5f  // 3
        };

        private readonly uint[] _indices = {
            0, 1, 2,
            2, 3, 0
        };

        public RectangleWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.12f, 0.15f, 1.0f);

            // VAO
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            // VBO
            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // EBO
            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            // Shader (very small)
            var vertexShaderSource = @"#version 330 core
                layout(location = 0) in vec2 aPosition;
                void main()
                {
                    gl_Position = vec4(aPosition, 0.0, 1.0);
                }";

            var fragmentShaderSource = @"#version 330 core
                out vec4 FragColor;
                uniform vec3 uColor;
                void main()
                {
                    FragColor = vec4(uColor, 1.0);
                }";

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexShaderSource);
            GL.CompileShader(vs);
            CheckShaderCompile(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentShaderSource);
            GL.CompileShader(fs);
            CheckShaderCompile(fs);

            _program = GL.CreateProgram();
            GL.AttachShader(_program, vs);
            GL.AttachShader(_program, fs);
            GL.LinkProgram(_program);
            CheckProgramLink(_program);

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            // Vertex attribute
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // unbind (safety)
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_program);
            int loc = GL.GetUniformLocation(_program, "uColor");
            GL.Uniform3(loc, 0.9f, 0.4f, 0.2f); // Orange Colour

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
            {
                throw new Exception($"Shader compile error: {GL.GetShaderInfoLog(shader)}");
            }
        }

        private static void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                throw new Exception($"Program link error: {GL.GetProgramInfoLog(program)}");
            }
        }
    }

    class Program
    {
        static void Main()
        {
            var gws = GameWindowSettings.Default;
            
            gws.UpdateFrequency = 60;

            var nws = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600), // Size -> ClientSize changed
                Title = "OpenTK - Rectangle",
                APIVersion = new Version(3, 3),                 
                Profile = ContextProfile.Core,                 
                Flags = ContextFlags.ForwardCompatible         
            };

            using (var win = new RectangleWindow(gws, nws))
            {
                win.Run();
            }
        }
    }
}
