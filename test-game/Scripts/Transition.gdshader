shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.0); // 0 = full màn hình, 1 = thu nhỏ hết

void fragment() {
    vec2 uv = SCREEN_UV;
    vec2 center = vec2(0.5, 0.5);
    float radius = mix(1.5, 0.0, progress); // giảm bán kính theo progress

    float dist = distance(uv, center);
	if (dist < radius)
    discard;
else
    COLOR = vec4(0.0, 0.0, 0.0, 1.0); // đen
}