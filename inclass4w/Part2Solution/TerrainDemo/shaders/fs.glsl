#version 330 core
// Fragment Shader (FS) - pass-through color
in vec3 vColor;
out vec4 FragColor;

void main()
{
    FragColor = vec4(vColor, 1.0);
}
