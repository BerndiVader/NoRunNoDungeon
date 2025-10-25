shader_type canvas_item;

uniform float sway_speed:hint_range(0.1f,5.0f)=1.0f; 
uniform float sway_strength:hint_range(0.0f,0.05f)=0.01f;
uniform float noise_scale:hint_range(1.0f,20.0f)=5.0f;
uniform float time_offset:hint_range(0.0f,100.0f)=0.0f;

float noise(vec2 p,float t) {
    return (sin(p.x*2.0f+t*sway_speed)*0.5f)+(sin(p.y*5.0f+t*sway_speed*1.5f)*0.3f);
}

void fragment() {
    vec2 uv=UV;
    uv.x+=noise(UV*noise_scale,TIME+time_offset)*sway_strength;
    COLOR=texture(TEXTURE,uv);
}