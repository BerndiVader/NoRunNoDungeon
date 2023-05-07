shader_type canvas_item;

uniform float offset:hint_range(-1.0f,1.0f);
uniform float fade:hint_range(0.0f,1.0f);
uniform float strip=4.0f;
uniform vec2 direction=vec2(1.0f,0.0f);

void fragment() {
	vec2 pixels=SCREEN_UV/SCREEN_PIXEL_SIZE;
	vec2 pdir=vec2(direction.y,-direction.x);
	vec2 corrected_uv=UV+(float(int(mod(dot(pdir,pixels)/strip,2)))*2.0f-1.0f)*offset*pdir;
	vec4 color=texture(TEXTURE,corrected_uv);
	color.a*=clamp(1.0f-offset/fade,0.0f,1.0f);
	COLOR=color;
}