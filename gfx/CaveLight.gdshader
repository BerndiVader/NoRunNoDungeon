shader_type canvas_item;
render_mode blend_add;

uniform float radius = 0.45;
uniform float amount = 0.8;
uniform vec4 light_color : hint_color = vec4(1.0, 0.9, 0.7, 1.0);

void fragment() {
    vec2 center = vec2(0.5);
    float dist = length(UV - center);
    float glow = smoothstep(radius, radius - 0.2, dist) * amount;

    vec4 tex = texture(TEXTURE, UV);
    vec3 final_color = tex.rgb * light_color.rgb * glow;

    COLOR = vec4(final_color, tex.a * glow);
}