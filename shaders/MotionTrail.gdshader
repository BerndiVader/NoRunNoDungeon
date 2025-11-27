shader_type canvas_item;

uniform vec2 frame_coords=vec2(0.0f,0.0f);
uniform vec2 nb_frames=vec2(1.0f,1.0f);
uniform vec2 velocity=vec2(0.0f,0.0f);
uniform float velocity_max=100.0f;
uniform float trail_size=20.0f;
uniform float alpha_start=0.8f;
uniform float alpha_tail=0.2f;
uniform float alpha_factor=1.2f;
uniform float noise_margin=0.68f;
uniform sampler2D noise;
uniform bool flip=false;

vec2 mid_uv(vec2 coords,vec2 px_size) {
    float px_mid_x=1.0f-px_size.x*0.5f;
    float px_mid_y=1.0f-px_size.y*0.5f;
    if(coords.x<1.0f) {
		px_mid_x=px_size.x*(floor(coords.x/px_size.x)+0.5f);
    }
    if(coords.y<1.0f) {
		px_mid_y=px_size.y*(floor(coords.y/px_size.y)+0.5f);
    }
    return vec2(px_mid_x,px_mid_y);
}

void fragment() {
    vec2 uv=UV;
    COLOR=texture(TEXTURE,uv);

    vec2 v_dir=normalize(velocity);
    if(flip) {
		v_dir.x=-v_dir.x;
    }

    float alpha=alpha_tail;
    float max_length=length(velocity)*trail_size/velocity_max;
    vec2 px_size=TEXTURE_PIXEL_SIZE;
    vec2 frame_uv=vec2(uv.x*nb_frames.x-frame_coords.x,uv.y*nb_frames.y-frame_coords.y);
    vec2 base_mid_uv=mid_uv(frame_uv,px_size);

	for(float v_length=max_length;v_length>=0.0f;v_length-=0.5f) {
        vec2 v=v_length*v_dir*TEXTURE_PIXEL_SIZE;
        vec2 px_mid_uv=base_mid_uv+v;
        float noise_value=texture(noise,px_mid_uv).r;
        vec4 color=texture(TEXTURE,px_mid_uv);
        if(noise_value>noise_margin&&color.a>0.0f) {
            if(COLOR.a==0.0f) {
				color.a=color.a*alpha;
                COLOR=color;
            }
            break;
        }
        alpha=min(alpha*alpha_factor,alpha_start);
	}

}