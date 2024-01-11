precision mediump float;

uniform sampler2D tDiffuse;

varying vec2 vUv;

void main() {
  vec4 scene = texture2D(tDiffuse, vUv);
  float a = 1.0 - step(0.9999, scene.a);
  // a *= 1.0 - step(0.45, vUv.x);
  gl_FragColor = vec4(vec3(a), 1.0);
}