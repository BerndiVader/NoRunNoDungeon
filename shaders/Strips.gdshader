shader_type canvas_item;

uniform vec2 direction=vec2(0.0f,0.5f);
uniform float offset:hint_range(-1.0f,1.0f);
uniform float fade:hint_range(0.0f,1.0f);
uniform float strip=4.0f;

void fragment() {
	vec2 norm=normalize(direction);
	vec2 pixels=SCREEN_UV/SCREEN_PIXEL_SIZE;
	vec2 pdir=vec2(norm.y,-norm.x);
	vec2 corrected_uv=UV+((float(int(dot(pdir,pixels)/strip)&0x1)*2.0f)-1.0f)*offset*pdir;
	vec4 color=texture(TEXTURE,corrected_uv);
	color.a*=clamp(1.0f-offset/fade,0.0f,1.0f);
	COLOR=color;
}