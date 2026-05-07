uniform sampler2D texture; // Textura da sprite
uniform vec3 color; // Cor para tintar (RGB, 0-255)

void main() {
    // Converte a cor de tintura de 0-255 para 0.0-1.0
    vec3 normalizedTintColor = color / 255.0;

    // Obtém a cor original do pixel da textura
    vec4 pixelColor = texture(texture, gl_TexCoord[0].xy);

    // Mistura a cor original com a cor de tintura
    vec3 tintedColor = pixelColor.rgb * normalizedTintColor;

    // Define a cor final do pixel, mantendo o canal alfa (transparência)
    gl_FragColor = vec4(tintedColor, pixelColor.a);
}