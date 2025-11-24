Assignment 9 – 3D Object Collision Detection in OpenTK

Seneca Polytechnic – GAM531
Student: Jung Kyu Mok

### 1. Overview

This project implements a small 3D environment using OpenTK (OpenGL 3.3) and demonstrates a complete 3D collision detection system between a first-person player and multiple static scene objects. The scene contains a floor, three walls, a wooden box, a door, and an NPC placeholder—each implemented as a GameObject with its own Transform and an AABB-based collider. The player can move freely with WASD keys, look around with the mouse, and interact with trigger objects (NPC, Door) by pressing E.

The primary goals of the assignment were to build a coherent 3D environment, implement collision detection using bounding volumes, separate rendering and physics logic, and integrate player movement with responsive collision handling. All four objectives have been fully implemented.

### 2. Collision Detection Method

This project uses Axis-Aligned Bounding Boxes (AABB) for collision detection.
Every GameObject has:

Transform (position, scale)

AABBCollider (min/max bounds around the object)

The AABBCollider performs overlap tests based on:

return (Min.X <= other.Max.X && Max.X >= other.Min.X) &&
       (Min.Y <= other.Max.Y && Max.Y >= other.Min.Y) &&
       (Min.Z <= other.Max.Z && Max.Z >= other.Min.Z);


AABBs were chosen because they:

Are simple and efficient

Work well for static level geometry (walls, floor, box, door)

Allow easy axis-based collision response

The collision system runs entirely in the PlayerController, not inside rendering code, ensuring clean separation of gameplay logic from OpenGL drawing.

### 3. Movement + Collision Integration

Player movement follows a clear pipeline:

Input
WASD movement is converted into a world-space movement vector based on camera forward/right.

Predictive Movement
Desired movement is applied separately along X and Z axes.

Collision Check
After each axis move, the player’s AABB is checked against all solid objects (walls, box, door).

Collision Response
If a collision is detected on a given axis, the movement on that axis is reverted, producing:

Hard blocking when facing a wall

Natural sliding when grazing along a surface

The result is smooth navigation with no jittering or clipping.

Trigger objects (NPC, Door) use isTrigger = true and have a separate mechanic:

If the player approaches within a set radius, it becomes the current interactable

Pressing E prints interaction text in the console
(Door also toggles between open/closed internally)


### 4. Scene Construction

The scene includes the following objects:

| Object    | Type              | Collider | Solid/Trigger    |
|-----------|-------------------|----------|------------------|
| Floor     | Large platform    | AABB     | Non-solid        |
| BackWall  | Wall              | AABB     | Solid            |
| LeftWall  | Wall              | AABB     | Solid            |
| RightWall | Wall              | AABB     | Solid            |
| Box       | Obstacle          | AABB     | Solid            |
| NPC       | Placeholder       | AABB     | Trigger          |
| Door      | Interactable door | AABB     | Solid + Trigger  |

Textures for the floor, walls, box, door, and NPC are stored in:
`Assets/Textures/`


The scene is coherent, fully closed, and the player cannot escape or clip through geometry.

### 5. Challenges and Solutions
5-1. Floor blocking the player

Initially, the floor was flagged as isSolid = true, preventing the player from moving.
Solution: Floor was set to isSolid = false to allow free movement.

5-2. Interactable detection inaccurate

Trigger detection sometimes only worked for the NPC.
Solution: Trigger logic was rewritten to detect objects using distance-based nearest interactable search, fixing reliability for both NPC and Door.

5-3. Texture tiling issues

The floor texture stretched incorrectly.
Solution: UV coordinates were adjusted and the shader allowed tile scaling.

### 6. Controls
Action	Key
Move	W A S D
Look	Mouse
Interact (NPC / Door)	E
Exit	Esc

### 7. Conclusion

This project successfully demonstrates 3D collision detection, object interaction, a functional first-person controller, and a fully built environment using OpenTK and OpenGL 3.3. The design follows clean architecture principles, ensuring readable, maintainable, and modular code. All assignment requirements—including collision detection, scene construction, movement integration, code quality, and polish—have been met.