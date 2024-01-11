precision mediump float;

uniform sampler2D tMain;
uniform sampler2D tOcclusion;
uniform sampler2D tGodray;

varying vec2 vUv;

void main() {
  vec4 main = texture2D(tMain, vUv);
  vec4 occlusion = texture2D(tOcclusion, vUv);
  vec4 godray = texture2D(tGodray, vUv);
  main += occlusion;
  main += godray;
  
  gl_FragColor = vec4(main.rgb, 1.0);
}