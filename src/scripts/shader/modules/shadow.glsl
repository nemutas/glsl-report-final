float calcShadow(vec3 ro, vec3 lp) {
  const int MAX_STEP = 200;
  const float MIN_DIST = 0.0001;
  const float MAX_DIST = 100.0;

  float ld = distance(lp, ro);
  vec3 rd = normalize(lp - ro);
  float total = 0.0;
  float result = 1.0;

  for (int i = 0; i < MAX_STEP; i++) {
    vec3 p = ro + rd * total;
    float d = sdf(p).x;
    total += d;
    if (abs(d) < MIN_DIST) {
      result = 0.0;
      break;
    } else if (ld < total || MAX_DIST < total) {
      break;
    }
  }

  return result;
}