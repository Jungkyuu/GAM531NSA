using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace PhongLighting
{
    public class Game : GameWindow
    {
        // --- Cube (Phong) ---
        private int _vao, _vbo, _ebo, _program;
        private int _uModel, _uView, _uProj;
        private int _uLightPos, _uViewPos, _uLightColor, _uObjectColor;
        private int _uAmbient, _uSpecularStrength, _uShininess, _uLightIntensity;

        // --- Sun (billboard sprite) ---
        private int _sunVao, _sunVbo, _sunEbo, _sunProgram;
        private float _sunSize = 0.38f; // world size of the sprite

        // Object rotation (keyboard)
        private float _objYaw = 0f;
        private float _objPitch = 0f;

        // Orbit camera (mouse)
        private float _camYaw = 0f;
        private float _camPitch = 0f;
        private float _camDist = 3.0f; // closer so cube fills screen more
        private const float MinDist = 2.0f, MaxDist = 20.0f;
        private Vector3 _camTarget = Vector3.Zero;

        private int _uAttC, _uAttL, _uAttQ, _uRimStr, _uRimPow, _uGamma;
        // Lighting
        private Vector3 _lightPos = new(2f, 2f, 2f);
        private Vector3 _lightColor = new(1f, 1f, 1f);
        private float _lightIntensity = 1.0f; // scalar
        private Vector3 _objectColor = new(1f, 0.8f, 0.6f);
        private float _ambientStrength = 0.12f;
        private float _specularStrength = 0.5f;
        private float _shininess = 32f;

        // Projection
        private Matrix4 _proj;

        // Animation time (for sun pulse/flicker)
        private float _time = 0f;

        // 24-vertex cube (per-face normals) : pos(3) normal(3)
        private readonly float[] _vertices =
        {
            // Front (+Z)
            -0.5f,-0.5f, 0.5f,  0f,0f,1f,
             0.5f,-0.5f, 0.5f,  0f,0f,1f,
             0.5f, 0.5f, 0.5f,  0f,0f,1f,
            -0.5f, 0.5f, 0.5f,  0f,0f,1f,
            // Right (+X)
             0.5f,-0.5f, 0.5f,  1f,0f,0f,
             0.5f,-0.5f,-0.5f,  1f,0f,0f,
             0.5f, 0.5f,-0.5f,  1f,0f,0f,
             0.5f, 0.5f, 0.5f,  1f,0f,0f,
            // Back (-Z)
             0.5f,-0.5f,-0.5f,  0f,0f,-1f,
            -0.5f,-0.5f,-0.5f,  0f,0f,-1f,
            -0.5f, 0.5f,-0.5f,  0f,0f,-1f,
             0.5f, 0.5f,-0.5f,  0f,0f,-1f,
            // Left (-X)
            -0.5f,-0.5f,-0.5f, -1f,0f,0f,
            -0.5f,-0.5f, 0.5f, -1f,0f,0f,
            -0.5f, 0.5f, 0.5f, -1f,0f,0f,
            -0.5f, 0.5f,-0.5f, -1f,0f,0f,
            // Top (+Y)
            -0.5f, 0.5f, 0.5f,  0f,1f,0f,
             0.5f, 0.5f, 0.5f,  0f,1f,0f,
             0.5f, 0.5f,-0.5f,  0f,1f,0f,
            -0.5f, 0.5f,-0.5f,  0f,1f,0f,
            // Bottom (-Y)
            -0.5f,-0.5f,-0.5f,  0f,-1f,0f,
             0.5f,-0.5f,-0.5f,  0f,-1f,0f,
             0.5f,-0.5f, 0.5f,  0f,-1f,0f,
            -0.5f,-0.5f, 0.5f,  0f,-1f,0f,
        };

        private readonly uint[] _indices =
        {
            0,1,2, 2,3,0,     4,5,6, 6,7,4,
            8,9,10, 10,11,8,  12,13,14, 14,15,12,
            16,17,18, 18,19,16, 20,21,22, 22,23,20
        };

        public Game(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) {}

        // ------------------------ Setup ------------------------
        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.07f, 0.08f, 0.11f, 1f);
            GL.Enable(EnableCap.DepthTest);
            // Use full pixel framebuffer (HiDPI safe)
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);

            // Ensure Phong shader files exist (names must be phong.vert / phong.frag)
            Directory.CreateDirectory("shaders");
            var vertPath = Path.Combine("shaders", "phong.vert");
            var fragPath = Path.Combine("shaders", "phong.frag");
            if (!File.Exists(vertPath) || !File.Exists(fragPath))
            {
                File.WriteAllText(vertPath, DefaultPhongVert);
                File.WriteAllText(fragPath, DefaultPhongFrag);
                Console.WriteLine("[Info] Wrote default shaders to 'shaders/phong.vert/.frag'.");
            }

            // Compile Phong program
            _program = CreateProgramFromFiles(vertPath, fragPath);

            // Cache uniform locations
            _uModel = GL.GetUniformLocation(_program, "model");
            _uView = GL.GetUniformLocation(_program, "view");
            _uProj = GL.GetUniformLocation(_program, "projection");
            _uLightPos = GL.GetUniformLocation(_program, "lightPos");
            _uViewPos = GL.GetUniformLocation(_program, "viewPos");
            _uLightColor = GL.GetUniformLocation(_program, "lightColor");
            _uObjectColor = GL.GetUniformLocation(_program, "objectColor");
            _uAmbient = GL.GetUniformLocation(_program, "ambientStrength");
            _uSpecularStrength = GL.GetUniformLocation(_program, "specularStrength");
            _uShininess = GL.GetUniformLocation(_program, "shininess");
            _uLightIntensity = GL.GetUniformLocation(_program, "lightIntensity");
            _uAttC   = GL.GetUniformLocation(_program, "attConst");
            _uAttL   = GL.GetUniformLocation(_program, "attLinear");
            _uAttQ   = GL.GetUniformLocation(_program, "attQuad");
            _uRimStr = GL.GetUniformLocation(_program, "rimStrength");
            _uRimPow = GL.GetUniformLocation(_program, "rimPower");
            _uGamma  = GL.GetUniformLocation(_program, "enableGamma");

            // Cube buffers
            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

            int stride = 6 * sizeof(float);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // ---- Sun billboard program (embedded; no external files needed) ----
            _sunProgram = CreateProgramFromStrings(SunBillboardVert, SunBillboardFrag);

            // Sun quad (offsets in local billboard space: -0.5..0.5)
            float[] sunQuad = { -0.5f,-0.5f,  0.5f,-0.5f,  0.5f,0.5f,  -0.5f,0.5f };
            uint[]  sunIdx  = { 0,1,2, 2,3,0 };

            _sunVao = GL.GenVertexArray();
            _sunVbo = GL.GenBuffer();
            _sunEbo = GL.GenBuffer();

            GL.BindVertexArray(_sunVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _sunVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sunQuad.Length * sizeof(float), sunQuad, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _sunEbo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sunIdx.Length * sizeof(uint), sunIdx, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0); // aOffset
            GL.EnableVertexAttribArray(0);

            UpdateProjection(); // uses FramebufferSize
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
            UpdateProjection();
        }

        private void UpdateProjection()
        {
            float aspect = FramebufferSize.X / (float)FramebufferSize.Y;
            _proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), aspect, 0.1f, 100f);
        }

        // ------------------------ Input ------------------------
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            _time += (float)e.Time; // for sun animation

            var k = KeyboardState;
            var m = MouseState;

            if (k.IsKeyDown(Keys.Escape)) Close();

            // Reset framing
            if (k.IsKeyDown(Keys.R))
            {
                _camYaw = 0f; _camPitch = 0f; _camDist = 3.0f;
                _objYaw = 0f; _objPitch = 0f;
            }

            // Object rotation
            float spin = MathHelper.DegreesToRadians(90f) * (float)e.Time;
            if (k.IsKeyDown(Keys.Left))  _objYaw   -= spin;
            if (k.IsKeyDown(Keys.Right)) _objYaw   += spin;
            if (k.IsKeyDown(Keys.Up))    _objPitch -= spin;
            if (k.IsKeyDown(Keys.Down))  _objPitch += spin;

            // Light movement
            float ls = 2.0f * (float)e.Time;
            if (k.IsKeyDown(Keys.I)) _lightPos.X += ls;
            if (k.IsKeyDown(Keys.K)) _lightPos.X -= ls;
            if (k.IsKeyDown(Keys.J)) _lightPos.Z += ls;
            if (k.IsKeyDown(Keys.L)) _lightPos.Z -= ls;
            if (k.IsKeyDown(Keys.U)) _lightPos.Y += ls;
            if (k.IsKeyDown(Keys.O)) _lightPos.Y -= ls;

            // Light intensity
            if (k.IsKeyDown(Keys.LeftBracket))
                _lightIntensity = MathF.Max(0f, _lightIntensity - 1.5f * (float)e.Time);
            if (k.IsKeyDown(Keys.RightBracket))
                _lightIntensity = MathF.Min(8f, _lightIntensity + 1.5f * (float)e.Time);

            // Shininess
            if (k.IsKeyDown(Keys.Equal) || k.IsKeyDown(Keys.KeyPadAdd))
                _shininess = Math.Clamp(_shininess + 40f * (float)e.Time, 1f, 256f);
            if (k.IsKeyDown(Keys.Minus) || k.IsKeyDown(Keys.KeyPadSubtract))
                _shininess = Math.Clamp(_shininess - 40f * (float)e.Time, 1f, 256f);

            // Mouse orbit
            if (m.IsButtonDown(MouseButton.Left))
            {
                var d = m.Delta;
                _camYaw   -= d.X * 0.005f;
                _camPitch -= d.Y * 0.005f;
                _camPitch = MathHelper.Clamp(_camPitch, MathHelper.DegreesToRadians(-85f), MathHelper.DegreesToRadians(85f));
            }

            // Mouse wheel zoom
            if (m.ScrollDelta.Y != 0)
            {
                _camDist = MathHelper.Clamp(_camDist * (1f - m.ScrollDelta.Y * 0.1f), MinDist, MaxDist);
            }
        }

        // ------------------------ Render ------------------------
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Camera position from orbit
            float cx = _camTarget.X + _camDist * MathF.Cos(_camPitch) * MathF.Sin(_camYaw);
            float cy = _camTarget.Y + _camDist * MathF.Sin(_camPitch);
            float cz = _camTarget.Z + _camDist * MathF.Cos(_camPitch) * MathF.Cos(_camYaw);
            var camPos = new Vector3(cx, cy, cz);
            var view   = Matrix4.LookAt(camPos, _camTarget, Vector3.UnitY);

            // Object model
            var model  = Matrix4.CreateRotationX(_objPitch) * Matrix4.CreateRotationY(_objYaw);

            // --- Draw cube with Phong lighting ---
            GL.UseProgram(_program);
            GL.UniformMatrix4(_uModel, false, ref model);
            GL.UniformMatrix4(_uView, false, ref view);
            GL.UniformMatrix4(_uProj, false, ref _proj);
            GL.Uniform1(_uAttC, 1.0f);
            GL.Uniform1(_uAttL, 0.12f);   
            GL.Uniform1(_uAttQ, 0.032f);  

            
            GL.Uniform1(_uRimStr, 0.25f); 
            GL.Uniform1(_uRimPow, 2.0f);

            
            GL.Uniform1(_uGamma, 1);
            GL.Uniform3(_uLightPos, _lightPos);
            GL.Uniform3(_uViewPos, camPos);
            GL.Uniform3(_uLightColor, _lightColor);
            GL.Uniform3(_uObjectColor, _objectColor);
            GL.Uniform1(_uAmbient, _ambientStrength);
            GL.Uniform1(_uSpecularStrength, _specularStrength);
            GL.Uniform1(_uShininess, _shininess);
            GL.Uniform1(_uLightIntensity, _lightIntensity);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            // --- Compute exact camera basis in WORLD space for sun billboard ---
            var camForward = Vector3.Normalize(_camTarget - camPos);
            var camRight   = Vector3.Normalize(Vector3.Cross(camForward, Vector3.UnitY));
            var camUp      = Vector3.Normalize(Vector3.Cross(camRight, camForward));

            // --- Draw SUN billboard at light position (additive blend, animated) ---
            GL.UseProgram(_sunProgram);

            int uLightPos = GL.GetUniformLocation(_sunProgram, "lightPos");
            int uProj     = GL.GetUniformLocation(_sunProgram, "projection");
            int uView     = GL.GetUniformLocation(_sunProgram, "view");
            int uSize     = GL.GetUniformLocation(_sunProgram, "size");
            int uColor    = GL.GetUniformLocation(_sunProgram, "sunColor");
            int uInten    = GL.GetUniformLocation(_sunProgram, "intensity");
            int uTime     = GL.GetUniformLocation(_sunProgram, "time");
            int uR        = GL.GetUniformLocation(_sunProgram, "camRight");
            int uU        = GL.GetUniformLocation(_sunProgram, "camUp");

            GL.Uniform3(uLightPos, _lightPos);
            GL.UniformMatrix4(uProj, false, ref _proj);
            GL.UniformMatrix4(uView, false, ref view);
            GL.Uniform1(uSize, _sunSize);
            GL.Uniform3(uColor, new Vector3(1.0f, 0.92f, 0.55f)); // warm yellow
            GL.Uniform1(uInten, _lightIntensity);
            GL.Uniform1(uTime, _time);
            GL.Uniform3(uR, camRight);
            GL.Uniform3(uU, camUp);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.One, BlendingFactor.One); // additive
            GL.DepthMask(false);

            GL.BindVertexArray(_sunVao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.DepthMask(true);
            GL.Disable(EnableCap.Blend);

            Title = $"Phong Lighting | Light:{_lightPos} | Intensity:{_lightIntensity:F2} | Shininess:{_shininess:F0}";
            SwapBuffers();
        }

        // ------------------------ Cleanup ------------------------
        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_program);

            GL.DeleteVertexArray(_sunVao);
            GL.DeleteBuffer(_sunVbo);
            GL.DeleteBuffer(_sunEbo);
            GL.DeleteProgram(_sunProgram);
        }

        // ------------------------ Helpers ------------------------
        private static int CreateProgramFromFiles(string vPath, string fPath)
        {
            string vs = File.ReadAllText(vPath);
            string fs = File.ReadAllText(fPath);
            return CreateProgramFromStrings(vs, fs);
        }

        private static int CreateProgramFromStrings(string vs, string fs)
        {
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out int ok1);
            if (ok1 == 0) throw new Exception(GL.GetShaderInfoLog(v));

            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out int ok2);
            if (ok2 == 0) throw new Exception(GL.GetShaderInfoLog(f));

            int p = GL.CreateProgram();
            GL.AttachShader(p, v);
            GL.AttachShader(p, f);
            GL.LinkProgram(p);
            GL.GetProgram(p, GetProgramParameterName.LinkStatus, out int ok3);
            if (ok3 == 0) throw new Exception(GL.GetProgramInfoLog(p));

            GL.DeleteShader(v);
            GL.DeleteShader(f);
            return p;
        }

        // -------- Default Phong shaders --------
        private const string DefaultPhongVert = @"#version 330 core
        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec3 aNormal;

        out vec3 FragPos;
        out vec3 Normal;

        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;

        void main()
        {
            vec4 worldPos = model * vec4(aPosition, 1.0);
            FragPos = worldPos.xyz;

            mat3 normalMat = mat3(transpose(inverse(model)));
            Normal = normalize(normalMat * aNormal);

            gl_Position = projection * view * worldPos;
        }";

        private const string DefaultPhongFrag = @"#version 330 core
        out vec4 FragColor;

        in vec3 FragPos;
        in vec3 Normal;

        uniform vec3 lightPos;
        uniform vec3 viewPos;
        uniform vec3 lightColor;
        uniform vec3 objectColor;

        uniform float ambientStrength;
        uniform float specularStrength;
        uniform float shininess;
        uniform float lightIntensity;

        void main()
        {
            vec3 light = lightColor * lightIntensity;

            // Ambient
            vec3 ambient = ambientStrength * light;

            // Diffuse
            vec3 N = normalize(Normal);
            vec3 L = normalize(lightPos - FragPos);
            float diff = max(dot(N, L), 0.0);
            vec3 diffuse = diff * light;

            // Specular (Phong)
            vec3 V = normalize(viewPos - FragPos);
            vec3 R = reflect(-L, N);
            float spec = pow(max(dot(V, R), 0.0), shininess);
            vec3 specular = specularStrength * spec * light;

            vec3 color = (ambient + diffuse + specular) * objectColor;
            FragColor = vec4(color, 1.0);
        }";

        // -------- Sun billboard shaders (embedded; camRight/camUp uniforms) --------
        private const string SunBillboardVert = @"#version 330 core
        layout(location = 0) in vec2 aOffset;     // quad offsets in [-0.5, 0.5]
        out vec2 vUV;

        uniform vec3 lightPos;                    // world-space light position
        uniform mat4 view;
        uniform mat4 projection;
        uniform float size;                       // world size of the billboard
        uniform vec3 camRight;                    // world-space camera right
        uniform vec3 camUp;                       // world-space camera up

        void main()
        {
            vec3 worldPos = lightPos + (camRight * aOffset.x + camUp * aOffset.y) * size;
            vUV = aOffset + vec2(0.5);            // 0..1
            gl_Position = projection * view * vec4(worldPos, 1.0);
        }";

        private const string SunBillboardFrag = @"#version 330 core
        in vec2 vUV;
        out vec4 FragColor;

        uniform vec3  sunColor;
        uniform float intensity;   // same scale as scene light
        uniform float time;        // seconds

        void main()
        {
            float r = distance(vUV, vec2(0.5));

            // Pulse (breathing)
            float pulse = 0.95 + 0.10 * sin(time * 2.2);
            float rp = r / pulse;

            // Soft profile
            float core = smoothstep(0.32, 0.00, rp);
            float glow = smoothstep(0.75, 0.32, rp);

            // Tiny flicker
            float flicker = 1.0 + 0.03 * sin(time * 38.0 + rp * 24.0);

            float a = clamp(core + 0.5 * (1.0 - glow), 0.0, 1.0);
            vec3 col = sunColor * (0.85 * core + 0.55 * (1.0 - glow));

            col *= (0.6 + 0.4 * intensity) * flicker;
            a   *= (0.85 + 0.15 * intensity);

            FragColor = vec4(col, a);
        }";
    }
}
