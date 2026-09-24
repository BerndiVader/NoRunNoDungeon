shader_type canvas_item;

uniform vec2 look_direction=vec2(1.0f,-2.0f);
uniform float distortion=5.0f;
uniform float direction=-1.0f;

void vertex()
{
    vec2 dir=normalize(look_direction);
    vec2 perpendicular=vec2(-dir.y,dir.x)*direction;

    float influence=dot(normalize(VERTEX),dir);
    influence=max(influence,0.0f);

    VERTEX+=perpendicular*influence*distortion;
}