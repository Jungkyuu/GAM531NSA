using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

// Main game window. Ties together:
// - Scene objects (walls, floor, box, NPC, door)
// - Player controller (movement + collision)
// - Camera
// - Rendering with textures
public class MyGame : GameWindow
{
    private Scene _scene = null!;
    private PlayerController _player = null!;

    private SimpleCamera _camera = null!;
    private SimpleShader _shader = null!;

    private int _vao;
    private int _vbo;

    private float _mouseSensitivity = 0.12f;

    // Textures
    private Texture2D _texFloor = null!;
    private Texture2D _texWall = null!;
    private Texture2D _texBox = null!;
    private Texture2D _texNPC = null!;
    private Texture2D _texDoor = null!;

    public MyGame(GameWindowSettings gws, NativeWindowSettings nws)
        : base(gws, nws)
    {
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Basic OpenGL settings
        GL.ClearColor(0.20f, 0.20f, 0.25f, 1.0f);
        GL.Enable(EnableCap.DepthTest);

        // Create scene and player
        _scene = new Scene();
        _scene.Initialize();

        // Start the player slightly closer to the center of the room
        _player = new PlayerController(new Vector3(0f, 0f, 3f));

        // First-person camera following the player
        float aspect = ClientSize.X / (float)ClientSize.Y;
        _camera = new SimpleCamera(_player.Transform.Position + new Vector3(0f, 1.7f, 0f), aspect);

        // Lock mouse cursor for FPS-style look
        CursorState = CursorState.Grabbed;

        // Shader and geometry
        _shader = new SimpleShader(VertexShaderSource, FragmentShaderSource);
        _shader.Use();
        _shader.SetInt("texture0", 0); // sampler2D uses texture unit 0

        CreateCubeGeometry();

        // Load textures (paths are relative to working directory)
        _texFloor = new Texture2D("Assets/Textures/floor.png");
        _texWall = new Texture2D("Assets/Textures/wall.png");
        _texBox = new Texture2D("Assets/Textures/box.png");
        _texDoor = new Texture2D("Assets/Textures/door.png");
        _texNPC = new Texture2D("Assets/Textures/npc.png");
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);
        _camera.AspectRatio = ClientSize.X / (float)ClientSize.Y;
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (!IsFocused)
            return;

        var keyboard = KeyboardState;

        // Exit application
        if (keyboard.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }

        // Mouse look
        Vector2 delta = MouseState.Delta;
        _camera.AddRotation(delta.X * _mouseSensitivity, delta.Y * _mouseSensitivity);

        // Horizontal forward/right vectors for WASD movement
        Vector3 forward = _camera.Front;
        forward.Y = 0f;
        if (forward.LengthSquared > 0f) forward.Normalize();

        Vector3 right = _camera.Right;
        right.Y = 0f;
        if (right.LengthSquared > 0f) right.Normalize();

        // Player movement + collision + interaction
        _player.Update((float)e.Time, keyboard, _scene.Objects, forward, right);

        // Camera follows player's head position
        _camera.Position = _player.Transform.Position + new Vector3(0f, 1.7f, 0f);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();

        Matrix4 view = _camera.GetViewMatrix();
        Matrix4 projection = _camera.GetProjectionMatrix();

        _shader.SetMatrix4("view", view);
        _shader.SetMatrix4("projection", projection);

        GL.BindVertexArray(_vao);

        foreach (var obj in _scene.Objects)
        {
            Matrix4 model = obj.Transform.GetModelMatrix();
            _shader.SetMatrix4("model", model);

            Texture2D tex;
            Vector3 tint;
            Vector2 uvScale;

            switch (obj.Name)
            {
                case "Floor":
                    tex = _texFloor;
                    tint = new Vector3(1f, 1f, 1f);
                    uvScale = new Vector2(8f, 8f);   // Large tiled floor
                    break;

                case "BackWall":
                case "LeftWall":
                case "RightWall":
                    tex = _texWall;
                    tint = new Vector3(1f, 1f, 1f);
                    uvScale = new Vector2(4f, 2f);   // Repeated brick pattern
                    break;

                case "Box":
                    tex = _texBox;
                    tint = new Vector3(1f, 1f, 1f);
                    uvScale = new Vector2(1f, 1f);
                    break;

                case "NPC":
                    tex = _texNPC;
                    tint = new Vector3(1f, 1f, 1f); // Color comes from npc.png
                    uvScale = new Vector2(1f, 1f);
                    break;

                case "Door":
                    tex = _texDoor;
                    tint = new Vector3(1f, 1f, 1f);
                    uvScale = new Vector2(1f, 2f); // Slight vertical repeat
                    break;

                default:
                    tex = _texWall;
                    tint = new Vector3(1f, 1f, 1f);
                    uvScale = new Vector2(1f, 1f);
                    break;
            }

            tex.Use(TextureUnit.Texture0);
            _shader.SetVector3("tintColor", tint);
            _shader.SetVector2("uvScale", uvScale);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteBuffer(_vbo);
        GL.DeleteVertexArray(_vao);

        _shader.Dispose();
        _texFloor.Dispose();
        _texWall.Dispose();
        _texBox.Dispose();
        _texDoor.Dispose();
        _texNPC.Dispose();
    }

    /// Creates a unit cube mesh with position and UV coordinates.
    private void CreateCubeGeometry()
    {
        float[] vertices =
        {
            // positions           // UV
            // Back
            -0.5f, -0.5f, -0.5f,   0f, 0f,
             0.5f, -0.5f, -0.5f,   1f, 0f,
             0.5f,  0.5f, -0.5f,   1f, 1f,
             0.5f,  0.5f, -0.5f,   1f, 1f,
            -0.5f,  0.5f, -0.5f,   0f, 1f,
            -0.5f, -0.5f, -0.5f,   0f, 0f,

            // Front
            -0.5f, -0.5f,  0.5f,   0f, 0f,
             0.5f, -0.5f,  0.5f,   1f, 0f,
             0.5f,  0.5f,  0.5f,   1f, 1f,
             0.5f,  0.5f,  0.5f,   1f, 1f,
            -0.5f,  0.5f,  0.5f,   0f, 1f,
            -0.5f, -0.5f,  0.5f,   0f, 0f,

            // Left
            -0.5f,  0.5f,  0.5f,   1f, 0f,
            -0.5f,  0.5f, -0.5f,   0f, 0f,
            -0.5f, -0.5f, -0.5f,   0f, 1f,
            -0.5f, -0.5f, -0.5f,   0f, 1f,
            -0.5f, -0.5f,  0.5f,   1f, 1f,
            -0.5f,  0.5f,  0.5f,   1f, 0f,

            // Right
             0.5f,  0.5f,  0.5f,   1f, 0f,
             0.5f,  0.5f, -0.5f,   0f, 0f,
             0.5f, -0.5f, -0.5f,   0f, 1f,
             0.5f, -0.5f, -0.5f,   0f, 1f,
             0.5f, -0.5f,  0.5f,   1f, 1f,
             0.5f,  0.5f,  0.5f,   1f, 0f,

            // Bottom
            -0.5f, -0.5f, -0.5f,   0f, 0f,
             0.5f, -0.5f, -0.5f,   1f, 0f,
             0.5f, -0.5f,  0.5f,   1f, 1f,
             0.5f, -0.5f,  0.5f,   1f, 1f,
            -0.5f, -0.5f,  0.5f,   0f, 1f,
            -0.5f, -0.5f, -0.5f,   0f, 0f,

            // Top
            -0.5f,  0.5f, -0.5f,   0f, 0f,
             0.5f,  0.5f, -0.5f,   1f, 0f,
             0.5f,  0.5f,  0.5f,   1f, 1f,
             0.5f,  0.5f,  0.5f,   1f, 1f,
            -0.5f,  0.5f,  0.5f,   0f, 1f,
            -0.5f,  0.5f, -0.5f,   0f, 0f
        };

        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        // Position attribute
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

        // UV attribute
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

        GL.BindVertexArray(0);
    }

    // Vertex shader with position + UV
    private const string VertexShaderSource = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
    TexCoord = aTexCoord;
}
";

    // Fragment shader with texture + tint color and UV tiling
    private const string FragmentShaderSource = @"
#version 330 core

in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D texture0;
uniform vec3 tintColor;
uniform vec2 uvScale;

void main()
{
    vec2 tiledUV = TexCoord * uvScale;
    vec4 tex = texture(texture0, tiledUV);
    FragColor = tex * vec4(tintColor, 1.0);
}
";

    // Simple first-person camera.
    private class SimpleCamera
    {
        public Vector3 Position;
        public float AspectRatio;

        public float Fov = MathHelper.DegreesToRadians(60f);

        public Vector3 Front => _front;
        public Vector3 Right => _right;
        public Vector3 Up => _up;

        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _right = Vector3.UnitX;
        private Vector3 _up = Vector3.UnitY;

        private float _yaw = -90f;   // facing -Z
        private float _pitch = -10f; // slightly looking down

        public SimpleCamera(Vector3 position, float aspect)
        {
            Position = position;
            AspectRatio = aspect;
            UpdateVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(Fov, AspectRatio, 0.1f, 100f);
        }

        // Adds yaw and pitch (in degrees) from mouse input.
        public void AddRotation(float deltaYaw, float deltaPitch)
        {
            _yaw += deltaYaw;
            _pitch -= deltaPitch;

            _pitch = MathHelper.Clamp(_pitch, -89f, 89f);

            UpdateVectors();
        }

        private void UpdateVectors()
        {
            Vector3 f;
            f.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            f.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            f.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));

            _front = Vector3.Normalize(f);
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }

    // Minimal shader wrapper for compiling GLSL shaders and setting uniforms.
    private class SimpleShader : IDisposable
    {
        public int Handle { get; }

        public SimpleShader(string vertexSrc, string fragmentSrc)
        {
            int vs = Compile(ShaderType.VertexShader, vertexSrc);
            int fs = Compile(ShaderType.FragmentShader, fragmentSrc);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vs);
            GL.AttachShader(Handle, fs);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int status);
            if (status == 0)
            {
                throw new Exception("Shader link error: " + GL.GetProgramInfoLog(Handle));
            }

            GL.DetachShader(Handle, vs);
            GL.DetachShader(Handle, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
        }

        private static int Compile(ShaderType type, string src)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, src);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int status);
            if (status == 0)
            {
                throw new Exception($"{type} compile error: {GL.GetShaderInfoLog(shader)}");
            }

            return shader;
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            if (loc != -1)
            {
                GL.UniformMatrix4(loc, false, ref value);
            }
        }

        public void SetVector3(string name, Vector3 value)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            if (loc != -1)
            {
                GL.Uniform3(loc, value);
            }
        }

        public void SetVector2(string name, Vector2 value)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            if (loc != -1)
            {
                GL.Uniform2(loc, value);
            }
        }

        public void SetInt(string name, int value)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            if (loc != -1)
            {
                GL.Uniform1(loc, value);
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
