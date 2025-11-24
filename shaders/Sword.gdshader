shader_type canvas_item;

uniform bool swing=false;
uniform float strength:hint_range(0.0f,0.5f)=0.25f;
uniform bool flip_h=false;

void fragment() {
	if(swing) {
	    vec2 uv=UV;
	    if(flip_h) {
			uv.x=1.0f-uv.x;
	    }
		
	    float offset=sin(uv.y*3.1415f)*1.0f*strength;
	    vec2 swing_uv=uv+vec2(offset,0.0f);

	    vec4 col=texture(TEXTURE,swing_uv);

	    for (int i=1;i<=3;i++) {
	        float t=float(i)/3.0f;
	        float trail_offset=offset*t;
	        vec2 trail_uv=uv+vec2(trail_offset,0.0f);
	        vec4 trail=texture(TEXTURE,trail_uv);
	        float trail_alpha=1.0f*(1.0f-t)*0.25f;
	        col.rgb+=trail.rgb*trail_alpha;
	    }

    	float glow=smoothstep(0.85f,1.0f,abs(uv.x+offset-0.5f));
    	col.rgb+=glow*1.0f*0.4f;
    	COLOR=vec4(col.rgb,col.a);
	}

}