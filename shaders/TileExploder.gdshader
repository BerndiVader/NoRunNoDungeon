shader_type canvas_item;

uniform float progress:hint_range(0.0f,1.0f)=0.0f;
uniform float explosion_strength:hint_range(0.0f,3.0f)=1.5f;
uniform float block_size:hint_range(0.01f,0.5f)=0.025f;

float rand(vec2 co) {
    return fract(sin(dot(co.xy,vec2(12.9898f,78.233f)))*43758.5453f);
}

void fragment() {
    vec2 block_uv=floor(UV/block_size)*block_size+block_size*0.5f;

    float angle=rand(block_uv)*6.2831853f;
    vec2 random_dir=vec2(cos(angle),sin(angle));

    float dist=length(block_uv-vec2(0.5f));
    vec2 explode_uv=UV+random_dir*pow(progress,1.5f)*explosion_strength*dist;
    float alpha=clamp(1.0f-progress*1.2f-dist*progress,0.0f,1.0f);

    vec4 tex=texture(TEXTURE,explode_uv);
    COLOR=vec4(tex.rgb,tex.a*alpha);
}
