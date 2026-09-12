shader_type canvas_item;

uniform sampler2D mask_tex;
uniform float mask_radius=138.5f;
uniform float darkness=0.95f;
uniform vec2 sprite_size=vec2(512.0f,288.0f);
uniform vec2 mask_pos;

uniform float time;
uniform float dizzy_strength=4.0f;
uniform float dizzy_speed=0.5f;

void fragment()
{
    vec2 frag_pos=UV*sprite_size;
    vec2 rel=frag_pos-mask_pos;

    float phase=fract(TIME*dizzy_speed);

    float wave_x=1.0f-abs(fract(rel.y*0.01f+phase)-0.5f)*2.0f;
    float wave_y=1.0f-abs(fract(rel.x*0.01f+phase)-0.5f)*2.0f;

    vec2 distorted_rel=rel;

    distorted_rel.x+=wave_x*dizzy_strength;
    distorted_rel.y+=wave_y*dizzy_strength;

    vec2 tex_uv=distorted_rel/mask_radius*0.5f+0.5f;

    float mask=0.0f;

    if(tex_uv.x>=0.0f&&tex_uv.x<=1.0f&&tex_uv.y>=0.0f&&tex_uv.y<=1.0f)
    {
        mask=texture(mask_tex,tex_uv).a;
    }

    float alpha=darkness*(1.0f-mask);
    COLOR=vec4(0.0f,0.0f,0.0f,alpha);
}
