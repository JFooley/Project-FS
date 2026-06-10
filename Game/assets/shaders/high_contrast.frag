uniform int direction;
uniform sampler2D tex;
uniform vec3 fillColor;
uniform vec2 texSize;

void main() {
    vec3 normalizedColor = fillColor / 255.0;

    vec4 pixelColor = texture2D(tex, gl_TexCoord[0].xy);

    vec2 texCoord = gl_TexCoord[0].xy * texSize;

    if (pixelColor.a < 0.001) {
        gl_FragColor = vec4(0.0, 0.0, 0.0, 0.0);

    } else if (direction == 1) {
        float stripe = mod(floor(texCoord.x / 2.0), 2.0);

        if (stripe == 0.0) {
            gl_FragColor = vec4(0.0, 0.0, 0.0, pixelColor.a);
        } else {
            gl_FragColor = vec4(normalizedColor, pixelColor.a);
        }

    } else if (direction == 2) {
        float stripe = mod(floor(texCoord.y / 2.0), 2.0);

        if (stripe == 0.0) {
            gl_FragColor = vec4(0.0, 0.0, 0.0, pixelColor.a);
        } else {
            gl_FragColor = vec4(normalizedColor, pixelColor.a);
        }

    } else {
        gl_FragColor = vec4(0.7, 0.7, 0.7, pixelColor.a);
    }
}