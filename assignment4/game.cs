using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace TextureCube
{
    public class TextureCubeWindow : GameWindow
    {
        private int _vao, _vbo, _ebo, _program, _tex0;
        private int _uMvp, _uTex;
        private float _angle;

        // 24 vertices (per-face UVs): pos(x,y,z), uv(u,v)
        private readonly float[] _vertices =
        {
            // Front (+Z)
            -0.5f,-0.5f, 0.5f, 0f,0f,
             0.5f,-0.5f, 0.5f, 1f,0f,
             0.5f, 0.5f, 0.5f, 1f,1f,
            -0.5f, 0.5f, 0.5f, 0f,1f,

            // Right (+X)
             0.5f,-0.5f, 0.5f, 0f,0f,
             0.5f,-0.5f,-0.5f, 1f,0f,
             0.5f, 0.5f,-0.5f, 1f,1f,
             0.5f, 0.5f, 0.5f, 0f,1f,

            // Back (-Z)
             0.5f,-0.5f,-0.5f, 0f,0f,
            -0.5f,-0.5f,-0.5f, 1f,0f,
            -0.5f, 0.5f,-0.5f, 1f,1f,
             0.5f, 0.5f,-0.5f, 0f,1f,

            // Left (-X)
            -0.5f,-0.5f,-0.5f, 0f,0f,
            -0.5f,-0.5f, 0.5f, 1f,0f,
            -0.5f, 0.5f, 0.5f, 1f,1f,
            -0.5f, 0.5f,-0.5f, 0f,1f,

            // Top (+Y)
            -0.5f, 0.5f, 0.5f, 0f,0f,
             0.5f, 0.5f, 0.5f, 1f,0f,
             0.5f, 0.5f,-0.5f, 1f,1f,
            -0.5f, 0.5f,-0.5f, 0f,1f,

            // Bottom (-Y)
            -0.5f,-0.5f,-0.5f, 0f,0f,
             0.5f,-0.5f,-0.5f, 1f,0f,
             0.5f,-0.5f, 0.5f, 1f,1f,
            -0.5f,-0.5f, 0.5f, 0f,1f,
        };

        // 36 indices
        private readonly uint[] _indices =
        {
            0,1,2,  2,3,0,        // Front
            4,5,6,  6,7,4,        // Right
            8,9,10, 10,11,8,      // Back
            12,13,14, 14,15,12,   // Left
            16,17,18, 18,19,16,   // Top
            20,21,22, 22,23,20    // Bottom
        };

        public TextureCubeWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.12f, 0.15f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, Size.X, Size.Y);

            // --- Shaders ---
            const string vs = @"#version 330 core
            layout(location=0) in vec3 aPos;
            layout(location=1) in vec2 aUV;
            uniform mat4 uMVP;
            out vec2 vUV;
            void main(){
                vUV = aUV;
                gl_Position = uMVP * vec4(aPos, 1.0);
            }";
            const string fs = @"#version 330 core
            in vec2 vUV;
            out vec4 FragColor;
            uniform sampler2D uTex;
            void main(){
                FragColor = texture(uTex, vUV);
            }";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            CheckShader(v);

            int f = GL.CreateShader(ShaderType.FragmentShader);
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
            _uTex = GL.GetUniformLocation(_program, "uTex");

            // --- Buffers ---
            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            _ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            int stride = 5 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // --- Texture ---
            Directory.CreateDirectory("assets");
            string texPath = Path.Combine("assets", "texture.png");
            _tex0 = LoadTextureOrFallback(texPath);

            GL.UseProgram(_program);
            GL.Uniform1(_uTex, 0); // sampler bound to texture unit 0

            // Unbind (safety)
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // rotation animation (+ bonus)
            _angle += MathHelper.DegreesToRadians(60f) * (float)e.Time;

            var k = KeyboardState;
            if (k.IsKeyDown(Keys.Left))  _angle -= MathHelper.DegreesToRadians(120f) * (float)e.Time;
            if (k.IsKeyDown(Keys.Right)) _angle += MathHelper.DegreesToRadians(120f) * (float)e.Time;

            if (k.IsKeyDown(Keys.Escape)) Close();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Model/View/Projection
            var model = Matrix4.CreateRotationY(_angle) * Matrix4.CreateRotationX(_angle * 0.25f);
            var view  = Matrix4.LookAt(new Vector3(2f, 2f, 3f), Vector3.Zero, Vector3.UnitY);
            float aspect = Size.X / (float)Size.Y;
            var proj  = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), aspect, 0.1f, 100f);

            // OpenTK-friendly: model * view * proj, transpose=false
            var mvp = model * view * proj;

            GL.UseProgram(_program);
            GL.UniformMatrix4(_uMvp, false, ref mvp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _tex0);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteTexture(_tex0);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_program);
        }

        private static int LoadTextureOrFallback(string path)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            // Texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            if (File.Exists(path))
            {
                using var fs = File.OpenRead(path);
                var img = ImageResult.FromStream(fs, ColorComponents.RedGreenBlueAlpha);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                              img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, img.Data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            else
            {
                // 2x2 checker fallback (white/gray)
                byte[] pixels =
                {
                    255,255,255,255,  170,170,170,255,
                    170,170,170,255,  255,255,255,255
                };
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                              2, 2, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                Console.WriteLine($"[Warn] Texture not found at {path}. Using fallback checker.");
            }

            return tex;
        }

        private static void CheckShader(int shader)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(shader));
        }
        private static void CheckProgram(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetProgramInfoLog(program));
        }
    }
}
