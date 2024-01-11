precision mediump float;

#define repeat(p, span) mod(p, span) - (0.5 * span)
#define map(x, min, max) (x * (max - min) + min)

struct Camera {
  vec3 position;
  mat4 projectionMatrixInverse;
  mat4 viewMatrixInverse;
  mat4 normalMatrix;
};

struct Frag {
  bool reflection;
  bool ao;
  bool shadow;
  bool godray;
  bool uv;
  bool id;
  bool pattern;
};

uniform vec2 uResolution;
uniform Camera uCamera;
uniform float uTime;
uniform float uRotation;
uniform Frag uFrag;

varying vec2 vUv;

const int MAX_STEP = 200;
const float MIN_DIST = 0.0001;
const float MAX_DIST = 200.0;

const float PI = acos(-1.0);
const float TAU = PI * 2.0;

const float EDGE_SIZE = 15.0;
const float BEVEL = 0.05;
const float REPEAT_SPAN = 1.0 + BEVEL * 2.0;
const float POLAR_DIVID = 4.0;
const vec2 WINDOW = vec2(6.0, 12.0);

float rotAngle = uRotation * 0.01;
vec3 rayOffset = vec3(0.0, -5.5, uTime * -5.0);

#include './modules/circlePattern.glsl'
#include './modules/primitives.glsl'
#include './modules/combinations.glsl'

float hash(vec2 p) { return fract(1e4 * sin(17.0 * p.x + p.y * 0.1) * (0.1 + abs(sin(p.y * 13.0 + p.x)))); }

mat2 rot(float a) {
  float s = sin(a);
  float c = cos(a);
  return mat2(c, s, -s, c);
}

vec3 pmod(vec2 p, float r) {
  float a = atan(p.x, p.y) + PI / r;
  float n = TAU / r;
  a = floor(a / n) * n;
  return vec3(p * rot(-a), a);
}

vec2 sdf(vec3 p) {
  p.xy = rot(p.z * rotAngle) * p.xy;
  p += rayOffset;

  vec3 p1 = p;
  p1.xy = pmod(p1.xy, POLAR_DIVID).xy;
  p1.xz = repeat(p1.xz, REPEAT_SPAN);
  p1.y -= (0.5 + BEVEL) * EDGE_SIZE;
  float b1 = sdBox(p1, vec3(0.5)) - BEVEL;

  vec3 p2 = p;
  float span = (0.5 + BEVEL) * WINDOW.x * 2.0 + (1.0 + BEVEL * 2.0) * 7.0;
  p2.z = repeat(p2.z, span);
  p2.z += 0.5 + BEVEL;
  float b2 = sdBox(p2, vec3(0.5 + BEVEL) * vec3(EDGE_SIZE * 2.0, WINDOW.y, WINDOW.x));

  float final = opSubtraction(b2, b1);

  return vec2(final, 0.0);
}

vec2 rayMarch(vec3 ro, vec3 rd) {
  float total = 0.0;
  vec2 d;

  for (int i = 0; i < MAX_STEP; i++) {
    vec3 p = ro + rd * total;
    d = sdf(p);
    total += d.x;
    if (abs(d.x) < MIN_DIST || MAX_DIST < total)
      break;
  }

  return vec2(total, d.y);
}

#include './modules/normal.glsl'
#include './modules/shadow.glsl'
#include './modules/ambientOcclusion.glsl'

void main() {
  vec3 ro = uCamera.position;
  vec4 ndcRay = vec4(vUv * 2.0 - 1.0, 1.0, 1.0);
  vec4 target = uCamera.viewMatrixInverse * uCamera.projectionMatrixInverse * ndcRay;
  vec3 rd = normalize(target.xyz / target.w - ro);

  vec2 rm = rayMarch(ro, rd);

  float total = rm.x;
  float materialId = rm.y;
  vec3 color = vec3(1);
  float a = mix(1.0, 0.99, float(uFrag.godray));

  if (total < MAX_DIST) {
    a = 1.0;
    vec3 p = ro + rd * total;
    vec3 normal = calcNormal(p);
    vec3 R = reflect(rd, normal);
    vec3 light = vec3(-50.0, 30.0, -50.0);
    vec3 L = normalize(light - p);

    color = vec3(1.0);

    float reflectionIntensity = 0.95;

    vec2 th = rot(p.z * rotAngle) * p.xy;
    bool isFloor = 0.5 < dot(normal, vec3(0, 1, 0)) && th.y < -2.0;
    bool isCeil = 0.5 < dot(normal, vec3(0, -1, 0)) && 12.2 < th.y;
    bool isLeftWall = 0.5 < dot(normal, vec3(1, 0, 0));
    bool isRightWall = 0.5 < dot(normal, vec3(-1, 0, 0));

    if (isFloor || isCeil || isLeftWall || isRightWall) {
      vec3 pp = p;
      pp.xy = rot(pp.z * rotAngle) * pp.xy;
      pp += rayOffset;

      vec3 pm = pmod(pp.xy, POLAR_DIVID);
      pp.xy = pm.xy;
      pp.y -= (0.5 + BEVEL) * EDGE_SIZE;
      vec2 uv = fract(pp.xz / REPEAT_SPAN);
      vec2 id = floor(pp.xz / REPEAT_SPAN);
      float r = hash(id + pm.z);

      if (uFrag.id) {
        color *= r;
      }
      if (uFrag.uv) {
        color *= vec3(uv, 1.0);
      }
      if (uFrag.pattern) {
        vec3 pattern = genPattern(uv);
        float mixedPattern = mix(pattern.x, pattern.y, step(0.33, r));
        mixedPattern = mix(mixedPattern, pattern.z, step(0.66, r));
        color *= map(mixedPattern, 0.5, 1.0);
        reflectionIntensity *= map(mixedPattern, 0.2, 1.0);
      }
      if (!uFrag.id && !uFrag.uv && !uFrag.pattern) {
        color *= map(step(0.15, r), 0.8, 1.0);
      } 
    }

    if (uFrag.reflection) {
      float dist = rayMarch(p + normal * MIN_DIST * 2.0, R).x;
      if (dist < MAX_DIST) {
        color *= reflectionIntensity;
      }
    }

    if (uFrag.ao) {
      float ao = ambientOcclusion(p, normal, 0.5, 1.0);
      color *= map(ao, 0.2, 1.0);
    }

    if (uFrag.shadow) {
      float shadow = calcShadow(p + normal * MIN_DIST * 2.0, light);
      color *= map(shadow, 0.7, 1.0);
    }

    float fade = smoothstep(MAX_DIST * 0.3, MAX_DIST * 0.8, total);
    color += fade;
  }

  // vec2 uv = vUv * 2.0 - 1.0;
  // uv = rot(PI * 0.5) * uv;
  // float angle = atan(uv.y, uv.x);
  // angle = (angle / PI) * 0.5 + 0.5;
  // angle = step(0.0, angle);

  // vec4 final = vec4(color, a);
  // final = mix(vec4(vec3(0), 1), final, angle);

  gl_FragColor = vec4(color, a);
}