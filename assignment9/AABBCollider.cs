using OpenTK.Mathematics;

public class AABBCollider : ICollider
{
    private readonly Transform _transform;
    private readonly Vector3 _halfSize;

    public AABBCollider(Transform transform, Vector3 halfSize)
    {
        _transform = transform;
        _halfSize = halfSize;
    }

    public Vector3 Min => _transform.Position - _halfSize;
    public Vector3 Max => _transform.Position + _halfSize;

    // Standard AABB vs AABB intersection test.
    // Checks if there is overlap along X, Y and Z axes.
 
    public bool Intersects(ICollider other)
    {
        return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
               (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
               (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);
    }
}
