shader_type canvas_item;
render_mode blend_add;

uniform float radius=0.5f;
uniform float amount=0.8f;
uniform vec4 light_color:hint_color=vec4(1.0f,0.9f,0.7f,1.0f);

void fragment() {
    vec2 center=vec2(0.5f);
    float dist=length(UV-center);
    float glow=smoothstep(radius,radius-0.2f,dist)*amount;

    vec4 tex=texture(TEXTURE,UV);
    vec3 final_color=tex.rgb*light_color.rgb*glow;

    COLOR=vec4(final_color,tex.a*glow);
}