shader_type canvas_item;

uniform bool overlay=false;
uniform float scanlines_opacity:hint_range(0.0f,1.0f)=0.4f;
uniform float scanlines_width:hint_range(0.0f,0.5f)=0.25f;
uniform float grille_opacity:hint_range(0.0f,1.0f)=0.3f;
uniform vec2 resolution=vec2(640.0f,480.0f);
uniform bool pixelate=true;
uniform bool roll=true;
uniform float roll_speed=8.0f;
uniform float roll_size:hint_range(0.0f,100.0f)=15.0f;
uniform float roll_variation:hint_range(0.1f,5.0f)=1.8f;
uniform float distort_intensity:hint_range(0.0f,0.2f)=0.05f;

uniform float noise_opacity:hint_range(0.0f,1.0f)=0.4f;
uniform float noise_speed=5.0f;

uniform float static_noise_intensity:hint_range(0.0f,1.0f)=0.06f;

uniform float aberration:hint_range(-1.0f,1.0f)=0.03f;
uniform float brightness=1.4f;
uniform bool discolor=true;

uniform float warp_amount:hint_range(0.0f, 5.0f)=1.0f;
uniform bool clip_warp=false;

uniform float vignette_intensity=0.4f;
uniform float vignette_opacity:hint_range(0.0f,1.0f)=0.5f;

vec2 random(vec2 uv) {
    uv=vec2(dot(uv,vec2(127.1f,311.7f)),dot(uv,vec2(269.5f,183.3f)));
    return-1.0f+2.0f*fract(sin(uv)*43758.5453123f);
}

float noise(vec2 uv) {
    vec2 uv_index=floor(uv);
    vec2 uv_fract=fract(uv);
    vec2 blur=smoothstep(0.0f,1.0f,uv_fract);

    return mix(
				mix(
					dot(random(uv_index+vec2(0.0f,0.0f)),uv_fract-vec2(0.0f,0.0f)),
					dot(random(uv_index+vec2(1.0f,0.0f)),uv_fract-vec2(1.0f,0.0f)),
					blur.x
				),
				mix(
					dot(random(uv_index+vec2(0.0f,1.0f)),uv_fract-vec2(0.0f,1.0f)),
                    dot(random(uv_index+vec2(1.0f,1.0f)),uv_fract-vec2(1.0f,1.0f)),
					blur.x
				),
				blur.y
			)*0.5f+0.5f;
}

vec2 warp(vec2 uv) {
	vec2 delta=uv-0.5f;
	float delta2=dot(delta.xy,delta.xy);	
	return uv+delta*delta2*delta2*warp_amount;
}

float border (vec2 uv) {
	float radius=min(warp_amount,0.08f);
	radius=max(min(min(radius*2.0f,1.0f),1.0f),1e-5);
	return clamp(1.0f-smoothstep(0.96f,1.0f,length(max(vec2(0.0f),(uv*2.0f-1.0f)-vec2(1.0f,1.0f)+radius))/radius),0.0f,1.0f);
}

float vignette(vec2 uv) {
	uv*=1.0f-uv.xy;
	return pow(uv.x*uv.y*15.0f,vignette_intensity*vignette_opacity);
}

void fragment() {
	vec2 uv=overlay?warp(SCREEN_UV):warp(UV);
	vec2 tex_uv=pixelate?ceil(uv*resolution)/resolution:uv;

	vec2 roll_uv=vec2(0.0f);
	float time=roll?TIME:0.0f;
	float roll_line=0.0f;
	
	if(roll||noise_opacity>0.0f) {
		roll_line=smoothstep(0.3f,0.9f,sin(uv.y*roll_size-(time*roll_speed)));
		roll_line*=roll_line*smoothstep(0.3f,0.9f,sin(uv.y*roll_size*roll_variation-(time*roll_speed*roll_variation)));
		roll_uv=vec2((roll_line*distort_intensity*(1.0f-UV.x)),0.0f);
	}
	
	vec4 tex;
	if(roll) {
		tex.r=texture(SCREEN_TEXTURE,tex_uv+roll_uv*0.8f+vec2(aberration,0.0f)*0.1f).r;
		tex.g=texture(SCREEN_TEXTURE,tex_uv+roll_uv*1.2f-vec2(aberration,0.0f)*0.1f).g;
		tex.b=texture(SCREEN_TEXTURE,tex_uv+roll_uv).b;
		tex.a=1.0f;
	} else {
		tex.r=texture(SCREEN_TEXTURE,tex_uv+vec2(aberration,0.0f)*0.1f).r;
		tex.g=texture(SCREEN_TEXTURE,tex_uv-vec2(aberration,0.0f)*0.1f).g;
		tex.b=texture(SCREEN_TEXTURE,tex_uv).b;
		tex.a=1.0f;
	}
	
	float r=tex.r;
	float g=tex.g;
	float b=tex.b;
	
	uv=warp(UV);
	
	if(grille_opacity>0.0f) {
		r=mix(r,r*smoothstep(0.85f,0.95f,abs(sin(uv.x*(resolution.x*3.14159265f)))),grille_opacity);
		g=mix(g,g*smoothstep(0.85f,0.95f,abs(sin(1.05f+uv.x*(resolution.x*3.14159265f)))),grille_opacity);
		b=mix(b,b*smoothstep(0.85f,0.95f,abs(sin(2.1f+uv.x*(resolution.x*3.14159265f)))),grille_opacity);
	}
	
	tex.r=clamp(r*brightness,0.0f,1.0f);
	tex.g=clamp(g*brightness,0.0f,1.0f);
	tex.b=clamp(b*brightness,0.0f,1.0f);
	
	float scanlines=0.5f;
	if(scanlines_opacity>0.0f) {
		scanlines=smoothstep(scanlines_width,scanlines_width+0.5f,abs(sin(uv.y*(resolution.y*3.14159265f))));
		tex.rgb=mix(tex.rgb,tex.rgb*vec3(scanlines),scanlines_opacity);
	}
	
	if(noise_opacity>0.0f) {
		float noise=smoothstep(0.4f,0.5f,noise(uv*vec2(2.0f,200.0f)+vec2(10.0f,(TIME*(noise_speed)))));
		roll_line*=noise*scanlines*clamp(random((ceil(uv*resolution)/resolution)+vec2(TIME*0.8f,0.0f)).x+0.8f,0.0f,1.0f);
		tex.rgb=clamp(mix(tex.rgb,tex.rgb+roll_line,noise_opacity),vec3(0.0f),vec3(1.0f));
	}
	
	if (static_noise_intensity>0.0f) {
		tex.rgb+=clamp(random((ceil(uv*resolution)/resolution)+fract(TIME)).x,0.0f,1.0f)*static_noise_intensity;
	}
	
	tex.rgb*=border(uv);
	tex.rgb*=vignette(uv);

	if(clip_warp) {
		tex.a=border(uv);
	}
	
	if(discolor) {
		tex.rgb=mix(tex.rgb,vec3(tex.r+tex.g+tex.b)/3.0f,0.5f);
		
		float midpoint=pow(0.5f,2.2f);
		tex.rgb=(tex.rgb-vec3(midpoint))*1.2f+midpoint;
	}
	
	COLOR=tex;
}