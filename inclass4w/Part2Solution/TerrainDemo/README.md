## TerrainDemo – OpenTK Exercises 6~9

A simple terrain rendering demo in C#/.NET 9 using OpenTK 4.x and ImageSharp.
Implements Exercise 6 (heightmap landscape), Exercise 7 (VBO preparation), Exercise 8 (DrawArrays), and Exercise 9 (external GLSL shaders).

## Features

Loads a grayscale heightmap.png from assets/

Generates 128×128 terrain grid, scales and colors based on height

Exercise 6: Indexed quads, basic immediate rendering

Exercise 7: Builds vertexData[] array and uploads to GPU VBO

Exercise 8: Uses GL.DrawArrays with VAO/VBO for efficient rendering

Exercise 9: External shaders (shaders/vs.glsl, shaders/fs.glsl) with position + color attributes

## Requirements

.NET 9 SDK (or .NET 8+)

OpenTK 4.x

SixLabors.ImageSharp

A valid grayscale assets/heightmap.png (any image will be resized to 128×128)

## Setup & Run
```bash
# Restore and build
dotnet restore
dotnet build

# Run the project
dotnet run
```

## File Structure
TerrainDemo/
  Program.cs
  Game.cs
  assets/
    heightmap.png
  shaders/
    vs.glsl
    fs.glsl
  TerrainDemo.csproj
