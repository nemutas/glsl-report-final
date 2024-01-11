float circleStroke(vec2 uv, vec2 p) {
  float d = distance(uv, p);
  float t = 0.02;
  return 1.0 - smoothstep(0.45, 0.45 + t, d) * smoothstep(0.55, 0.55 - t, d);
}

float star(vec2 uv) {
  float t = 0.02;
  float d = 1.0 - smoothstep(0.45, 0.45 + t, distance(uv, vec2(0, 0)));
  d += 1.0 - smoothstep(0.45, 0.45 + t, distance(uv, vec2(0, 1)));
  d += 1.0 - smoothstep(0.45, 0.45 + t, distance(uv, vec2(1, 0)));
  d += 1.0 - smoothstep(0.45, 0.45 + t, distance(uv, vec2(1, 1)));
  return clamp(d, 0.0, 1.0);
}

vec3 genPattern(vec2 uv) {
  float p1 = circleStroke(uv, vec2(0, 1)) * circleStroke(uv, vec2(1, 0));
  float p2 = circleStroke(uv, vec2(0, 0)) * circleStroke(uv, vec2(1, 1));
  float p3 = star(uv);
  return vec3(p1, p2, p3);
}
