uniform sampler2D texture; // Textura da sprite
uniform vec3 fillColor;    // Cor para preencher (RGB, 0-255)

void main()
{
    // Obtém a cor original do pixel da textura
    vec4 pixelColor = texture(texture, gl_TexCoord[0].xy);

    // Converte a cor de preenchimento de 0-255 para 0.0-1.0
    vec3 normalizedColor = fillColor / 255.0;

    // Aplica a cor de preenchimento apenas nos pixels visíveis (alfa > 0)
    if (pixelColor.a > 0.0) {
        gl_FragColor = vec4(normalizedColor, pixelColor.a); // Mantém o canal alfa original
    } else {
        gl_FragColor = pixelColor; // Preserva pixels totalmente transparentes
    }
}