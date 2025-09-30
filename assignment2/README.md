# Assignment 2 – Vector & Matrix Operations in C#

## 🎯 Objective
The goal of this assignment is to implement and demonstrate basic vector and matrix operations in **C# with OpenTK**, forming the foundation of transformations in graphics programming.  
This work builds upon **Assignment 1 (rectangle rendering)** and extends it by applying transformations such as scaling, rotation, and translation.

---

## 📦 Requirements Implemented
- **Vector operations** (demonstrated conceptually, but not the focus here):
  - Addition, subtraction, dot product, cross product  
- **Matrix operations**:
  - Identity matrix  
  - Scaling  
  - Rotation (Z-axis)  
  - Translation  
  - Matrix multiplication  
- **Applied to geometry**:
  - A rectangle from Assignment 1 is rendered.  
  - Transformations are applied in the vertex shader using a uniform matrix (`uMVP`).  

---

## 🖥️ How to Run
1. Clone this repository and navigate to the project folder:
   ```bash
   cd assignment2
   ```

2. Restore dependencies:
   ```bash
   dotnet restore assignment2.csproj
   ```

3. Run the project:
   ```bash
   dotnet run --project assignment2.csproj
   ```

4. An OpenTK window will open and show a **rectangle** with applied transformations.

---

## 🎮 Controls
- The rectangle **rotates automatically** around the Z-axis.  
- Press **← (Left Arrow)** to rotate faster counter-clockwise.  
- Press **→ (Right Arrow)** to rotate faster clockwise.  
- Press **Esc** to close the window.

---

## 📊 Example Output
When running the program, the following happens:
- The rectangle is scaled (`1.2x` width, `0.8x` height).  
- It rotates around the Z-axis continuously.  
- It is translated slightly (`+0.25` in X, `+0.15` in Y).  
- Transformations are combined via matrix multiplication:  

\[
M = T \times R \times S \times I
\]

The rectangle will appear **rotated, scaled, and shifted** in the rendered window.

---

## 📚 Library Used
- **[OpenTK](https://opentk.net/)** (Open Toolkit library for C# OpenGL development)

---