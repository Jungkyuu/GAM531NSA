#version 330 core
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

    // Correct normal transform = inverse-transpose of model's upper-left 3x3
    mat3 normalMat = mat3(transpose(inverse(model)));
    Normal = normalize(normalMat * aNormal);

    gl_Position = projection * view * worldPos;
}