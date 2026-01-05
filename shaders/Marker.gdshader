shader_type canvas_item;

uniform vec4 glow_color:hint_color=vec4(1.0f,1.0f,0.5f,0.7f);

void fragment() {
    float dist=length(UV-vec2(0.5f));
    float glow=smoothstep(0.4f,0.5f,dist);
    COLOR=glow_color*(1.0f-glow);
}