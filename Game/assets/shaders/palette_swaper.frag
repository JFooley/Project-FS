uniform sampler2D tex;
uniform sampler2D palette;
uniform float palette_size;
uniform float palette_quantity;
uniform float palette_index;
uniform vec3 light;

void main() {
    vec4 spriteData = texture2D(tex, gl_TexCoord[0].xy);

    if (spriteData.a < 0.001) discard;

    float bestIndex = 0.0;
    float minDist = 1000.0;

    for (float i = 0.0; i < 256.0; i += 1.0) {
        if (i >= palette_size) break;

        float u0 = (i + 0.5) / palette_size;
        float v0 = 0.5 / palette_quantity;

        vec3 palColor = texture2D(palette, vec2(u0, v0)).rgb;

        float dist = distance(spriteData.rgb, palColor);

        if (dist < minDist) {
            minDist = dist;
            bestIndex = i;
        }
    }

    float u = (bestIndex + 0.5) / palette_size;
    float v = (palette_index + 0.5) / palette_quantity;

    vec3 color = texture2D(palette, vec2(u, v)).rgb;

    color *= light;

    gl_FragColor = vec4(color, spriteData.a);
}