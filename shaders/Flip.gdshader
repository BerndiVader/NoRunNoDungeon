shader_type canvas_item;

uniform bool flip_h=false;
uniform bool flip_v=false;

void fragment() {
    vec2 uv=UV;
    if(flip_h) {
        uv.x=1.0f-uv.x;
    }
    if(flip_v) {
        uv.x=1.0f-uv.y;
    }
    COLOR=texture(TEXTURE,uv)*COLOR;
}
