# GAM531 – Assignment 3: 3D Cube Rendering with OpenTK

## Library
- **OpenTK 4.9.1** (OpenGL 3.3 Core Profile)

## Implementation
- **Cube Geometry**: Defined using 8 unique vertices with RGB colors and 36 indices (12 triangles, 2 per face).
- **Shaders**:
  - Vertex shader applies an MVP (Model * View * Projection) transform.
  - Fragment shader outputs per-vertex color.
- **Matrices**:
  - **Projection**: Perspective projection (`60° FOV`, aspect ratio = window size, near = 0.1, far = 100).
  - **View**: Camera created with `LookAt(2,2,3, 0,0,0, up=Y)`.
  - **Model**: Rotation applied each frame around Y axis and slightly on X axis.
- **Transformations**:
  - Cube continuously rotates (auto-rotation).
  - Optional interaction: Arrow keys adjust rotation speed and direction.
- **OpenGL Settings**:
  - Depth testing enabled (`GL.Enable(DepthTest)`).
  - Viewport updated on load and resize.

## Example Output
When running `dotnet run`, a colorful 3D cube appears and rotates:

![The screenshot of the 3D Cube](<Screenshot 2025-09-24 at 9.34.07 PM.png>)

The cube shows proper 3D perspective (front and back faces visible, depth correct).

## How to Run
```bash
dotnet restore
dotnet run
