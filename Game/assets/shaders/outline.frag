uniform sampler2D tex;
uniform vec3 fillColor;

bool isEdge(vec2 texCoord, sampler2D tex) {
    if (texture(tex, texCoord).a == 0.0) {
        for (int i = -1; i < 2; i++) {
            for (int j = -1; j < 2; j++) {
                if (texture(tex, texCoord + vec2(i, j) / vec2(textureSize(tex, 0))).a > 0.0) {
                    return true;
                }
            }
        }
    }
    return false;
}  

void main() {
    vec3 normalizedColor = fillColor / 255.0;

    if (isEdge(gl_TexCoord[0].xy, tex)) {
        gl_FragColor = vec4(normalizedColor, 1.0);
    }
} 