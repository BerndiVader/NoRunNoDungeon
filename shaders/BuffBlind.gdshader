shader_type canvas_item;

uniform sampler2D mask_tex;
uniform float mask_radius=138.5f;
uniform vec2 sprite_size=vec2(512.0f,288.0f);
uniform float dizzy_strength=8.0f;
uniform float dizzy_speed=2.0f;

uniform vec2 mask_pos;
uniform float time;

void fragment() {
    vec2 frag_pos=UV*sprite_size;

    vec2 rel=frag_pos-mask_pos;
    float dist=length(rel);

    float angle=atan(rel.y,rel.x);
    float wave=sin(angle*8.0f+time*dizzy_speed)*dizzy_strength;
    float distorted_dist=dist+wave;

    vec2 distorted_rel=rel*(distorted_dist/max(dist,0.001f));
    vec2 tex_uv=(mask_pos+distorted_rel-mask_pos)/mask_radius*0.5f+0.5f;
    
    float mask=0.0f;
    if(tex_uv.x>=0.0f&&tex_uv.x<=1.0f&&tex_uv.y>=0.0f&&tex_uv.y<=1.0f) {
        mask=texture(mask_tex,tex_uv).a;
    }
    
    float alpha=0.95f*(1.0f-mask);
    COLOR=vec4(0.0f,0.0f,0.0f,alpha);
}