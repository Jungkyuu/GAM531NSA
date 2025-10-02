# Assignment 4 – Texture Mapping in OpenGL with OpenTK

## Objective
Render a 3D object (cube) with a 2D texture using OpenGL + OpenTK. Implement correct UVs, load/bind a texture, and sample it in shaders. Depth testing and perspective are applied. The cube rotates continuously (+1 bonus).

## How to Run
```bash
dotnet restore
dotnet run
Place a texture image at ./assets/texture.png (PNG or JPG).
If the file is missing, the app uses a small fallback checker so it still runs.

Controls
Left / Right: adjust rotation direction/speed

Esc: exit

What’s Implemented
Project setup: OpenTK 4.9.1 + StbImageSharp for image loading

3D geometry: Cube with 24 vertices and per-face UVs, 36 indices

Texture management: GL.GenTexture, GL.BindTexture, GL.TexImage2D, parameters, GL.GenerateMipmap

Shaders:

Vertex: passes UVs and transforms with uMVP

Fragment: samples sampler2D uTex

Rendering: Perspective + depth test (no distortion); rotating model

Deliverables
Include:

Source code (this project)

At least one texture image at assets/texture.png

A screenshot of the running app