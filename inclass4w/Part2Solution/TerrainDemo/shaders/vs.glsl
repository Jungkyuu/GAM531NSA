#version 330 core
// Vertex Shader (VS) - position + color, MVP transform
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aColor;

uniform mat4 uMVP;

out vec3 vColor;

void main()
{
    vColor = aColor;
    gl_Position = uMVP * vec4(aPos, 1.0);
}
