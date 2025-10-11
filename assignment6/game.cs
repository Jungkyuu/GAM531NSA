using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Assignment6_FPSCamera
{
    public class Game : GameWindow
    {
        // GL handles
        private int _programColor;
        private int _vaoCube, _vboCube, _eboCube;
        private int _vaoGrid, _vboGrid;
        private int _uMvpColor;
        private int _gridVertexCount;

        // Camera & input
        private readonly Camera _cam = new Camera(new Vector3(0f, 1.5f, 5f), yawDeg: -90f, pitchDeg: 0f);
        private float _moveSpeed = 4.0f;          // m/s
        private float _mouseSensitivity = 0.12f;  // deg per pixel
        private bool _grabbed = true;
        private float _time;

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            
            base.OnLoad();
            GL.ClearColor(0.07f, 0.08f, 0.12f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(0, 0, Size.X, Size.Y);

            _programColor = CreateProgram(VertexSrcColor, FragmentSrcColor);
            _uMvpColor = GL.GetUniformLocation(_programColor, "uMVP");

            CreateCube();
            CreateGrid(size: 20, halfExtent: 10f, y: 0f);

            SetGrab(true);
            _cam.AddYawPitch(0f, -10f);
            _cam.Position = new Vector3(0f, 1.5f, 6f);   
            _cam.LookAtTarget(new Vector3(0f, 0.5f, 0f));
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteVertexArray(_vaoCube);
            GL.DeleteBuffer(_vboCube);
            GL.DeleteBuffer(_eboCube);
            GL.DeleteVertexArray(_vaoGrid);
            GL.DeleteBuffer(_vboGrid);
            GL.DeleteProgram(_programColor);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _time += (float)e.Time;

            var k = KeyboardState;

            // ESC: release -> ESC again to quit
            if (k.IsKeyPressed(Keys.Escape))
            {
                if (_grabbed) SetGrab(false);
                else Close();
            }
            // Tab: toggle mouse grab
            if (k.IsKeyPressed(Keys.Tab)) SetGrab(!_grabbed);

            // movement
            float speed = _moveSpeed * (k.IsKeyDown(Keys.LeftShift) ? 2f : 1f);
            float dt = (float)e.Time;

            if (k.IsKeyDown(Keys.W)) _cam.Position += _cam.Front * speed * dt;
            if (k.IsKeyDown(Keys.S)) _cam.Position -= _cam.Front * speed * dt;
            if (k.IsKeyDown(Keys.D)) _cam.Position += _cam.Right * speed * dt;
            if (k.IsKeyDown(Keys.A)) _cam.Position -= _cam.Right * speed * dt;
            if (k.IsKeyDown(Keys.E)) _cam.Position += _cam.Up * speed * dt;   // up
            if (k.IsKeyDown(Keys.Q)) _cam.Position -= _cam.Up * speed * dt;   // down

            // mouse look + zoom (only when grabbed)
            if (_grabbed)
            {
                var m = MouseState;
                var delta = m.Delta;
                if (delta.LengthSquared > 0)
                {
                    float yawDelta =  delta.X * _mouseSensitivity;   // left/right
                    float pitDelta = -delta.Y * _mouseSensitivity;   // up/down (invert Y)
                    _cam.AddYawPitch(yawDelta, pitDelta);
                }

                float scroll = m.ScrollDelta.Y;
                if (Math.Abs(scroll) > float.Epsilon)
                    _cam.Zoom(scroll * 2.5f); // FOV 30~90
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            float aspect = Size.X / (float)Size.Y;
            var view = _cam.GetViewMatrix();
            var proj = _cam.GetProjection(aspect);

            // draw grid
            GL.UseProgram(_programColor);
            var mvpGrid = Matrix4.Identity * view * proj;
            GL.UniformMatrix4(_uMvpColor, false, ref mvpGrid);
            GL.BindVertexArray(_vaoGrid);
            GL.DrawArrays(PrimitiveType.Lines, 0, _gridVertexCount);

            // draw cubes
            GL.BindVertexArray(_vaoCube);
            DrawCube(Matrix4.CreateRotationY(_time * 0.4f) * Matrix4.CreateTranslation(0f, 0.5f, 0f), view, proj);
            DrawCube(Matrix4.CreateRotationY(-_time * 0.6f) * Matrix4.CreateTranslation(3f, 0.5f, 2f), view, proj);
            DrawCube(Matrix4.CreateRotationY(_time * 0.3f) * Matrix4.CreateTranslation(-4f, 0.5f, -3f), view, proj);

            SwapBuffers();
        }

        // ---------- helpers ----------

        private void SetGrab(bool grab)
        {
            _grabbed = grab;
            CursorState = grab ? CursorState.Grabbed : CursorState.Normal;
        }

        private void DrawCube(Matrix4 model, Matrix4 view, Matrix4 proj)
        {
            var mvp = model * view * proj;
            GL.UniformMatrix4(_uMvpColor, false, ref mvp);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
        }

        private void CreateGrid(int size, float halfExtent, float y)
        {
            int lines = size + 1;
            float step = (halfExtent * 2f) / size;
            var verts = new List<float>();

            // lines along X (vary Z)
            for (int i = 0; i < lines; i++)
            {
                float z = -halfExtent + i * step;
                verts.AddRange(new[] { -halfExtent, y, z, 0.5f, 0.5f, 0.5f });
                verts.AddRange(new[] {  halfExtent, y, z, 0.5f, 0.5f, 0.5f });
            }
            // lines along Z (vary X)
            for (int i = 0; i < lines; i++)
            {
                float x = -halfExtent + i * step;
                verts.AddRange(new[] { x, y, -halfExtent, 0.5f, 0.5f, 0.5f });
                verts.AddRange(new[] { x, y,  halfExtent, 0.5f, 0.5f, 0.5f });
            }

            _gridVertexCount = verts.Count / 6;
            _vaoGrid = GL.GenVertexArray();
            _vboGrid = GL.GenBuffer();

            GL.BindVertexArray(_vaoGrid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboGrid);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Count * sizeof(float), verts.ToArray(), BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        private void CreateCube()
        {
            // 24 unique vertices (per-face color), 36 indices
            float[] v =
            {
                // pos                  // color
                // +Z (front)
                -0.5f,0.0f, 0.5f,   0.95f,0.55f,0.35f,
                 0.5f,0.0f, 0.5f,   0.95f,0.55f,0.35f,
                 0.5f,1.0f, 0.5f,   0.95f,0.75f,0.45f,
                -0.5f,1.0f, 0.5f,   0.95f,0.75f,0.45f,

                // +X (right)
                 0.5f,0.0f, 0.5f,   0.45f,0.75f,0.95f,
                 0.5f,0.0f,-0.5f,   0.45f,0.75f,0.95f,
                 0.5f,1.0f,-0.5f,   0.35f,0.55f,0.95f,
                 0.5f,1.0f, 0.5f,   0.35f,0.55f,0.95f,

                // -Z (back)
                 0.5f,0.0f,-0.5f,   0.55f,0.95f,0.55f,
                -0.5f,0.0f,-0.5f,   0.55f,0.95f,0.55f,
                -0.5f,1.0f,-0.5f,   0.35f,0.95f,0.55f,
                 0.5f,1.0f,-0.5f,   0.35f,0.95f,0.55f,

                // -X (left)
                -0.5f,0.0f,-0.5f,   0.95f,0.55f,0.85f,
                -0.5f,0.0f, 0.5f,   0.95f,0.55f,0.85f,
                -0.5f,1.0f, 0.5f,   0.85f,0.55f,0.95f,
                -0.5f,1.0f,-0.5f,   0.85f,0.55f,0.95f,

                // +Y (top)
                -0.5f,1.0f, 0.5f,   0.95f,0.95f,0.75f,
                 0.5f,1.0f, 0.5f,   0.95f,0.95f,0.75f,
                 0.5f,1.0f,-0.5f,   0.85f,0.95f,0.85f,
                -0.5f,1.0f,-0.5f,   0.85f,0.95f,0.85f,

                // -Y (bottom)
                -0.5f,0.0f,-0.5f,   0.35f,0.35f,0.35f,
                 0.5f,0.0f,-0.5f,   0.35f,0.35f,0.35f,
                 0.5f,0.0f, 0.5f,   0.45f,0.45f,0.45f,
                -0.5f,0.0f, 0.5f,   0.45f,0.45f,0.45f,
            };

            uint[] idx =
            {
                0,1,2, 2,3,0,      // front
                4,5,6, 6,7,4,      // right
                8,9,10, 10,11,8,   // back
                12,13,14, 14,15,12,// left
                16,17,18, 18,19,16,// top
                20,21,22, 22,23,20 // bottom
            };

            _vaoCube = GL.GenVertexArray();
            _vboCube = GL.GenBuffer();
            _eboCube = GL.GenBuffer();

            GL.BindVertexArray(_vaoCube);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboCube);
            GL.BufferData(BufferTarget.ArrayBuffer, v.Length * sizeof(float), v, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _eboCube);
            GL.BufferData(BufferTarget.ElementArrayBuffer, idx.Length * sizeof(uint), idx, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Tiny color-only shader (MVP * vertex color)
        private static readonly string VertexSrcColor = @"
        #version 330 core
        layout(location=0) in vec3 aPos;
        layout(location=1) in vec3 aColor;
        uniform mat4 uMVP;
        out vec3 vColor;
        void main(){ vColor=aColor; gl_Position=uMVP*vec4(aPos,1.0); }";

        private static readonly string FragmentSrcColor = @"
        #version 330 core
        in vec3 vColor;
        out vec4 FragColor;
        void main(){ FragColor=vec4(vColor,1.0); }";

        private static int CreateProgram(string vs, string fs)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out int okV);
            if (okV == 0) throw new Exception(GL.GetShaderInfoLog(v));

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out int okF);
            if (okF == 0) throw new Exception(GL.GetShaderInfoLog(f));

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out int okP);
            if (okP == 0) throw new Exception(GL.GetProgramInfoLog(p));

            GL.DeleteShader(v);
            GL.DeleteShader(f);
            return p;
        }
    }
}
