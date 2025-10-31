using OpenTK.Mathematics;

namespace Game.GLNS;

public class Camera
{
    public Vector3 Position;
    public float Pitch = 0f;
    public float Yaw = -90f;
    public float AspectRatio;

    public Camera(Vector3 position, float aspect)
    {
        Position = position;
        AspectRatio = aspect;
    }

    public Vector3 Forward
    {
        get
        {
            float p = MathHelper.DegreesToRadians(Pitch);
            float y = MathHelper.DegreesToRadians(Yaw);
            return Vector3.Normalize(new Vector3(MathF.Cos(p) * MathF.Cos(y), MathF.Sin(p), MathF.Cos(p) * MathF.Sin(y)));
        }
    }

    public Vector3 Right => Vector3.Normalize(Vector3.Cross(Forward, Vector3.UnitY));
    public Vector3 Up => Vector3.Normalize(Vector3.Cross(Right, Forward));

    public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Forward, Up);
    public Matrix4 GetProjectionMatrix() => Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), AspectRatio, 0.1f, 100f);

    public void ProcessMouse(float dx, float dy)
    {
        const float sens = 0.08f;
        Yaw += dx * sens;
        Pitch -= dy * sens;
        Pitch = MathHelper.Clamp(Pitch, -89f, 89f);
    }
}