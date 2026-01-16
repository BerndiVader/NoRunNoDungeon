shader_type canvas_item;

uniform sampler2D gradient_texture;
uniform float gradient_width:hint_range(0.0f,512.0f,1.0f)=16.0f;

uniform float shine_progress:hint_range(0.0f,1.0f,0.01f)=0.0f;
uniform float shine_size:hint_range(0.01f,1.0f,0.01f)=0.35f;
uniform float shine_angle:hint_range(0.0f,89.9f,0.1f)=37.0f;
uniform float shine_intensity:hint_range(1.0f,10.0f)=2.5f;

uniform float threshold:hint_range(0.0f,1.0f)=0.01f;

float scale(float value,float in_min,float in_max,float out_min,float out_max) {
    return(value-in_min)*(out_max-out_min)/(in_max-in_min)+out_min;
}

void fragment() {
    vec2 pixelated_uv=UV;
    COLOR=texture(TEXTURE,UV);

    float slope=tan(radians(shine_angle));
    float progress=scale(shine_progress,0.0f,1.0f,-1.0f-shine_size-shine_size*slope,1.0f*slope+shine_size+shine_size*slope);
    
    float shine_distance=(slope*pixelated_uv.x-pixelated_uv.y-progress)/(sqrt(slope*slope+1.0f));
    float shine_color_coord=clamp((shine_distance/(2.0f*shine_size))+0.5f,0.0f,1.0f);

    float grad_u=(shine_color_coord*(gradient_width-1.0f)+0.5f)/gradient_width;
    vec2 grad_uv=vec2(grad_u,0.5f);
    vec4 raw_color=texture(gradient_texture,grad_uv);

    float shine=step(shine_distance,shine_size)-step(shine_distance,-shine_size);
    float max_component=max(max(COLOR.r,COLOR.g),COLOR.b);

    vec3 glint=raw_color.rgb*shine*raw_color.a*float(max_component>=threshold)*shine_intensity;

    COLOR.rgb=mix(COLOR.rgb,glint,shine*raw_color.a*float(max_component>=threshold));
}