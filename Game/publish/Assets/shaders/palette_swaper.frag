uniform sampler2D texture;
uniform sampler2D palette;
uniform float palette_size;
uniform float palette_quantity;
uniform float palette_index;
uniform vec3 light;

void main() {
    vec4 spriteData = texture2D(texture, gl_TexCoord[0].xy);
    
    if (spriteData.a == 0.0) discard;
    
    float index = spriteData.r * 255.0;
    float u = (index + 0.5) / palette_size;
    float v = (palette_index + 0.5) / palette_quantity;
    
    vec3 color = texture2D(palette, vec2(u, v)).rgb;
    color = color * light;
    gl_FragColor = vec4(color, spriteData.a);
}