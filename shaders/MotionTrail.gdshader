shader_type canvas_item;

uniform vec2 frame_coords=vec2(0.0f,0.f);
uniform vec2 nb_frames=vec2(1.0f,1.0f);
uniform vec2 velocity=vec2(0.0f,0.0f);
uniform float velocity_max=100.0f;
uniform float trail_size=20.0f;
uniform float alpha_start=0.8f;
uniform float alpha_tail=0.2f;
uniform float alpha_factor=1.2f;
uniform float noise_margin=0.68f;
uniform sampler2D noise;


vec2 get_mid_uv(vec2 coords,vec2 px_size) {
	float px_mid_x=1.0f-px_size.x/2.0f;
	float px_mid_y=1.0f-px_size.y/2.0f;
	if(coords.x<1.0f){
		px_mid_x=px_size.x*(floor(coords.x/px_size.x)+1.0f/2.0f);
	}
	if(coords.y<1.0f) {
		px_mid_y=px_size.y*(floor(coords.y/px_size.y)+1.0f/2.0f); 
	}
	return vec2(px_mid_x,px_mid_y);
}

void fragment() {
	COLOR=texture(TEXTURE,UV);

	float UV_X=(UV.x*nb_frames.x-frame_coords.x);
	float UV_Y=(UV.y*nb_frames.y-frame_coords.y);
	vec2 uv=vec2(UV_X,UV_Y);
	
	vec2 v_dir=normalize(velocity);
	float v_length=length(velocity)*trail_size/velocity_max;
	
	float alpha_step=(alpha_start-alpha_tail)/trail_size;
	float alpha=alpha_tail;
	while(v_length>0.0f) {
		vec2 velo=v_length*v_dir*TEXTURE_PIXEL_SIZE;
		vec2 px_mid_uv=get_mid_uv(UV,TEXTURE_PIXEL_SIZE)+velo;
		float noiseValue=texture(noise,px_mid_uv).r;
		vec4 new_color=texture(TEXTURE,px_mid_uv);
		if(noiseValue>noise_margin&&new_color.a>0.0f) {
			if(COLOR.a==0.0f) {
				COLOR=new_color;
				COLOR.a=new_color.a*alpha;
			}
			break;
		}
		v_length-=0.5f;
		alpha*=alpha_factor;
		alpha=min(alpha,alpha_start);
	}
}