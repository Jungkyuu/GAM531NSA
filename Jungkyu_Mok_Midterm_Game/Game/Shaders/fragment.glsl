#version 330 core
out vec4 FragColor;

in vec3 vFragPos;
in vec3 vNormal;
in vec2 vUV;

uniform vec3 uViewPos;
uniform vec3 uLightPos;
uniform int  uLightOn;
uniform sampler2D uTex0;

void main()
{
    vec3 baseColor = texture(uTex0, vUV).rgb;

    // Phong
    vec3 ambient = 0.2 * baseColor;

    vec3 norm = normalize(vNormal);
    vec3 lightDir = normalize(uLightPos - vFragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * baseColor;

    vec3 viewDir = normalize(uViewPos - vFragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    vec3 specular = spec * vec3(1.0);

    vec3 light = ambient + (uLightOn == 1 ? (diffuse + specular) : vec3(0.0));
    FragColor = vec4(light, 1.0);
}