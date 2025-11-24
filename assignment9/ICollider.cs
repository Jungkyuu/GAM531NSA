using OpenTK.Mathematics;

/// Common interface for all collider types.
/// For this assignment, use AABB (Axis-Aligned Bounding Boxes).
public interface ICollider
{
    // Minimum and maximum corners of the collider in world space
    Vector3 Min { get; }
    Vector3 Max { get; }

    // Returns true if this collider intersects another collider
    bool Intersects(ICollider other);
}
