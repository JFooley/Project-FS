uniform sampler2D texture;     // Sprite original
uniform sampler2D palette;     // Paleta de cores (linhas = esquemas, colunas = cores)
uniform float palette_size;    // Número de cores por linha
uniform float palette_quantity;// Número de linhas na paleta
uniform float palette_index;   // Linha da paleta desejada
uniform vec3 light;            // Iluminação

void main() {
    vec4 spriteData = texture2D(texture, gl_TexCoord[0].xy);

    if (spriteData.a == 0.0) discard;

    // Procurar a cor mais próxima na linha 0 da paleta
    float bestIndex = 0.0;
    float minDist = 1000.0;

    for (float i = 0.0; i < palette_size; i += 1.0) {
        float u0 = (i + 0.5) / palette_size;
        float v0 = 0.5 / palette_quantity; // Linha 0
        vec3 palColor = texture2D(palette, vec2(u0, v0)).rgb;

        // Distância euclidiana RGB
        float dist = distance(spriteData.rgb, palColor);
        if (dist < minDist) {
            minDist = dist;
            bestIndex = i;
        }
    }

    // Agora pega a cor correspondente na linha desejada
    float u = (bestIndex + 0.5) / palette_size;
    float v = (palette_index + 0.5) / palette_quantity;
    vec3 color = texture2D(palette, vec2(u, v)).rgb;

    color *= light; // Multiplicação de iluminação
    gl_FragColor = vec4(color, spriteData.a);
}