# GAM531 – Basic Vector & Matrix Ops (C# + OpenTK)

## Which library used?
- **OpenTK 5** (added via NuGet) to satisfy the course requirement.  
  > This assignment focuses on math; no OpenGL window is required.

## Implemented operations
### Vectors
- Addition, subtraction (`+`, `-`)
- Dot product
- Cross product

### Matrices (4×4, row-major)
- Identity
- Scaling
- Rotation around Z axis
- Matrix × Matrix multiplication
- Matrix × Vector (as point) transform (`TransformPoint`)

## How to build & run
```bash
dotnet build
dotnet run
