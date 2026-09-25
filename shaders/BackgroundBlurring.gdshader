shader_type canvas_item;

uniform sampler2D screen_texture;
uniform float amount:hint_range(0.0f,5.0f);

void fragment() {
    vec2 uv=SCREEN_UV;
    float offset=amount/500.0f;
    vec3 col=texture(screen_texture,uv).rgb*0.4f;
    col+=texture(screen_texture,uv+vec2(offset,0.0f)).rgb*0.15f;
    col+=texture(screen_texture,uv-vec2(offset,0.0f)).rgb*0.15f;
    col+=texture(screen_texture,uv+vec2(0.0f,offset)).rgb*0.15f;
    col+=texture(screen_texture,uv-vec2(0.0f,offset)).rgb*0.15f;
    COLOR.rgb=col;
    COLOR.a=1.0f;
}	