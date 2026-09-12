shader_type canvas_item;

uniform sampler2D mask_tex;
uniform float mask_radius=138.5f;
uniform float darkness=0.95f;
uniform vec2 sprite_size=vec2(512.0f,288.0f);
uniform float dizzy_strength=8.0f;
uniform float dizzy_speed=2.0f;

uniform vec2 mask_pos;

void fragment()
{
    vec2 frag_pos=UV*sprite_size;
    vec2 rel=frag_pos-mask_pos;
    float dist=length(rel);

    vec2 n=rel/max(dist,0.001f);
    float x=n.x;
    float y=n.y;

    float x2=x*x;
    float y2=y*y;

    float cos2=x2-y2;
    float sin2=2.0f*x*y;

    float cos4=cos2*cos2-sin2*sin2;
    float sin4=2.0f*cos2*sin2;

    float cos8=cos4*cos4-sin4*sin4;
    float sin8=2.0f*cos4*sin4;

    float phase=TIME*dizzy_speed;
    float wave=(sin8*cos(phase)+cos8*sin(phase))*dizzy_strength;

    float distorted_dist=dist+wave;
    vec2 distorted_rel=n*distorted_dist;
    vec2 tex_uv=distorted_rel/mask_radius*0.5f+0.5f;

    float mask=0.0f;

    if(tex_uv.x>=0.0f&&tex_uv.x<=1.0f&&tex_uv.y>=0.0f&&tex_uv.y<=1.0f)
    {
        mask=texture(mask_tex,tex_uv).a;
    }

    float alpha=darkness*(1.0f-mask);
    COLOR=vec4(0.0f,0.0f,0.0f,alpha);
}