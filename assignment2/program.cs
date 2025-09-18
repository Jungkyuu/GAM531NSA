using System;

namespace GAM531Math
{
    // Simple 3D vector with required ops
    public readonly struct Vector3
    {
        public readonly float X, Y, Z;

        public Vector3(float x, float y, float z) => (X, Y, Z) = (x, y, z);

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b)
            => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static float Dot(Vector3 a, Vector3 b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vector3 Cross(Vector3 a, Vector3 b)
            => new(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );

        public override string ToString()
            => $"({X:0.###}, {Y:0.###}, {Z:0.###})";
    }

    // Row-major 4x4 matrix for standard 3D transforms
    public sealed class Matrix4
    {
        // m[row, col]
        private readonly float[,] m = new float[4, 4];

        public float this[int r, int c]
        {
            get => m[r, c];
            set => m[r, c] = value;
        }

        public static Matrix4 Identity()
        {
            var I = new Matrix4();
            for (int i = 0; i < 4; i++) I[i, i] = 1f;
            return I;
        }

        public static Matrix4 Scaling(float sx, float sy, float sz)
        {
            var S = Identity();
            S[0, 0] = sx; S[1, 1] = sy; S[2, 2] = sz;
            return S;
        }

        // Rotation around Z axis by angle in radians
        public static Matrix4 RotationZ(float radians)
        {
            var R = Identity();
            float c = MathF.Cos(radians);
            float s = MathF.Sin(radians);
            R[0, 0] = c;  R[0, 1] = -s;
            R[1, 0] = s;  R[1, 1] =  c;
            // R[2,2] and R[3,3] remain 1
            return R;
        }

        public static Matrix4 operator *(Matrix4 A, Matrix4 B)
        {
            var C = new Matrix4();
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                {
                    float sum = 0f;
                    for (int k = 0; k < 4; k++)
                        sum += A[r, k] * B[k, c];
                    C[r, c] = sum;
                }
            return C;
        }

        // Transform a Vector3 as a point (x,y,z,1). Returns (x',y',z') / w if w != 1.
        public Vector3 TransformPoint(Vector3 v)
        {
            float x = v.X, y = v.Y, z = v.Z;
            float x2 = m[0,0]*x + m[0,1]*y + m[0,2]*z + m[0,3]*1f;
            float y2 = m[1,0]*x + m[1,1]*y + m[1,2]*z + m[1,3]*1f;
            float z2 = m[2,0]*x + m[2,1]*y + m[2,2]*z + m[2,3]*1f;
            float w  = m[3,0]*x + m[3,1]*y + m[3,2]*z + m[3,3]*1f;

            if (w != 0f && w != 1f)
                return new Vector3(x2 / w, y2 / w, z2 / w);
            return new Vector3(x2, y2, z2);
        }
    }

    public static class Program
    {
        static float DegToRad(float deg) => deg * MathF.PI / 180f;

        public static void Main()
        {
            // VECTOR DEMOS
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(-4, 0.5f, 2);

            Console.WriteLine("=== Vector Operations ===");
            Console.WriteLine($"a          = {a}");
            Console.WriteLine($"b          = {b}");
            Console.WriteLine($"a + b      = {a + b}");
            Console.WriteLine($"a - b      = {a - b}");
            Console.WriteLine($"dot(a,b)   = {Vector3.Dot(a, b):0.###}");
            Console.WriteLine($"cross(a,b) = {Vector3.Cross(a, b)}");

            // MATRIX DEMOS
            Console.WriteLine("\n=== Matrix Operations ===");
            var I = Matrix4.Identity();
            var S = Matrix4.Scaling(2f, 0.5f, 1.5f);
            var Rz = Matrix4.RotationZ(DegToRad(45f)); // rotate 45° around Z
            var RS = Rz * S; // apply S first, then Rz: RS * v

            var v = new Vector3(1, 0, 0);
            Console.WriteLine($"Identity * v = {I.TransformPoint(v)}  (should be same as v)");
            Console.WriteLine($"Scale(2,0.5,1.5) * v = {S.TransformPoint(v)}");
            Console.WriteLine($"RotateZ(45°) * v     = {Rz.TransformPoint(v)}");
            Console.WriteLine($"RotateZ * Scale * v  = {RS.TransformPoint(v)}");

            Console.WriteLine("\nProgram finished. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
