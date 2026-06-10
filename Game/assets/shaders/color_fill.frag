uniform sampler2D tex;
uniform vec3 fillColor;

void main()
{
    vec4 pixelColor = texture2D(tex, gl_TexCoord[0].xy);

    vec3 normalizedColor = fillColor / 255.0;

    if (pixelColor.a > 0.001)
    {
        gl_FragColor = vec4(normalizedColor, pixelColor.a);
    }
    else
    {
        gl_FragColor = pixelColor;
    }
}