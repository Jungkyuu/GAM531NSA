# OpenTKReview

A minimal OpenTK 4.x project in C# (.NET 8) for learning real-time graphics.  
It opens an 800×600 window and renders a CPU pixel buffer to the screen via an OpenGL texture.  
Use it as a foundation for exercises on pixel manipulation, coordinate transforms, and basic 3D.

## Features
- Clean `GameWindow` setup (OpenGL 3.3 Core)
- CPU pixel buffer → GL texture blit (fullscreen quad)
- ESC to quit
- Optional interactive controls (zoom/pan) for plotting and transforms

## Prerequisites
- .NET 8 SDK
- Visual Studio 2022 (or VS Code/Rider)
- NuGet access

## Setup

```bash
# Clone the repository (clone the repo root, not a tree URL)
git clone https://github.com/Jungkyuu/GAM531NSA.git
cd GAM531NSA/inclass4w/Part1Solution

Install OpenTK (choose one):

Visual Studio: Project → Manage NuGet Packages… → Browse “OpenTK” → Install

CLI:

dotnet restore
dotnet add package OpenTK
```

## Run

```bash
Visual Studio: Build and Run (F5)
CLI:

dotnet run
```

You should see a black 800×600 window.

## How to Use / Assignments

This project contains multiple exercises (1–4).
To run a specific exercise, open Game.cs and toggle the calls inside Tick(...) (uncomment the line for the exercise you want; comment others).

```bash 

Example:

//DrawExercise1_BlueSquare();
//DrawExercise2_RgGradientWithBlueTint();
DrawExercise3_SpinningSquare(deltaTime);
//DrawExercise4_GenericTransform(deltaTime);

```


## Keyboard Controls (where applicable)

Z / X: Zoom in / out (±10% of current range)

Arrow Keys: Pan (±5% of current range)

R: Reset world range to defaults

ESC: Quit

## Screenshots (for submission)

Exercise 1: exercise1.png

Exercise 2: exercise2.png

Exercise 3: exercise3.png

Exercise 4: exercise4.png 

Extra Screenshot: Code.png (showing TX/TY code)

