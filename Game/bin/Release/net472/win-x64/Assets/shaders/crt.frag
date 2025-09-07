uniform sampler2D texture;
uniform vec2 textureSize;

void main()
{
    // Coordenadas normalizadas (0 a 1)
    vec2 uv = gl_TexCoord[0].xy;

    // Distorção de barril (opcional)
    vec2 dist = uv - vec2(0.5, 0.5);
    float dist2 = dot(dist, dist);
    uv = uv - dist * dist2 * 0.1;

    // Scanlines (linhas escuras horizontais)
    float scanline = sin(uv.y * textureSize.y * 3.14159) * 0.1;
    vec3 color = texture2D(texture, uv).rgb;

    // Máscara de sombra (aperture grille) - padrão RGB
    vec2 maskPos = fract(uv * textureSize / 3.0);
    vec3 mask = vec3(
        fract(maskPos.x + 0.333) < 0.333 ? 1.0 : 0.0,
        fract(maskPos.x + 0.666) < 0.333 ? 1.0 : 0.0,
        fract(maskPos.x) < 0.333 ? 1.0 : 0.0
    );

    // Aplica a máscara e scanline
    color = color * mask;
    color -= scanline;

    gl_FragColor = vec4(color, 1.0);
}