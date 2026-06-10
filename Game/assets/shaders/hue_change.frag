uniform sampler2D tex;
uniform vec3 hslInput;

// Função para converter HSL para RGB
vec3 hslToRgb(vec3 hsl) {
    float h = hsl.x;
    float s = hsl.y;
    float l = hsl.z;

    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    float x = c * (1.0 - abs(mod(h * 6.0, 2.0) - 1.0));
    float m = l - c / 2.0;

    vec3 rgb;

    if (h < 1.0 / 6.0) {
        rgb = vec3(c, x, 0.0);

    } else if (h < 2.0 / 6.0) {
        rgb = vec3(x, c, 0.0);

    } else if (h < 3.0 / 6.0) {
        rgb = vec3(0.0, c, x);

    } else if (h < 4.0 / 6.0) {
        rgb = vec3(0.0, x, c);

    } else if (h < 5.0 / 6.0) {
        rgb = vec3(x, 0.0, c);

    } else {
        rgb = vec3(c, 0.0, x);
    }

    return rgb + m;
}

void main() {
    vec4 pixelColor = texture2D(tex, gl_TexCoord[0].xy);

    float luminance = dot(pixelColor.rgb, vec3(0.2126, 0.7152, 0.0722));

    vec3 hslAdjusted = hslInput;

    float N = 0.0;

    if (hslAdjusted.z >= 0.5) {
        N = 1.0 - hslAdjusted.z;
    } else {
        N = hslAdjusted.z;
    }

    hslAdjusted.z = luminance * ((hslAdjusted.z + N) - (hslAdjusted.z - N)) + (hslAdjusted.z - N);

    vec3 finalColor = hslToRgb(hslAdjusted);

    gl_FragColor = vec4(finalColor, pixelColor.a);
}