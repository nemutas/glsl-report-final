import * as THREE from 'three'
import { Canvas } from '../Canvas'
import { Offscreen } from './Offscreen'
import vertexShader from '../shader/screen.vs'
import fragmentShader from '../shader/screen.fs'
import { gui } from '../Gui'

export class Main extends Offscreen {
  private obj = {
    time: true,
  }

  constructor(canvas: Canvas) {
    const material = new THREE.RawShaderMaterial({
      uniforms: {
        uResolution: { value: [canvas.size.width, canvas.size.height] },
        uCamera: {
          value: {
            position: canvas.camera.position,
            projectionMatrixInverse: canvas.camera.projectionMatrixInverse,
            viewMatrixInverse: canvas.camera.matrixWorld,
            normalMatrix: canvas.camera.matrixWorld.clone().transpose(),
          },
        },
        uTime: { value: 0 },
        uRotation: { value: 0.4 },
        uFrag: {
          value: {
            reflection: true,
            ao: true,
            shadow: true,
            godray: true,
            uv: false,
            id: false,
            pattern: true,
          },
        },
      },
      vertexShader,
      fragmentShader,
      transparent: true,
    })

    super(canvas, material)

    this.setControls()
  }

  private setControls() {
    gui.add(this.obj, 'time')
    gui.add(this.uniforms.uFrag.value, 'ao')
    gui.add(this.uniforms.uFrag.value, 'shadow')
    gui.add(this.uniforms.uFrag.value, 'reflection')
    gui.add(this.uniforms.uFrag.value, 'godray')
    gui.add(this.uniforms.uFrag.value, 'uv')
    gui.add(this.uniforms.uFrag.value, 'id')
    gui.add(this.uniforms.uFrag.value, 'pattern')
    gui.add(this.uniforms.uRotation, 'value', -1, 1, 0.01).name('rotation')
  }

  resize() {
    this.uniforms.uResolution.value = [this.canvas.size.width, this.canvas.size.height]
    super.resize()
  }

  render() {
    if (this.obj.time) {
      this.uniforms.uTime.value += this.canvas.time.delta
    }
    this.uniforms.uCamera.value.normalMatrix = this.canvas.camera.matrixWorld.clone().transpose()
    super.render()
  }
}
