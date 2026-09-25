shader_type canvas_item;
render_mode blend_mix;

uniform float radius=5.0f;
uniform float amount=0.25f;

void fragment() {
	float r=radius;
	vec2 ps=TEXTURE_PIXEL_SIZE;
	vec4 col=texture(TEXTURE,UV);
	vec4 glow=col;

	glow+=texture(TEXTURE,UV+vec2(-r,-r)*ps);
	glow+=texture(TEXTURE,UV+vec2(-r,0.0f)*ps);
	glow+=texture(TEXTURE,UV+vec2(-r,r)*ps);
	glow+=texture(TEXTURE,UV+vec2(0.0f,-r)*ps);
	glow+=texture(TEXTURE,UV+vec2(0.0f,r)*ps);
	glow+=texture(TEXTURE,UV+vec2(r,-r)*ps);
	glow+=texture(TEXTURE,UV+vec2(r,0.0f)*ps);
	glow+=texture(TEXTURE,UV+vec2(r,r)*ps);

	r*=2.0f;
	glow+=texture(TEXTURE,UV+vec2(-r,-r)*ps);
	glow+=texture(TEXTURE,UV+vec2(-r,0.0f)*ps);
	glow+=texture(TEXTURE,UV+vec2(-r,r)*ps);
	glow+=texture(TEXTURE,UV+vec2(0.0f,-r)*ps);
	glow+=texture(TEXTURE,UV+vec2(0.0f,r)*ps);
	glow+=texture(TEXTURE,UV+vec2(r,-r)*ps);
	glow+=texture(TEXTURE,UV+vec2(r,0.0f)*ps);
	glow+=texture(TEXTURE,UV+vec2(r,r)*ps);

	glow/=17.0f;
	glow*=amount;
	col.rgb*=col.a;

	COLOR=glow+col;
}