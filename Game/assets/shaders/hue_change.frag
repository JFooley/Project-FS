uniform sampler2D texture; // Textura da sprite
uniform vec3 hslInput;     // Entrada HSL (hue, saturação, luminosidade)

// Função para converter HSL para RGB
vec3 hslToRgb(vec3 hsl)
{
    float h = hsl.x;
    float s = hsl.y;
    float l = hsl.z;

    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    float x = c * (1.0 - abs(mod(h * 6.0, 2.0) - 1.0));
    float m = l - c / 2.0;

    vec3 rgb;

    if (h < 1.0 / 6.0)
    {
        rgb = vec3(c, x, 0.0);
    }
    else if (h < 2.0 / 6.0)
    {
        rgb = vec3(x, c, 0.0);
    }
    else if (h < 3.0 / 6.0)
    {
        rgb = vec3(0.0, c, x);
    }
    else if (h < 4.0 / 6.0)
    {
        rgb = vec3(0.0, x, c);
    }
    else if (h < 5.0 / 6.0)
    {
        rgb = vec3(x, 0.0, c);
    }
    else
    {
        rgb = vec3(c, 0.0, x);
    }

    return rgb + m;
}

void main()
{
    // Obtém a cor original do pixel da textura
    vec4 pixelColor = texture(texture, gl_TexCoord[0].xy);

    // Calcula a luminância do pixel original
    float luminance = dot(pixelColor.rgb, vec3(0.2126, 0.7152, 0.0722));

    // Ajusta a luminosidade da cor HSL fornecida com base na luminância do pixel
    vec3 hslAdjusted = hslInput;
    float N = 0;
    if (hslAdjusted.z >= 0.5) N = 1 - hslAdjusted.z;
    else N = hslAdjusted.z;
    hslAdjusted.z = luminance * ((hslAdjusted.z + N) - (hslAdjusted.z - N)) + (hslAdjusted.z - N);

    // Converte HSL para RGB
    vec3 finalColor = hslToRgb(hslAdjusted);

    // Mantém o canal alfa original
    gl_FragColor = vec4(finalColor, pixelColor.a);
}