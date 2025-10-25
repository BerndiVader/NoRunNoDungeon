shader_type canvas_item;

uniform sampler2D screen_texture;
uniform float amount : hint_range(0.0, 5.0);

void fragment() {
    vec2 uv = SCREEN_UV;
    float offset = amount / 500.0;
    vec3 col = texture(screen_texture, uv).rgb * 0.4;
    col += texture(screen_texture, uv + vec2(offset, 0.0)).rgb * 0.15;
    col += texture(screen_texture, uv - vec2(offset, 0.0)).rgb * 0.15;
    col += texture(screen_texture, uv + vec2(0.0, offset)).rgb * 0.15;
    col += texture(screen_texture, uv - vec2(0.0, offset)).rgb * 0.15;
    COLOR.rgb = col;
    COLOR.a = 1.0;
}	