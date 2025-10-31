# 🕹️ Mini 3D Explorer Game (C# + OpenTK)

This project is a **mini 3D first-person explorer game** built using **C# (.NET + OpenTK)**.  
It demonstrates key OpenGL concepts including:

- 3D rendering pipeline (VAO / VBO / EBO)
- First-person camera movement + mouse look
- Phong lighting (ambient / diffuse / specular)
- Texture mapping
- Interactive light toggle

The user can move freely in the scene, look around, and toggle a dynamic moving light.
Development Environment: macOS + VS Code + .NET CLI (no Visual Studio solution file required)
---

## 🎮 Gameplay Instructions

| Action | Key |
|-------|-----|
| Move forward | **W** |
| Move backward | **S** |
| Move left | **A** |
| Move right | **D** |
| Move up | **Space** |
| Move down | **Left Shift** |
| Toggle light | **E** |
| Release mouse / Quit | **ESC** |
| Mouse look | Move mouse |

---

## ✨ Features

✅ Windowed 3D game scene  
✅ FPS camera movement + mouse look  
✅ Per-pixel **Phong lighting**  
✅ Textured objects (plane + cube)  
✅ Interactive light — press **E** to toggle  
✅ Organized code structure:

- `Shader.cs`
- `Texture.cs`
- `Mesh.cs`
- `Camera.cs`

✅ Built with `.NET + OpenTK`

---

## 🧠 How to Build & Run

### ✅ Requirements

- **.NET 9 SDK**
- OS: macOS / Windows / Linux
- NuGet packages:
  - `OpenTK` 4.9.x
  - `StbImageSharp` 2.27.x

### ✅ Build

```bash
dotnet restore
dotnet build
dotnet run
```


## Project Structure
```bash
└── Game
    ├── Assets
    │   └── checker.png
    ├── GL
    │   ├── Camera.cs
    │   ├── Mesh.cs
    │   ├── Shader.cs
    │   └── Texture.cs
    ├── Game.cs
    ├── Game.csproj
    ├── Program.cs
    ├── README.md
    └── Shaders
        ├── fragment.glsl
        └── vertex.glsl
```

## Credits

Texture: Checkerboard texture from https://opengameart.org
 (royalty-free)

## Notes

Press ESC once to release mouse, press ESC again to exit game

Uses modern OpenGL (GLSL 330)

Tested on macOS with .NET + OpenTK