#version 330 core
out vec4 FragColor;

in vec3 FragPos;
in vec3 Normal;

uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 lightColor;     // e.g., vec3(1.0)
uniform vec3 objectColor;    // base albedo

uniform float ambientStrength;
uniform float specularStrength;
uniform float shininess;     // 16 ~ 128
uniform float lightIntensity;

// New: attenuation & rim params
uniform float attConst;      // typically 1.0
uniform float attLinear;     // 0.09 ~ 0.14
uniform float attQuad;       // 0.032 ~ 0.07
uniform float rimStrength;   // 0.0 ~ 1.0 (0.25 good)
uniform float rimPower;      // 1.0 ~ 4.0  (2.0 good)
uniform bool  enableGamma;   // true -> apply gamma correction (2.2)

void main()
{
    // Vectors
    vec3 N = normalize(Normal);
    vec3 L = normalize(lightPos - FragPos);
    vec3 V = normalize(viewPos  - FragPos);

    // Distance attenuation (quadratic model)
    float dist = max(length(lightPos - FragPos), 0.0001);
    float atten = 1.0 / (attConst + attLinear * dist + attQuad * dist * dist);

    // Lighting energy (color * intensity * attenuation)
    vec3 light = lightColor * lightIntensity * atten;

    // Ambient
    vec3 ambient = ambientStrength * light;

    // Diffuse (Lambert)
    float NdotL = max(dot(N, L), 0.0);
    vec3 diffuse = NdotL * light;

    // Specular (Blinn-Phong)
    vec3 H = normalize(L + V);
    float NdotH = max(dot(N, H), 0.0);
    float spec = pow(NdotH, shininess);
    vec3 specular = specularStrength * spec * light;

    // Rim light (view-dependent edge boost)
    float rim = pow(1.0 - max(dot(N, V), 0.0), rimPower);
    vec3 rimCol = rimStrength * rim * lightColor * atten;

    // Combine
    vec3 color = (ambient + diffuse) * objectColor + specular + rimCol;

    // Optional gamma correction to sRGB
    if (enableGamma) {
        color = pow(color, vec3(1.0/2.2));
    }

    FragColor = vec4(color, 1.0);
}
