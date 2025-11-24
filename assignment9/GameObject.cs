// Represents an object in the 3D scene, such as walls, floor, box, NPC, or door.
// Each object has a Transform and an optional Collider.
// Rendering is handled in MyGame; logic stays here.
public class GameObject
{
    public string Name;
    public Transform Transform { get; }
    public ICollider? Collider { get; }

    
    // Solid objects block player movement (e.g., walls, boxes, closed doors).
    public bool IsSolid;

    // Trigger objects do not block movement, but I still detect when the player touches them
    // (e.g., NPCs, interaction zones around doors).
    public bool IsTrigger;

    public GameObject(string name, Transform transform, ICollider? collider,
        bool isSolid = true, bool isTrigger = false)
    {
        Name = name;
        Transform = transform;
        Collider = collider;
        IsSolid = isSolid;
        IsTrigger = isTrigger;
    }
}
