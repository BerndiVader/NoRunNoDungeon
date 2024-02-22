shader_type canvas_item;

uniform sampler2D screen_texture;
uniform float amount:hint_range(0.0f,5.0f);

void fragment() {
	COLOR.rgb=textureLod(screen_texture,SCREEN_UV,amount).rgb;
}