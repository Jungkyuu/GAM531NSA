using System.Collections.Generic;
using OpenTK.Mathematics;

/// Holds all static scene objects.
/// Responsible only for creating and storing objects, not for movement or rendering.
public class Scene
{
    public List<GameObject> Objects { get; } = new List<GameObject>();

    // Creates a small coherent 3D environment containing:
    // - Floor
    // - Back wall
    // - Left wall
    // - Right wall
    // - Box (solid obstacle)
    // - NPC (trigger only, for interaction)
    // - Door (solid + trigger, can be opened)
    // Each object has a Transform and an AABB collision volume.
    public void Initialize()
    {
        Objects.Clear();

        // 1) Floor (large thin box).
        // The floor has a collider for reference, but it does not block movement
        // because I do not simulate gravity in this assignment.
        var floorTransform = new Transform
        {
            Position = new Vector3(0f, -1f, 0f),
            Scale = new Vector3(20f, 0.2f, 20f)
        };
        var floorCollider = new AABBCollider(floorTransform, new Vector3(10f, 0.1f, 10f));
        Objects.Add(new GameObject("Floor", floorTransform, floorCollider, isSolid: false, isTrigger: false));

        // 2) Back wall (at Z = -8).
        var backWallTransform = new Transform
        {
            Position = new Vector3(0f, 0f, -8f),
            Scale = new Vector3(16f, 4f, 0.5f)
        };
        var backWallCollider = new AABBCollider(backWallTransform, new Vector3(8f, 2f, 0.25f));
        Objects.Add(new GameObject("BackWall", backWallTransform, backWallCollider, isSolid: true, isTrigger: false));

        // 3) Left wall (at X = -8).
        var leftWallTransform = new Transform
        {
            Position = new Vector3(-8f, 0f, 0f),
            Scale = new Vector3(0.5f, 4f, 16f)
        };
        var leftWallCollider = new AABBCollider(leftWallTransform, new Vector3(0.25f, 2f, 8f));
        Objects.Add(new GameObject("LeftWall", leftWallTransform, leftWallCollider, isSolid: true, isTrigger: false));

        // 4) Right wall (at X = +8).
        var rightWallTransform = new Transform
        {
            Position = new Vector3(8f, 0f, 0f),
            Scale = new Vector3(0.5f, 4f, 16f)
        };
        var rightWallCollider = new AABBCollider(rightWallTransform, new Vector3(0.25f, 2f, 8f));
        Objects.Add(new GameObject("RightWall", rightWallTransform, rightWallCollider, isSolid: true, isTrigger: false));

        // 5) Box in the center area (solid obstacle).
        var boxTransform = new Transform
        {
            Position = new Vector3(2f, -0.5f, -2f),
            Scale = new Vector3(2f, 1f, 2f)
        };
        var boxCollider = new AABBCollider(boxTransform, new Vector3(1f, 0.5f, 1f));
        Objects.Add(new GameObject("Box", boxTransform, boxCollider, isSolid: true, isTrigger: false));

        // 6) NPC: trigger only (does not block movement).
        // Placed near the left side so it is easy to see when the scene starts.
        var npcTransform = new Transform
        {
            Position = new Vector3(-4f, -0.5f, -3f),
            // made the NPC a bit larger
            Scale    = new Vector3(2.0f, 2.0f, 2.0f)
        };
        // collider half extents should match half of the visual scale
        var npcCollider = new AABBCollider(npcTransform, new Vector3(0.6f, 0.9f, 0.6f));

        Objects.Add(new GameObject(
            name: "NPC",
            transform: npcTransform,
            collider: npcCollider,
            isSolid: false,
            isTrigger: true
        ));

        // 7) Door: solid + trigger. The player can open it when close and pressing E.
        // Placed in the center of the back wall.
        var doorTransform = new Transform
        {
            Position = new Vector3(0f, -0.5f, -7.5f),
            // slightly wider and taller door
            Scale    = new Vector3(3f, 4f, 0.3f)
        };
        var doorCollider = new AABBCollider(doorTransform, new Vector3(1.5f, 2f, 0.15f));

        Objects.Add(new GameObject(
            "Door",
            doorTransform,
            doorCollider,
            isSolid: true,
            isTrigger: true
        ));

    }
}
