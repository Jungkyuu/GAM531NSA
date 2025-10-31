using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Game.GLNS;

namespace Game;

public class GameWindow3D : GameWindow
{
    private Shader _shader = null!;
    private Texture _texture = null!;
    private Mesh _cube = null!;
    private Mesh _plane = null!;
    private Camera _camera = null!;
    private Vector3 _lightPos = new(1.5f, 2.0f, 1.5f);
    private bool _lightOn = true;
    private double _elapsed;

    public GameWindow3D(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

    protected override void OnLoad()
    {
        WindowState = OpenTK.Windowing.Common.WindowState.Fullscreen;
        base.OnLoad();
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.08f, 0.09f, 0.11f, 1f);

        _camera = new Camera(new Vector3(0f, 1.2f, 4.5f), Size.X / (float)Size.Y);
        CursorState = CursorState.Grabbed;

        _shader = new Shader("Shaders/vertex.glsl", "Shaders/fragment.glsl");
        _texture = new Texture("Assets/checker.png");

        _cube = MeshFactory.CreateTexturedCube();
        _plane = MeshFactory.CreatePlane(10f);
        _camera = new Camera(new Vector3(0f, 2.0f, 5.0f), Size.X / (float)Size.Y);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        
        GL.Viewport(0, 0, e.Width, e.Height);

        if (_camera is not null && e.Height > 0)
        {
            _camera.AspectRatio = e.Width / (float)e.Height;
        }
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        _elapsed += args.Time;
        var input = KeyboardState;

        if (input.IsKeyPressed(Keys.Escape))
        {
            if (CursorState == CursorState.Grabbed) CursorState = CursorState.Normal;
            else Close();
        }

        if (input.IsKeyPressed(Keys.E))
            _lightOn = !_lightOn;

        float speed = 3.5f;
        float dt = (float)args.Time;
        if (input.IsKeyDown(Keys.W)) _camera.Position += _camera.Forward * speed * dt;
        if (input.IsKeyDown(Keys.S)) _camera.Position -= _camera.Forward * speed * dt;
        if (input.IsKeyDown(Keys.A)) _camera.Position -= _camera.Right * speed * dt;
        if (input.IsKeyDown(Keys.D)) _camera.Position += _camera.Right * speed * dt;
        if (input.IsKeyDown(Keys.Space)) _camera.Position += _camera.Up * speed * dt;
        if (input.IsKeyDown(Keys.LeftShift)) _camera.Position -= _camera.Up * speed * dt;

        _lightPos.X = MathF.Cos((float)_elapsed) * 2.2f;
        _lightPos.Z = MathF.Sin((float)_elapsed) * 2.2f;
        _lightPos.Y = 1.8f;
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        if (!IsFocused || CursorState != CursorState.Grabbed) return;
        _camera.ProcessMouse((float)e.DeltaX, (float)e.DeltaY);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _shader.SetMatrix4("uView", _camera.GetViewMatrix());
        _shader.SetMatrix4("uProj", _camera.GetProjectionMatrix());
        _shader.SetVector3("uViewPos", _camera.Position);
        _shader.SetVector3("uLightPos", _lightPos);
        _shader.SetInt("uLightOn", _lightOn ? 1 : 0);

        _texture.Bind(TextureUnit.Texture0);
        _shader.SetInt("uTex0", 0);

        _plane.Model = Matrix4.Identity;
        _shader.SetMatrix4("uModel", _plane.Model);
        _plane.Draw();

        _cube.Model = Matrix4.CreateTranslation(-1.25f, 0.5f, 0f);
        _shader.SetMatrix4("uModel", _cube.Model);
        _cube.Draw();

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _cube.Dispose();
        _plane.Dispose();
        _texture.Dispose();
        _shader.Dispose();
    }
}