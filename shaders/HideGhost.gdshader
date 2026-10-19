shader_type canvas_item;

uniform float hide_amount=0.0f;
uniform vec4 outline_color:hint_color=vec4(0.4f,0.4f,0.4f,1.0f);
uniform float outline_width=0.5f;
uniform float pulse_speed=1.0f;

void fragment()
{
    vec4 tex=texture(TEXTURE,UV);
    float pulse=0.5f+0.5f*sin(TIME*pulse_speed);

    vec2 ps=TEXTURE_PIXEL_SIZE*outline_width;
    float neighbour=0.0f;
    for(int x=-1;x<=1;x++)
    {
        for(int y=-1;y<=1;y++)
        {
            neighbour=max(neighbour,texture(TEXTURE,UV+vec2(float(x),float(y))*ps).a);
        }
    }

    float edge=(1.0f-tex.a)*neighbour*hide_amount*(0.5f+0.5f*pulse);

    vec3 body=mix(tex.rgb,outline_color.rgb,hide_amount*0.6f);
    float body_alpha=tex.a*mix(1.0f,0.3f,hide_amount);

    vec3 rgb=mix(body,outline_color.rgb,edge);
    float alpha=max(body_alpha,edge*outline_color.a);

    COLOR=vec4(rgb,alpha);
}
