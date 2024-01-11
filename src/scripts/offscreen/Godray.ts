import * as THREE from 'three'
import { Canvas } from '../Canvas'
import { Offscreen } from './Offscreen'
import vertexShader from '../shader/screen.vs'
import fragmentShader from '../shader/godray.fs'
import { gui } from '../Gui'

export class Godray extends Offscreen {
  constructor(canvas: Canvas, source: THREE.Texture) {
    const material = new THREE.RawShaderMaterial({
      uniforms: {
        tDiffuse: { value: source },
        // lightPosition: { value: [-0.2, 1.2] },
        lightPosition: { value: [0.6, 0.5] },
        exposure: { value: 0.55 },
        decay: { value: 0.95 },
        density: { value: 0.31 },
        weight: { value: 0.12 },
        samples: { value: 50 },
      },
      vertexShader,
      fragmentShader,
    })

    super(canvas, material)

    this.setControls()
  }

  private setControls() {
    const folder = gui.addFolder('godray')
    folder.close()

    const add = (name: string, min = 0, max = 1, step = 0.01) => {
      folder.add(this.uniforms[name], 'value', min, max, step).name(name)
    }

    add('exposure')
    add('decay')
    add('density')
    add('weight')
    add('samples', 10, 100, 10)
  }
}
