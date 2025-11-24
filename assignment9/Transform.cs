using OpenTK.Mathematics;

// Simple transform component used by all scene objects.
// Stores position, scale and rotation in world space.
public class Transform
{
    public Vector3 Position;
    public Vector3 Scale;
    public Vector3 Rotation; // Euler angles (radians). 

    public Transform()
    {
        Position = Vector3.Zero;
        Scale = Vector3.One;
        Rotation = Vector3.Zero;
    }

    // Builds the model matrix for this transform.
    // Order: Scale -> Rotate (Y) -> Translate.
    public Matrix4 GetModelMatrix()
    {
        Matrix4 translation = Matrix4.CreateTranslation(Position);
        Matrix4 scale = Matrix4.CreateScale(Scale);
        Matrix4 rotationY = Matrix4.CreateRotationY(Rotation.Y);

        return scale * rotationY * translation;
    }
}
