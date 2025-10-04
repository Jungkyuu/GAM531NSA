using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace WindowEngine
{
    // Draws into a CPU pixel buffer (0xRRGGBB), uploads it to a GL texture,
    // and renders a fullscreen textured quad (Core 3.3 friendly).
    public class Game
    {
        // CPU pixel buffer (0xRRGGBB)
        private Surface screen;

        // GL resources
        private int texId = 0;
        private int vao = 0, vbo = 0, ebo = 0;
        private int program = 0;

        private long frameCount = 0;

        private const float ZoomFactor = 0.10f; 
        private const float PanFactor  = 0.05f;


        private static int CreateColor(int r, int g, int b) => (r << 16) | (g << 8) | b;

        // Generic world range 
        private float worldMinX = -2f, worldMaxX = 2f;
        private float worldMinY = -2f, worldMaxY = 2f;

        // rotation angle
        private float angle = 0f;

        public Game(int width, int height)
        {
            screen = new Surface(width, height);
        }

        public void Init()
        {
            // --- Create texture that mirrors the CPU pixel buffer ---
            texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                screen.width, screen.height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            // --- Fullscreen quad (two triangles) with UVs ---
            float[] quad =
            {
                // pos.xy   uv.xy
                -1f, -1f,  0f, 0f,
                 1f, -1f,  1f, 0f,
                 1f,  1f,  1f, 1f,
                -1f,  1f,  0f, 1f
            };
            uint[] idx = { 0, 1, 2, 2, 3, 0 };

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quad.Length * sizeof(float), quad, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, idx.Length * sizeof(uint), idx, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); // position
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1); // uv
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            GL.BindVertexArray(0);

            program = CreateShader(
                // VS
                @"#version 330 core
                layout(location = 0) in vec2 aPos;
                layout(location = 1) in vec2 aUV;
                out vec2 vUV;
                void main() { vUV = aUV; gl_Position = vec4(aPos, 0.0, 1.0); }",
                // FS (flip Y so top-left of CPU buffer appears at top-left on screen)
                @"#version 330 core
                in vec2 vUV;
                out vec4 FragColor;
                uniform sampler2D uTex;
                void main() {
                    vec2 uv = vec2(vUV.x, 1.0 - vUV.y);
                    FragColor = texture(uTex, uv);
                }"
            );

            GL.ClearColor(0f, 0f, 0f, 1f);
        }


        // Enable which you want to run
        public void Tick(double deltaTime)
        {
            // Clear background to black on the CPU buffer
            Array.Clear(screen.pixels, 0, screen.pixels.Length);

            // Exercise 1
            //DrawExercise1_BlueSquare();

            // Exercise 2
            //DrawExercise2_RgGradientWithBlueTint();

            // Exercise 3
            //DrawExercise3_SpinningSquare(deltaTime);

            // Exercise 4
            DrawExercise4_GenericTransform(deltaTime);


            // Upload to texture and render the fullscreen quad
            UploadPixelsToTexture();
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(program);
            int loc = GL.GetUniformLocation(program, "uTex");
            GL.Uniform1(loc, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // advance animation
            frameCount++;
        }



        // Exercise 1 core: per-column blue gradient inside a centered 300x300 square
        private void DrawExercise1_BlueSquare()
        {
            int w = screen.width, h = screen.height;

            // Square dimensions
            int squareW = 300, squareH = 300;

            // Center position
            int cx = w / 2, cy = h / 2;
            int left = cx - squareW / 2;
            int top = cy - squareH / 2;

            // Draw: each column uses a unique blue shade 0..255; rows are identical
            for (int y = 0; y < squareH; y++)
            {
                int sy = top + y;
                if ((uint)sy >= (uint)h) continue;

                for (int x = 0; x < squareW; x++)
                {
                    int sx = left + x;
                    if ((uint)sx >= (uint)w) continue;

                    // Column-based blue gradient (0..255)
                    int blue = (int)(255.0 * x / Math.Max(1, squareW - 1)); // 0..255
                    int color = blue; // 0x0000BB (blue channel only)
                    int location = sx + sy * w; // pixel index = x + y * width
                    screen.pixels[location] = color;
                }
            }
        }


        // Exercise 2: red-green gradient (x -> R, y -> G) + time-fading blue via sin
        private void DrawExercise2_RgGradientWithBlueTint()
        {
            int w = screen.width, h = screen.height;

            // speed factor for the blue tint oscillation
            double t = frameCount * 0.05;           // adjust 0.05 for faster/slower pulsation
            int blue = (int)(127.5 * (1.0 + Math.Sin(t))); // 0..255

            for (int y = 0; y < h; y++)
            {
                // map y to 0..255 for green
                int g = (int)(255.0 * y / Math.Max(1, h - 1));

                int rowOffset = y * w;
                for (int x = 0; x < w; x++)
                {
                    // map x to 0..255 for red
                    int r = (int)(255.0 * x / Math.Max(1, w - 1));

                    screen.pixels[rowOffset + x] = CreateColor(r, g, blue);
                }
            }
        }


        // Exercise 3: spinning white square using world coords (-2..2) with TX/TY
        private void DrawExercise3_SpinningSquare(double deltaTime)
        {

            // rotation speed in radians per second
            float rotationSpeed = 1.0f; // ~1 rad/s ≈ 57 deg/s

            // update angle using deltaTime
            angle += rotationSpeed * (float)deltaTime;

            float scale = 1.0f;
            var basePts = new (float x, float y)[]
            {
                (-0.5f * scale, -0.5f * scale),
                ( 0.5f * scale, -0.5f * scale),
                ( 0.5f * scale,  0.5f * scale),
                (-0.5f * scale,  0.5f * scale),
            };

            float c = MathF.Cos(angle);
            float s = MathF.Sin(angle);

            var scr = new (int x, int y)[4];
            for (int i = 0; i < 4; i++)
            {
                // rotate in world space
                float rx = basePts[i].x * c - basePts[i].y * s; // x' = x cos a - y sin a
                float ry = basePts[i].x * s + basePts[i].y * c; // y' = x sin a + y cos a

                // map world [-2,2] -> screen, with Y inverted inside TY()
                scr[i] = (TX(-2f, 2f, rx), TY(-2f, 2f, ry));
            }

            // draw edges in white via screen.Line()
            for (int i = 0; i < 4; i++)
            {
                var a = scr[i];
                var b = scr[(i + 1) % 4];
                screen.Line(a.x, a.y, b.x, b.y, 0xFFFFFF);
            }
        }



        // Exercise 4: spinning white square using generic TX/TY and adjustable world ranges
        private void DrawExercise4_GenericTransform(double deltaTime)
        {
            // rotation speed in radians/sec (time-based for smoothness)
            float rotationSpeed = 1.0f; // ~57deg/s
            angle += rotationSpeed * (float)deltaTime;

            // Optional pulsation to show scaling robustness
            float scale = 1.0f + 0.15f * MathF.Sin(angle * 0.7f);

            // base unit square centered at origin
            var basePts = new (float x, float y)[]
            {
                (-0.5f * scale, -0.5f * scale),
                ( 0.5f * scale, -0.5f * scale),
                ( 0.5f * scale,  0.5f * scale),
                (-0.5f * scale,  0.5f * scale),
            };

            float c = MathF.Cos(angle), s = MathF.Sin(angle);

            var scr = new (int x, int y)[4];
            for (int i = 0; i < 4; i++)
            {
                float rx = basePts[i].x * c - basePts[i].y * s;
                float ry = basePts[i].x * s + basePts[i].y * c;

                scr[i] = (TX(worldMinX, worldMaxX, rx),
                        TY(worldMinY, worldMaxY, ry));
            }

            // draw edges in white
            for (int i = 0; i < 4; i++)
            {
                var a = scr[i];
                var b = scr[(i + 1) % 4];
                Line(a.x, a.y, b.x, b.y, 0xFFFFFF);
            }

            // draw a subtle frame showing world bounds
            DrawBounds();
        }

        private void DrawBounds()
        {
            // Map world corners to screen; note TY flips Y by definition
            int left   = TX(worldMinX, worldMaxX, worldMinX);
            int right  = TX(worldMinX, worldMaxX, worldMaxX);
            int top    = TY(worldMinY, worldMaxY, worldMaxY);
            int bottom = TY(worldMinY, worldMaxY, worldMinY);

            int col = 0x444444; // dark gray
            Line(left,  top,    right, top,    col);
            Line(right, top,    right, bottom, col);
            Line(right, bottom, left,  bottom, col);
            Line(left,  bottom, left,  top,    col);
        }
        private int TX(float minX, float maxX, float x)
        {
            float u = (x - minX) / MathF.Max(1e-6f, (maxX - minX));
            return (int)MathF.Round(u * (screen.width - 1));
        }

        // Map world Y [minY, maxY] -> screen Y [0, height) with Y inverted
        private int TY(float minY, float maxY, float y)
        {
            float u = (maxY - y) / MathF.Max(1e-6f, (maxY - minY));
            return (int)MathF.Round(u * (screen.height - 1));
        }

        // Simple Bresenham line writer into screen.pixels (color: 0xRRGGBB)
        private void Line(int x0, int y0, int x1, int y1, int color)
        {
            int w = screen.width, h = screen.height;

            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                if ((uint)x0 < (uint)w && (uint)y0 < (uint)h)
                    screen.pixels[x0 + y0 * w] = color;

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }


        // Convert 0xRRGGBB -> BGRA byte[] and upload via TexSubImage2D
        private void UploadPixelsToTexture()
        {
            byte[] tmp = new byte[screen.width * screen.height * 4];
            int p = 0;
            for (int i = 0; i < screen.pixels.Length; i++)
            {
                int c = screen.pixels[i];
                byte r = (byte)((c >> 16) & 255);
                byte g = (byte)((c >> 8) & 255);
                byte b = (byte)(c & 255);
                tmp[p++] = b;      // B
                tmp[p++] = g;      // G
                tmp[p++] = r;      // R
                tmp[p++] = 255;    // A
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.BindTexture(TextureTarget.Texture2D, texId);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, screen.width, screen.height,
                             PixelFormat.Bgra, PixelType.UnsignedByte, tmp);
        }

        private static int CreateShader(string vs, string fs)
        {
            int V = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(V, vs);
            GL.CompileShader(V);
            GL.GetShader(V, ShaderParameter.CompileStatus, out int okV);
            if (okV == 0) throw new Exception("VS: " + GL.GetShaderInfoLog(V));

            int F = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(F, fs);
            GL.CompileShader(F);
            GL.GetShader(F, ShaderParameter.CompileStatus, out int okF);
            if (okF == 0) throw new Exception("FS: " + GL.GetShaderInfoLog(F));

            int P = GL.CreateProgram();
            GL.AttachShader(P, V);
            GL.AttachShader(P, F);
            GL.LinkProgram(P);
            GL.GetProgram(P, GetProgramParameterName.LinkStatus, out int okP);
            if (okP == 0) throw new Exception("LINK: " + GL.GetProgramInfoLog(P));

            GL.DetachShader(P, V);
            GL.DetachShader(P, F);
            GL.DeleteShader(V);
            GL.DeleteShader(F);
            return P;
        }

        // Handle Z/X zoom and arrow-key pan for Exercise 4
        public void HandleInput(KeyboardState kb, double deltaTime)
        {
            // Reset world range
            if (kb.IsKeyPressed(Keys.R))
            {
                worldMinX = -2f; worldMaxX = 2f;
                worldMinY = -2f; worldMaxY = 2f;
            }

            float rx = worldMaxX - worldMinX;
            float ry = worldMaxY - worldMinY;
            float cx = (worldMinX + worldMaxX) * 0.5f;
            float cy = (worldMinY + worldMaxY) * 0.5f;

            // Zoom In (Z): shrink ranges about the center
            if (kb.IsKeyDown(Keys.Z))
            {
                float zx = rx * ZoomFactor;
                float zy = ry * ZoomFactor;
                worldMinX = cx - (rx - zx) * 0.5f; worldMaxX = cx + (rx - zx) * 0.5f;
                worldMinY = cy - (ry - zy) * 0.5f; worldMaxY = cy + (ry - zy) * 0.5f;
            }

            // Zoom Out (X): expand ranges about the center
            if (kb.IsKeyDown(Keys.X))
            {
                float zx = rx * ZoomFactor;
                float zy = ry * ZoomFactor;
                worldMinX = cx - (rx + zx) * 0.5f; worldMaxX = cx + (rx + zx) * 0.5f;
                worldMinY = cy - (ry + zy) * 0.5f; worldMaxY = cy + (ry + zy) * 0.5f;
            }

            // Recompute range for panning step
            rx = worldMaxX - worldMinX;
            ry = worldMaxY - worldMinY;
            float px = rx * PanFactor;
            float py = ry * PanFactor;

            if (kb.IsKeyDown(Keys.Left))  { worldMinX -= px; worldMaxX -= px; }
            if (kb.IsKeyDown(Keys.Right)) { worldMinX += px; worldMaxX += px; }
            if (kb.IsKeyDown(Keys.Up))    { worldMinY += py; worldMaxY += py; }
            if (kb.IsKeyDown(Keys.Down))  { worldMinY -= py; worldMaxY -= py; }

            // Avoid degenerate ranges
            const float EPS = 1e-3f;
            if (worldMaxX - worldMinX < EPS) { float m = (worldMinX + worldMaxX) * 0.5f; worldMinX = m - EPS/2; worldMaxX = m + EPS/2; }
            if (worldMaxY - worldMinY < EPS) { float m = (worldMinY + worldMaxY) * 0.5f; worldMinY = m - EPS/2; worldMaxY = m + EPS/2; }
        }

    }

    // Simple CPU pixel surface (0xRRGGBB)
    public class Surface
    {
        public int[] pixels;
        public readonly int width, height;

        public Surface(int width, int height)
        {
            this.width = width;
            this.height = height;
            pixels = new int[width * height];
        }

        public void Line(int x0, int y0, int x1, int y1, int color)
        {
            int w = width, h = height;

            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;

            while (true)
            {
                if ((uint)x0 < (uint)w && (uint)y0 < (uint)h)
                    pixels[x0 + y0 * w] = color;

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }
    }
}
