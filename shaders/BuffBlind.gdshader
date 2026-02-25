shader_type canvas_item;

uniform sampler2D mask_tex;
uniform vec2 mask_pos;

uniform float mask_radius=128.0f;
uniform vec2 sprite_size=vec2(512.0f,288.0f);

void fragment() {
    vec2 tex_uv=(UV*sprite_size-mask_pos)/mask_radius*0.5f+0.5f;

    if(tex_uv.x>=0.0f&&tex_uv.x<=1.0f&&tex_uv.y>=0.0f&&tex_uv.y<=1.0f) {
        float mask=texture(mask_tex,tex_uv).a;
        COLOR=vec4(0.0f,0.0f,0.0f,1.0f*(1.0f-mask));
    } else {
        COLOR=vec4(0.0f,0.0f,0.0f,1.0f);
    }

}