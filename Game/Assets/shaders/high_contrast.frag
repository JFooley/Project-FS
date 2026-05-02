uniform int direction;
uniform sampler2D texture; // Textura da sprite
uniform vec3 fillColor;    // Cor para preencher

void main() {
    vec3 normalizedColor = fillColor / 255.0; 
    vec4 pixelColor = texture(texture, gl_TexCoord[0].xy);
    vec2 texCoord = gl_TexCoord[0].xy * vec2(textureSize(texture, 0));
    
    if (pixelColor.a == 0.0) { // Transparente
        gl_FragColor = vec4(0, 0, 0, 0);

    } else if (direction == 1) { // vertical
        if ((int(texCoord.x)/2) % 2 == 0) {
            gl_FragColor = vec4(0, 0, 0, pixelColor.a);
        } else {
            gl_FragColor = vec4(normalizedColor, pixelColor.a);
        }

    } else if (direction == 2) { // horizontal
        if ((int(texCoord.y)/2) % 2 == 0) {
            gl_FragColor = vec4(0, 0, 0, pixelColor.a);
        } else {
            gl_FragColor = vec4(normalizedColor, pixelColor.a);
        }

    } else { // Não direcional
        gl_FragColor = vec4(0.7, 0.7, 0.7, pixelColor.a);
    }
}