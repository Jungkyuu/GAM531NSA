// File: Program.cs
//
// Cross-version note:
// Use positional args for CreateOrthographicOffCenter(0, 800, 0, 600, -1, 1) to avoid API name drift.
//
// This version implements:
// - Clean separation of concerns (Movement vs Animator).
// - Finite State Machine (Idle/Walk/Run/Jump) with smooth transitions.
// - Last-frame hold rule (matches the base demo).
// - Jump physics (velocity + gravity), Shift sprint, and optional dedicated jump row.
// Comments are written in a clear, Canadian student style.

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.IO;
using ImageSharp = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenTK_Sprite_Animation
{
    public class SpriteAnimationGame : GameWindow
    {
        private Character _character;
        private int _shaderProgram;
        private int _vao, _vbo;
        private int _texture;

        // Cached locations for per-frame updates.
        private int _modelLoc;

        public SpriteAnimationGame()
            : base(new GameWindowSettings(),
                   new NativeWindowSettings { Size = (800, 600), Title = "Sprite Animation" })
        { }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _shaderProgram = CreateShaderProgram();
            _texture = LoadTexture("Sprite_Character.png");

            // Quad: [pos.x, pos.y, uv.x, uv.y] in model space (centered quad).
            float w = 32f, h = 64f; // half-size → on-screen 64x128
            float[] vertices =
            {
                -w, -h, 0f, 0f,
                 w, -h, 1f, 0f,
                 w,  h, 1f, 1f,
                -w,  h, 0f, 1f
            };

            _vao = GL.GenVertexArray();
            GL.BindVertexArray(_vao);

            _vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // aPosition
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            // aTexCoord
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.UseProgram(_shaderProgram);

            // Bind sampler to unit 0.
            int texLoc = GL.GetUniformLocation(_shaderProgram, "uTexture");
            GL.Uniform1(texLoc, 0);

            // Orthographic projection in pixel coords.
            int projLoc = GL.GetUniformLocation(_shaderProgram, "projection");
            Matrix4 ortho = Matrix4.CreateOrthographicOffCenter(0, 800, 0, 600, -1, 1);
            GL.UniformMatrix4(projLoc, false, ref ortho);

            // Initial model transform (centre).
            _modelLoc = GL.GetUniformLocation(_shaderProgram, "model");
            Matrix4 model = Matrix4.CreateTranslation(400, 300, 0);
            GL.UniformMatrix4(_modelLoc, false, ref model);

            _character = new Character(_shaderProgram);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            var kb = KeyboardState;
            if (kb.IsKeyDown(Keys.Escape)) Close();

            _character.Update((float)e.Time, kb);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Update model transform from character world-space position.
            Matrix4 model = Matrix4.CreateTranslation(_character.Position.X, _character.Position.Y, 0f);
            GL.UseProgram(_shaderProgram);
            GL.UniformMatrix4(_modelLoc, false, ref model);

            // Bind texture & VAO and draw.
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _texture);
            GL.BindVertexArray(_vao);

            _character.Render();

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteProgram(_shaderProgram);
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            base.OnUnload();
        }

        // --- Shader utilities ----------------------------------------------------

        private int CreateShaderProgram()
        {
            string vs = @"
#version 330 core
layout(location = 0) in vec2 aPosition;
layout(location = 1) in vec2 aTexCoord;
out vec2 vTexCoord;
uniform mat4 projection;
uniform mat4 model;
void main() {
    gl_Position = projection * model * vec4(aPosition, 0.0, 1.0);
    vTexCoord = vec2(aTexCoord.x, 1.0 - aTexCoord.y); // flip V for PNG origin
}";

            string fs = @"
#version 330 core
in vec2 vTexCoord;
out vec4 color;
uniform sampler2D uTexture;
uniform vec2 uOffset;  // normalized UV start (0..1)
uniform vec2 uSize;    // normalized UV size  (0..1)
void main() {
    vec2 uv = uOffset + vTexCoord * uSize;
    color = texture(uTexture, uv);
}";

            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            CheckShaderCompile(v, "VERTEX");

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            CheckShaderCompile(f, "FRAGMENT");

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            CheckProgramLink(p);

            GL.DetachShader(p, v);
            GL.DetachShader(p, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);
            return p;
        }

        private static void CheckShaderCompile(int shader, string stage)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception($"{stage} SHADER COMPILE ERROR:\n{GL.GetShaderInfoLog(shader)}");
        }

        private static void CheckProgramLink(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0) throw new Exception($"PROGRAM LINK ERROR:\n{GL.GetProgramInfoLog(program)}");
        }

        // --- Texture --------------------------------------------------------------------------

        private int LoadTexture(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}", path);

            using var img = ImageSharp.Load<Rgba32>(path);

            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            var pixels = new byte[4 * img.Width * img.Height];
            img.CopyPixelDataTo(pixels);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                          img.Width, img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            // Crisp sampling between atlas frames (prevents bleeding).
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            // Avoid wrap artefacts on frame borders.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            return tex;
        }
    }

    // --- Compatibility enum (kept) -------------------------------------------------------------
    public enum Direction { None, Right, Left }

    // --- FSM states actually useing ------------------------------------------------------------
    public enum PlayerState { Idle, Walk, Run, Jump }

    // ===========================================================================================
    // Character: composes Movement (physics + input) and Animator (state machine + frames)
    // ===========================================================================================
    public class Character
    {
        private readonly int _shader;

        // World-space anchor; the Game updates the model matrix from this.
        public Vector2 Position = new(400f, 300f);

        // Keep sheet/atlas constants here for a single source of truth.
        private const float FrameW = 64f;
        private const float FrameH = 128f;
        private const int   Columns = 4;
        private const int   Rows    = 2;   
        private const float Gap     = 60f; // horizontal gap in pixels

        private static float SheetW => Columns * FrameW + (Columns - 1) * Gap;
        private static float SheetH => Rows * FrameH;

        // If/when a jump row is added to the PNG, set JumpRowIndex = 2 and Rows = 3.
        private const int JumpRowIndex = 0; // For now reuse row 0. Switch to 2 when adds the row.

        // Movement & Animator are cleanly separated and talk via simple properties/callbacks.
        private readonly Movement _movement;
        private readonly Animator _animator;

        public Character(int shader)
        {
            _shader = shader;

            // Wire animator with a small callback that applies UVs into the shader.
            _animator = new Animator(
                applyFrame: (col, row, faceRight) => ApplyFrame(col, row, faceRight),
                jumpRowIndex: JumpRowIndex);

            _movement = new Movement(
                getPos:  () => Position,
                setPos:  p  => Position = p,
                setFace: right => _animator.FacingRight = right);

            _animator.Play(PlayerState.Idle); // show something right away
        }

        public void Update(float dt, KeyboardState kb)
        {
            _movement.HandleInput(kb);
            _movement.Integrate(dt);            // updates Position and grounded state
            _animator.Resolve(_movement);       // decide next PlayerState from movement status
            _animator.Step(dt);                 // advance frames with last-frame hold rule
        }

        public void Render()
        {
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
        }

        // Applies UV sub-rect (uOffset/uSize) for a frame. Keeps all GL touchpoints local.
        private void ApplyFrame(int col, int row, bool facingRight)
        {
            // Wrap row safely into [0, Rows)
            row = ((row % Rows) + Rows) % Rows;

            // Pixel-space start (account for horizontal gap).
            float uPx = col * (FrameW + Gap);
            float vPx = row * FrameH;

            // Normalize to 0..1
            float u  = uPx / SheetW;
            float v  = vPx / SheetH;
            float uw = FrameW / SheetW;
            float vh = FrameH / SheetH;

            // Horizontal flip by making width negative (simple and branch-free in shader).
            if (!facingRight)
            {
                u  = u + uw;
                uw = -uw;
            }

            GL.UseProgram(_shader);
            int off = GL.GetUniformLocation(_shader, "uOffset");
            int sz  = GL.GetUniformLocation(_shader, "uSize");
            GL.Uniform2(off, u, v);
            GL.Uniform2(sz,  uw, vh);
        }

        // ---------------------------------------------------------------------------------------
        // Movement: physics + input (clean, testable, single-responsibility)
        // ---------------------------------------------------------------------------------------
        private sealed class Movement
        {
            // Injected getters/setters so Movement doesn't know about GL or Character internals.
            private readonly Func<Vector2> _getPos;
            private readonly Action<Vector2> _setPos;
            private readonly Action<bool> _setFacing;

            // Public status queried by Animator.
            public bool Grounded { get; private set; } = true;
            public bool Moving   => MathF.Abs(_moveAxis) > 0f;
            public bool Sprint   { get; private set; }
            public bool FacingRight { get; private set; } = true;

            private float _moveAxis;       // -1..1 from keyboard
            private Vector2 _vel;          // px/s

            // Tunables (simple and readable numbers; tweak to taste).
            private const float WalkSpeed    = 180f;
            private const float RunSpeed     = 320f;
            private const float JumpVelocity = 520f;
            private const float Gravity      = -1500f;
            private const float GroundY      = 300f;

            public Movement(Func<Vector2> getPos, Action<Vector2> setPos, Action<bool> setFace)
            {
                _getPos = getPos;
                _setPos = setPos;
                _setFacing = setFace;
            }

            public void HandleInput(KeyboardState kb)
            {
                _moveAxis = 0f;
                if (kb.IsKeyDown(Keys.Right)) _moveAxis += 1f;
                if (kb.IsKeyDown(Keys.Left))  _moveAxis -= 1f;

                Sprint = kb.IsKeyDown(Keys.LeftShift) || kb.IsKeyDown(Keys.RightShift);

                if (_moveAxis != 0f)
                {
                    FacingRight = _moveAxis > 0f;
                    _setFacing(FacingRight);
                }

                // Start jump from ground only (no mid-air jumps).
                if (kb.IsKeyPressed(Keys.Space) && Grounded)
                {
                    _vel.Y = JumpVelocity;
                    Grounded = false;
                }
            }

            public void Integrate(float dt)
            {
                // Horizontal velocity
                float target = 0f;
                if (_moveAxis != 0f) target = Sprint ? RunSpeed : WalkSpeed;
                _vel.X = _moveAxis * target;

                // Gravity
                if (!Grounded) _vel.Y += Gravity * dt;

                // Integrate world position
                var p = _getPos();
                p += _vel * dt;

                // Clamp to ground plane
                if (p.Y <= GroundY)
                {
                    p.Y = GroundY;
                    _vel.Y = 0f;
                    Grounded = true;
                }

                _setPos(p);
            }
        }

        // ---------------------------------------------------------------------------------------
        // Animator: state machine + frame stepping (no physics logic here)
        // ---------------------------------------------------------------------------------------
        private sealed class Animator
        {
            private readonly Action<int, int, bool> _apply; // (col, row, faceRight)
            private readonly int _jumpRowIndex;

            public bool FacingRight { get; set; } = true;

            private float _timer;
            private int _frame;       // 0..Frames-1 in current clip
            private PlayerState _state = PlayerState.Idle;
            private class Clip { public int Row, StartCol, Frames; public float Fps; public bool HoldLast; }

            private readonly System.Collections.Generic.Dictionary<PlayerState, Clip> _clips
                = new System.Collections.Generic.Dictionary<PlayerState, Clip>();

            public Animator(Action<int, int, bool> applyFrame, int jumpRowIndex)
            {
                _apply = applyFrame;
                _jumpRowIndex = jumpRowIndex;
                RegisterClips();
            }

            private void RegisterClips()
            {
                // Rows are defined for the "facing-right" strip; left is done by flipping UV.
                _clips[PlayerState.Idle] = new Clip { Row = 0, StartCol = 0, Frames = 1, Fps = 6,  HoldLast = true  };
                _clips[PlayerState.Walk] = new Clip { Row = 0, StartCol = 1, Frames = 3, Fps = 10, HoldLast = false };
                _clips[PlayerState.Run]  = new Clip { Row = 0, StartCol = 1, Frames = 3, Fps = 16, HoldLast = false };
                _clips[PlayerState.Jump] = new Clip { Row = _jumpRowIndex, StartCol = 0, Frames = 2, Fps = 12, HoldLast = true };
            }

            // Decide next state based on movement status; keeps transitions predictable.
            public void Resolve(Movement m)
            {
                PlayerState next;
                if (!m.Grounded) next = PlayerState.Jump;
                else if (!m.Moving) next = PlayerState.Idle;
                else next = m.Sprint ? PlayerState.Run : PlayerState.Walk;

                if (next != _state) Play(next);
            }

            // Step frames based on currently active clip. Enforces the "last-frame hold" rule.
            public void Step(float dt)
            {
                var clip = _clips[_state];

                // Hold when:
                // - The clip is flagged as HoldLast (e.g., Jump while airborne), or
                // - Walk/Run and movement input has stopped (handled by Resolve→Idle).
                if (clip.HoldLast) return;

                _timer += dt;
                float spf = 1f / MathF.Max(clip.Fps, 1f);
                while (_timer >= spf)
                {
                    _timer -= spf;
                    _frame = (_frame + 1) % clip.Frames;
                    _apply(clip.StartCol + _frame, clip.Row, FacingRight);
                }
            }

            public void Play(PlayerState s)
            {
                _state = s;
                _timer = 0f;
                _frame = 0;
                var clip = _clips[s];
                _apply(clip.StartCol, clip.Row, FacingRight); // show first frame immediately
            }

            // Public helper for initial state.
            public void PlayIdle() => Play(PlayerState.Idle);
        }

        // Convenience to kick off the first frame on construction.
        public void Play(PlayerState s) => _animator.Play(s);
    }

    // --- Entry point ---------------------------------------------------------------------------
    internal class Program
    {
        private static void Main()
        {
            using var game = new SpriteAnimationGame();
            game.Run();
        }
    }
}