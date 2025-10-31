using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Game.GLNS;

public class Mesh : IDisposable
{
    private readonly int vao, vbo, ebo;
    private readonly int indexCount;
    public Matrix4 Model = Matrix4.Identity;

    public Mesh(float[] vertices, uint[] indices)
    {
        indexCount = indices.Length;
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        ebo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

        int stride = 8 * sizeof(float);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));
        GL.EnableVertexAttribArray(2);
    }

    public void Draw()
    {
        GL.BindVertexArray(vao);
        GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(vao);
        GL.DeleteBuffer(vbo);
        GL.DeleteBuffer(ebo);
    }
}

public static class MeshFactory
{
    public static Mesh CreatePlane(float size)
    {
        float s = size / 2f;
        float[] v =
        {
            -s, 0, -s, 0,1,0, 0,0,
             s, 0, -s, 0,1,0, 1,0,
             s, 0,  s, 0,1,0, 1,1,
            -s, 0,  s, 0,1,0, 0,1,
        };
        uint[] i = { 0,1,2, 0,2,3 };
        return new Mesh(v, i);
    }

    public static Mesh CreateTexturedCube()
    {
        float[] v =
        {
            // pos         normal     uv
            -0.5f,-0.5f,-0.5f, 0,0,-1, 0,0,
             0.5f,-0.5f,-0.5f, 0,0,-1, 1,0,
             0.5f, 0.5f,-0.5f, 0,0,-1, 1,1,
            -0.5f, 0.5f,-0.5f, 0,0,-1, 0,1,
        };
        uint[] i = { 0,1,2, 0,2,3 };
        return new Mesh(v, i);
    }
}