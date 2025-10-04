using System;
using OpenTK.Graphics.OpenGL4;


namespace WindowEngine
{
    public sealed class Shader : IDisposable
    {
        public int Handle { get; private set; }
        public Shader(string vsSource, string fsSource)
        {
            int vs = Compile(ShaderType.VertexShader, vsSource);
            int fs = Compile(ShaderType.FragmentShader, fsSource);
            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vs);
            GL.AttachShader(Handle, fs);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var ok);
            if (ok == 0) throw new Exception(GL.GetProgramInfoLog(Handle));
            GL.DeleteShader(vs); GL.DeleteShader(fs);
        }
        static int Compile(ShaderType type, string src)
        {
            int s = GL.CreateShader(type);
            GL.ShaderSource(s, src);
            GL.CompileShader(s);
            GL.GetShader(s, ShaderParameter.CompileStatus, out var ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(s));
            return s;
        }
        public void Use() => GL.UseProgram(Handle);
        public int Loc(string name) => GL.GetUniformLocation(Handle, name);
        public void Dispose() { if (Handle != 0) GL.DeleteProgram(Handle); Handle = 0; }
    }
}