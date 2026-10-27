
shader_type canvas_item;

uniform float hide_amount=0.0f;
uniform vec4 outline_color:hint_color=vec4(0.35f,0.75f,1.0f,1.0f);
uniform vec4 ghost_color:hint_color=vec4(0.35f,0.65f,0.9f,1.0f);

uniform float outline_width=1.0f;
uniform float hidden_alpha=0.04f;

void fragment()
{
    vec4 tex=texture(TEXTURE,UV);
    float h=clamp(hide_amount,0.0f,1.0f);

    vec2 ps=TEXTURE_PIXEL_SIZE*outline_width;
    float neighbour=0.0f;

    for(int x=-1;x<=1;x++) {
        for(int y=-1;y<=1;y++) {
            if(x==0&&y==0) continue;
            neighbour=max(neighbour,texture(TEXTURE,UV+vec2(float(x),float(y))*ps).a);
        }
    }

    float edge=neighbour*(1.0f-tex.a);
    float edge_alpha=edge*h*outline_color.a;

    vec3 body_rgb=mix(tex.rgb,ghost_color.rgb,h*0.5f);
    float body_alpha=tex.a*mix(1.0f,hidden_alpha,h);

    vec3 rgb=mix(body_rgb,outline_color.rgb,edge);
    float alpha=max(body_alpha,edge_alpha);

    COLOR=vec4(rgb,alpha);
}
