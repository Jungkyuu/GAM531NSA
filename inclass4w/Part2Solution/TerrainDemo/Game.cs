using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace WindowEngine
{
    public class Game : IDisposable
    {
        private readonly int winW, winH;

        public Game(int width, int height) { winW = width; winH = height; }

        private Stopwatch timer = new Stopwatch();

        // Heightmap
        const int N = 128; // heightmap size
        private float[,] h = new float[N, N];


        // GL resources
        private int vao, vbo, ebo;

        #pragma warning disable 0169, 0649
        private Shader? shader;              // kept for Exercise 6
        private int uMVP, uModel, uLightDir; // kept for Exercise 6
        private Shader? shaderArrays;        // kept for Exercise 8
        #pragma warning restore 0169, 0649


        // animation state
        private float time;


        // optional placeholder to match your original API
        private Surface screen = new Surface(1, 1);


        // simple Surface as in your sample (not used for GL)
        public class Surface { public int[] pixels; public int width, height; public Surface(int w, int h) { width = w; height = h; pixels = new int[w * h]; } }

        // Exercise7
        private float[]? vertexData;
        private int vboOnly;

        // Exercise 8: DrawArrays pipeline
        private int vaoArrays;         // VAO describing vertexData layout (aPos only)
        private int vertexCount;       // vertexData.Length / 3 (xyz per vertex)

        // Exercise 9: shader program + VAO/VBO for positions/colors
        private int program;       // linked GL program
        private int vaoShader;     // VAO that binds position/color VBOs
        private int vboPos;        // positions (xyz)
        private int vboCol;        // colors (rgb)
        private int uMVP_Loc;      // uniform location for MVP

        // FPS counter
        private DateTime fpsStart = DateTime.UtcNow;
        private int frameCount = 0;
        [StructLayout(LayoutKind.Sequential)]
        struct Vertex
        {
            public Vector3 Pos; public Vector3 Normal; public Vertex(Vector3 p, Vector3 n) { Pos = p; Normal = n; }
        }


        public void Init()
        {
            timer.Start();
            // --- GL state ---
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0.08f, 0.08f, 0.11f, 1f);

            // --- Step 1: load heightmap and build vertex data ---
            LoadHeightmap("assets/heightmap.png");
            BuildVertexDataFromHeightmap(scaleXY: 2.0f, heightScale: 0.4f);

            // --- Step 2: upload raw VBO (Exercise 7) ---
            CreateAndUploadVBO();
            SetupDrawArraysVAO(); // Exercise 8 helper (pos-only VAO)

            // --- Step 3: load external shaders (Exercise 9) ---
            LoadAndLinkShadersFromFiles("shaders/vs.glsl", "shaders/fs.glsl");

            // --- Step 4: build color buffer from positions ---
            if (vertexData is null || vertexData.Length == 0)
                throw new InvalidOperationException("vertexData is empty.");
            float[] colorData = BuildColorsFromPositions(vertexData);

            // --- Step 5: create VAO for shader program ---
            vboPos = GL.GenBuffer();
            vboCol = GL.GenBuffer();
            vaoShader = GL.GenVertexArray();

            GL.BindVertexArray(vaoShader);

            // positions at location 0
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboPos);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            // colors at location 1
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboCol);
            GL.BufferData(BufferTarget.ArrayBuffer, colorData.Length * sizeof(float), colorData, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);

            // unbind for safety
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // --- Step 6: cache vertex count ---
            vertexCount = vertexData.Length / 3;

            Console.WriteLine($"Exercise 9 initialized: {vertexCount} vertices ready.");
        }




        public void OnResize(int w, int h)
        {
            GL.Viewport(0, 0, Math.Max(1, w), Math.Max(1, h));
        }


        public void Tick(float dt)
        {
            time += dt;
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            RenderGL();
        }

        
        // Builds a VAO for the Exercise 7 VBO so we can DrawArrays with aPos only
        private void SetupDrawArraysVAO()
        {
            if (vboOnly == 0 || vertexData is null || vertexData.Length == 0)
                throw new InvalidOperationException("VBO or vertexData not ready.");

            // Create VAO
            vaoArrays = GL.GenVertexArray();
            GL.BindVertexArray(vaoArrays);

            // Bind the VBO as the source for attribute 0 (aPos)
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboOnly);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                index: 0,
                size: 3,
                type: VertexAttribPointerType.Float,
                normalized: false,
                stride: sizeof(float) * 3,
                offset: 0
            );

            // Unbind to keep state clean
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Cache vertex count (xyz per vertex)
            vertexCount = vertexData.Length / 3;
        }

        public void RenderGL()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Build MVP (use your existing model/view/proj)
            float aspect = 16f / 9f;
            int[] vp = new int[4]; GL.GetInteger(GetPName.Viewport, vp);
            if (vp[3] != 0) aspect = vp[2] / (float)vp[3];

            var proj = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(60f), aspect, 0.05f, 100f);

            var model =
                Matrix4.CreateRotationY((float)timer.Elapsed.TotalSeconds * 0.25f) *
                Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-30f));

            float fly = (float)timer.Elapsed.TotalSeconds * 0.15f;
            var eye = new Vector3(0f, 0.6f, 2.8f - fly);
            var center = new Vector3(0f, 0f, 0f);
            var up = Vector3.UnitY;
            var view = Matrix4.LookAt(eye, center, up);

            var mvp = model * view * proj;

            // --- DrawArrays path (Exercise 8) ---
            if (shaderArrays != null && vaoArrays != 0 && vertexCount > 0)
            {
                shaderArrays.Use();
                int locMVP = shaderArrays.Loc("uMVP");
                GL.UniformMatrix4(locMVP, false, ref mvp);

                GL.BindVertexArray(vaoArrays);
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
                GL.BindVertexArray(0);
            }


            // --- Exercise 9 draw path (external GLSL program with position+color) ---
            if (program != 0 && vaoShader != 0 && vertexCount > 0)
            {
                GL.UseProgram(program);
                GL.UniformMatrix4(uMVP_Loc, false, ref mvp);

                GL.BindVertexArray(vaoShader);
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
                GL.BindVertexArray(0);

                GL.UseProgram(0);
            }

            // keep Exercise 6 indexed draw to compare
            // (comment out if you want DrawArrays-only screenshot)
            /*
            shader!.Use();
            GL.UniformMatrix4(uMVP, false, ref mvp);
            GL.UniformMatrix4(uModel, false, ref model);
            GL.Uniform3(uLightDir, new Vector3(0.3f, 1.0f, 0.5f));
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            */

            // --- FPS counter (prints once per second) ---
            frameCount++;
            var now = DateTime.UtcNow;
            if ((now - fpsStart).TotalSeconds >= 1.0)
            {
                Console.WriteLine($"FPS: {frameCount}");
                frameCount = 0;
                fpsStart = now;
            }
        }


        private void LoadHeightmap(string path)
        {
            using var img = Image.Load<L8>(path);

            // make image 128 * 128
            if (img.Width != N || img.Height != N)
            {
                img.Mutate(x => x.Resize(N, N, KnownResamplers.Bicubic));
            }

            img.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < N; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < N; x++)
                    {
                        h[x, y] = row[x].PackedValue / 255f; // 0..1
                    }
                }
            });
        }



        private void BuildTerrainMesh(float scaleXY, float heightScale)
        {
            var vertices = new Vertex[N * N];
            float half = scaleXY * 0.5f;


            // positions
            for (int z = 0; z < N; z++)
            {
                for (int x = 0; x < N; x++)
                {
                    float px = (x / (float)(N - 1)) * scaleXY - half;
                    float pz = (z / (float)(N - 1)) * scaleXY - half;
                    float py = (h[x, z] - 0.5f) * 2f * heightScale; // -heightScale..+heightScale
                    vertices[x + z * N].Pos = new Vector3(px, py, pz);
                }
            }


            // normals via central differences
            Vector3 P(int x, int z)
            {
                x = Math.Clamp(x, 0, N - 1);
                z = Math.Clamp(z, 0, N - 1);
                return vertices[x + z * N].Pos;
            }
            for (int z = 0; z < N; z++)
            {
                for (int x = 0; x < N; x++)
                {
                    var px = P(x + 1, z) - P(x - 1, z);
                    var pz = P(x, z + 1) - P(x, z - 1);
                    var n = Vector3.Normalize(Vector3.Cross(pz, px));
                    vertices[x + z * N].Normal = n;
                }
            }


            // indices (two tris per quad) exercise6
            // indexCount = (N - 1) * (N - 1) * 6;
            // var indices = new int[indexCount];
            // int k = 0;
            // for (int z = 0; z < N - 1; z++)
            // {
            //     for (int x = 0; x < N - 1; x++)
            //     {
            //         int i0 = x + z * N;
            //         int i1 = (x + 1) + z * N;
            //         int i2 = x + (z + 1) * N;
            //         int i3 = (x + 1) + (z + 1) * N;


            //         indices[k++] = i0; indices[k++] = i2; indices[k++] = i1; // tri 1
            //         indices[k++] = i1; indices[k++] = i2; indices[k++] = i3; // tri 2
            //     }
            // }

            // GL buffers
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();


            GL.BindVertexArray(vao);


            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * Marshal.SizeOf<Vertex>(), vertices, BufferUsageHint.StaticDraw);


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            //Enable for exercise6
            // GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, BufferUsageHint.StaticDraw);


            int stride = Marshal.SizeOf<Vertex>();
            GL.EnableVertexAttribArray(0); // aPos
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);


            GL.EnableVertexAttribArray(1); // aNormal
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, sizeof(float) * 3);


            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            if (vbo != 0) GL.DeleteBuffer(vbo);
            if (ebo != 0) GL.DeleteBuffer(ebo);
            if (vao != 0) GL.DeleteVertexArray(vao);
            shader?.Dispose();
            if (vboPos != 0) GL.DeleteBuffer(vboPos);
            if (vboCol != 0) GL.DeleteBuffer(vboCol);
            if (vaoShader != 0) GL.DeleteVertexArray(vaoShader);
            if (program != 0) GL.DeleteProgram(program);
        }


        // Turn the height grid into a flat triangle list: (127*127 quads) * (2 tris) * (3 verts) * (xyz)
        private void BuildVertexDataFromHeightmap(float scaleXY, float heightScale)
        {
            int floatsPerCell = 2 * 3 * 3; // 18 floats per quad
            int cellCount = (N - 1) * (N - 1);
            vertexData = new float[cellCount * floatsPerCell];

            float half = scaleXY * 0.5f;

            Vector3 P(int ix, int iz)
            {
                float wx = (ix / (float)(N - 1)) * scaleXY - half;
                float wz = (iz / (float)(N - 1)) * scaleXY - half;
                float wy = (h[ix, iz] - 0.5f) * 2f * heightScale;
                return new Vector3(wx, wy, wz);
            }

            int k = 0;
            for (int z = 0; z < N - 1; z++)
            {
                for (int x = 0; x < N - 1; x++)
                {
                    var i0 = P(x, z);
                    var i1 = P(x + 1, z);
                    var i2 = P(x, z + 1);
                    var i3 = P(x + 1, z + 1);

                    // tri 1: i0, i2, i1
                    vertexData[k++] = i0.X; vertexData[k++] = i0.Y; vertexData[k++] = i0.Z;
                    vertexData[k++] = i2.X; vertexData[k++] = i2.Y; vertexData[k++] = i2.Z;
                    vertexData[k++] = i1.X; vertexData[k++] = i1.Y; vertexData[k++] = i1.Z;

                    // tri 2: i1, i2, i3
                    vertexData[k++] = i1.X; vertexData[k++] = i1.Y; vertexData[k++] = i1.Z;
                    vertexData[k++] = i2.X; vertexData[k++] = i2.Y; vertexData[k++] = i2.Z;
                    vertexData[k++] = i3.X; vertexData[k++] = i3.Y; vertexData[k++] = i3.Z;
                }
            }
        }

        private void CreateAndUploadVBO()
        {
            if (vertexData is null || vertexData.Length == 0)
                throw new InvalidOperationException("vertexData is empty. Build it first.");

            vboOnly = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboOnly);

            // Upload once, draw many times: StaticDraw is optimal for mostly-unchanged data.
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertexData.Length * sizeof(float),
                vertexData,
                BufferUsageHint.StaticDraw
            );

            // Create a VAO that describes how to read positions from the VBO
            vaoArrays = GL.GenVertexArray();
            GL.BindVertexArray(vaoArrays);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboOnly);

            // position at location 0: 3 floats tightly packed
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(
                index: 0,
                size: 3,
                type: VertexAttribPointerType.Float,
                normalized: false,
                stride: sizeof(float) * 3,
                offset: 0
            );

            // unbind to keep state clean
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // cache vertex count for DrawArrays
            vertexCount = vertexData!.Length / 3;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // Build color array (rgb) from vertex positions' y (height-based gradient)
        private float[] BuildColorsFromPositions(float[] positions /* length multiple of 3 */)
        {
            int count = positions.Length / 3;
            var colors = new float[count * 3];

            for (int i = 0; i < count; i++)
            {
                float y = positions[i * 3 + 1]; // height
                // Map y (~-heightScale..+heightScale) into 0..1
                float t = Math.Clamp((y + 0.2f) * 1.5f, 0f, 1f);

                // low -> high: ocean blue -> rock gray
                var low = new Vector3(0.05f, 0.20f, 0.35f);
                var high = new Vector3(0.60f, 0.60f, 0.60f);
                Vector3 c = Vector3.Lerp(low, high, t);

                colors[i * 3 + 0] = c.X;
                colors[i * 3 + 1] = c.Y;
                colors[i * 3 + 2] = c.Z;
            }
            return colors;
        }

        // ---------- Shader utilities (load/compile/link) ----------
        private static string LoadTextFile(string path)
        {
            // Resolve relative to executable directory
            string baseDir = AppContext.BaseDirectory;
            string fullPath = System.IO.Path.Combine(baseDir, path);
            if (!System.IO.File.Exists(fullPath))
                throw new FileNotFoundException($"Shader not found: {fullPath}");
            return System.IO.File.ReadAllText(fullPath);
        }

        private static int Compile(ShaderType type, string src)
        {
            int s = GL.CreateShader(type);
            GL.ShaderSource(s, src);
            GL.CompileShader(s);
            GL.GetShader(s, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(s));
            return s;
        }

        private static int LinkProgram(int vs, int fs)
        {
            int prog = GL.CreateProgram();
            GL.AttachShader(prog, vs);
            GL.AttachShader(prog, fs);
            GL.LinkProgram(prog);
            GL.GetProgram(prog, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetProgramInfoLog(prog));
            GL.DetachShader(prog, vs);
            GL.DetachShader(prog, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
            return prog;
        }

        private void LoadAndLinkShadersFromFiles(string vsPath, string fsPath)
        {
            // Load GLSL source from files
            string vsSrc = LoadTextFile(vsPath);
            string fsSrc = LoadTextFile(fsPath);

            // Compile and link into a GL program
            int vs = Compile(ShaderType.VertexShader, vsSrc);
            int fs = Compile(ShaderType.FragmentShader, fsSrc);
            program = LinkProgram(vs, fs);

            // Cache uniform location
            uMVP_Loc = GL.GetUniformLocation(program, "uMVP");
            if (uMVP_Loc < 0) throw new Exception("uniform uMVP not found.");
        }

        private const string VS = @"#version 330 core
        layout(location=0) in vec3 aPos;
        layout(location=1) in vec3 aNormal;


        uniform mat4 uMVP;
        uniform mat4 uModel;


        out vec3 vNormalWS;
        out float vHeight;


        void main(){
        vec4 posWS = uModel * vec4(aPos,1.0);
        vNormalWS = mat3(uModel) * aNormal;
        vHeight = aPos.y;
        gl_Position = uMVP * vec4(aPos,1.0);
        }";


            private const string FS = @"#version 330 core
        in vec3 vNormalWS;
        in float vHeight;
        out vec4 FragColor;


        uniform vec3 uLightDir;


        void main(){
        vec3 N = normalize(vNormalWS);
        vec3 L = normalize(-uLightDir);
        float diff = max(dot(N,L), 0.05);


        vec3 lowCol = vec3(0.05, 0.2, 0.35);
        vec3 midCol = vec3(0.12, 0.45, 0.18);
        vec3 highCol = vec3(0.5, 0.5, 0.5);


        float t = clamp((vHeight + 0.2) * 1.5, 0.0, 1.0);
        vec3 baseCol = mix(midCol, highCol, t);
        baseCol = mix(lowCol, baseCol, smoothstep(0.0, 0.2, vHeight));


        FragColor = vec4(baseCol * diff, 1.0);
        }";
    
        // Minimal shaders for DrawArrays (position only)
        private const string VS_POS_ONLY = @"
        #version 330 core
        layout(location=0) in vec3 aPos;
        uniform mat4 uMVP;
        out float vHeight;
        void main(){
            vHeight = aPos.y;
            gl_Position = uMVP * vec4(aPos,1.0);
        }";

        private const string FS_POS_ONLY = @"
        #version 330 core
        in float vHeight;
        out vec4 FragColor;
        void main(){
            // simple height-tinted color
            float t = clamp((vHeight + 0.2) * 1.5, 0.0, 1.0);
            vec3 lowCol  = vec3(0.05, 0.2, 0.35);
            vec3 highCol = vec3(0.6, 0.6, 0.6);
            vec3 c = mix(lowCol, highCol, t);
            FragColor = vec4(c, 1.0);
        }";

    }
}