shader_type canvas_item;

uniform sampler2D noise;
uniform vec4 color_a:hint_color=vec4(1.0f,1.0f,1.0f,1.0f);
uniform vec4 color_b:hint_color=vec4(0.0f,1.0f,1.0f,1.0f);
uniform float trail_threshold=0.6f;
uniform float speed=10.0f;
uniform float trail_length=1000.0f;

void fragment() {
	vec4 noise_trail=texture(noise,vec2(UV.x/trail_length+TIME/speed,UV.y));
	if(noise_trail.r<trail_threshold){
		COLOR=vec4(0.0f);
	} else {
		COLOR=mix(color_a,color_b,1.0f-noise_trail.r);
	}
}