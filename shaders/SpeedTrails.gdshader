shader_type canvas_item;

uniform sampler2D noise;
uniform vec4 color_a:hint_color=vec4(1.0f,1.0f,1.0f,1.0f);
uniform vec4 color_b:hint_color=vec4(0.0f,1.0f,1.0f,1.0f);
uniform float trail_threshold=0.6f;
uniform float speed=10.0f;
uniform float trail_length=1000.0f;
uniform float angle:hint_range(0.0f,360.0f)=0.0f;

void fragment() {
	float rad=radians(angle);
	float cos_rad=cos(rad);
	float sin_rad=sin(rad);
	vec2 uv=vec2(UV.x*cos_rad-UV.y*sin_rad,UV.x*sin_rad+UV.y*cos_rad);
	vec4 noise_trail=texture(noise,vec2(uv.x/trail_length+TIME/speed,uv.y));
	vec4 color;
	if(noise_trail.r<trail_threshold){
		color=vec4(0.0f);
	} else {
		color=mix(color_a,color_b,1.0f-noise_trail.r);
	}
	COLOR=color;
}