using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

/// Handles player movement, collision detection and simple interactions
/// with trigger objects (NPC, doors, etc.).
public class PlayerController
{
    // Transform of the player capsule / box.
    public Transform Transform { get; }

    // Simple AABB collider around the player.
    public ICollider Collider { get; }

    
    // Currently focused interactable object (NPC, door, etc.).
    
    public GameObject? CurrentInteractable { get; private set; }

    private readonly float _moveSpeed = 4.0f;
    private readonly float _interactRadius = 2.0f;

    public PlayerController(Vector3 startPosition)
    {
        Transform = new Transform
        {
            Position = startPosition,
            Scale = new Vector3(1f, 2f, 1f) // roughly human-sized
        };

        // Half extents roughly match the scale.
        Collider = new AABBCollider(Transform, new Vector3(0.5f, 1.0f, 0.5f));
    }

    /// Updates movement, collision and interactions.
    public void Update(
        float deltaTime,
        KeyboardState keyboard,
        List<GameObject> objects,
        Vector3 forward,
        Vector3 right)
    {
        // 1) Compute desired movement based on WASD input.
        Vector3 moveDir = Vector3.Zero;

        if (keyboard.IsKeyDown(Keys.W))
            moveDir += forward;
        if (keyboard.IsKeyDown(Keys.S))
            moveDir -= forward;
        if (keyboard.IsKeyDown(Keys.D))
            moveDir += right;
        if (keyboard.IsKeyDown(Keys.A))
            moveDir -= right;

        if (moveDir.LengthSquared > 0f)
            moveDir.Normalize();

        Vector3 desiredMove = moveDir * _moveSpeed * deltaTime;

        // 2) Apply movement with collision resolution against solid objects.
        MoveWithCollision(desiredMove, objects);

        // 3) Detect interactable object near the player (NPC / door / etc.).
        UpdateCurrentInteractable(objects);

        // 4) Handle interaction input (E key).
        HandleInteractionInput(keyboard);
    }

    
    // Moves the player while preventing penetration into solid objects.
    // Movement is resolved axis by axis (X then Z) to create simple sliding.
    private void MoveWithCollision(Vector3 desiredMove, List<GameObject> objects)
    {
        const float epsilon = 0.0001f;

        // X axis
        if (MathF.Abs(desiredMove.X) > epsilon)
        {
            Transform.Position += new Vector3(desiredMove.X, 0f, 0f);
            if (IsCollidingWithSolids(objects))
            {
                // Undo movement if it causes a collision.
                Transform.Position -= new Vector3(desiredMove.X, 0f, 0f);
            }
        }

        // Z axis
        if (MathF.Abs(desiredMove.Z) > epsilon)
        {
            Transform.Position += new Vector3(0f, 0f, desiredMove.Z);
            if (IsCollidingWithSolids(objects))
            {
                Transform.Position -= new Vector3(0f, 0f, desiredMove.Z);
            }
        }
    }


    // Returns true if the player collider is intersecting any solid object.
    private bool IsCollidingWithSolids(List<GameObject> objects)
    {
        foreach (var obj in objects)
        {
            if (!obj.IsSolid || obj.Collider == null)
                continue;

            if (Collider.Intersects(obj.Collider))
                return true;
        }

        return false;
    }

    // Finds the closest trigger object within an interaction radius
    // and stores it in CurrentInteractable.
    // Uses distance instead of collider intersection so that solid
    // doors can still be interacted with while blocking movement.
    private void UpdateCurrentInteractable(List<GameObject> objects)
    {
        CurrentInteractable = null;
        float bestDistance = float.MaxValue;

        foreach (var obj in objects)
        {
            if (!obj.IsTrigger)
                continue;

            // Distance between player and object centers.
            float distance = (obj.Transform.Position - Transform.Position).Length;

            if (distance < _interactRadius && distance < bestDistance)
            {
                bestDistance = distance;
                CurrentInteractable = obj;
            }
        }
    }

    // Handles pressing the interaction key (E) with the current object.
    private void HandleInteractionInput(KeyboardState keyboard)
    {
        if (!keyboard.IsKeyPressed(Keys.E))
            return;

        if (CurrentInteractable == null)
            return;

        var obj = CurrentInteractable;

        if (obj.Name == "NPC")
        {
            Console.WriteLine("You talk to the NPC. (Example interaction)");
        }
        else if (obj.Name == "Door")
        {
            // Toggle the door between closed (solid) and open (non-solid).
            obj.IsSolid = !obj.IsSolid;

            if (obj.IsSolid)
            {
                Console.WriteLine("The door is now closed.");
            }
            else
            {
                Console.WriteLine("The door opens and you can walk through.");
            }
        }
        else
        {
            Console.WriteLine($"You interact with: {obj.Name}");
        }
    }
}
