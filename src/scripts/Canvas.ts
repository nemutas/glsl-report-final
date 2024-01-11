import * as THREE from 'three'
import { Three } from './core/Three'
import vertexShader from './shader/screen.vs'
import fragmentShader from './shader/blend.fs'
import { Main } from './offscreen/Main'
import { Occlusion } from './offscreen/Occlusion'
import { Offscreen } from './offscreen/Offscreen'
import { Godray } from './offscreen/Godray'
import { gui } from './Gui'

export class Canvas extends Three {
  private offscreens: Offscreen[] = []

  constructor(canvas: HTMLCanvasElement) {
    super(canvas)
    this.init()
    this.setControls()
    this.createPass()

    window.addEventListener('resize', this.resize.bind(this))
    this.renderer.setAnimationLoop(this.anime.bind(this))
  }

  private init() {
    // this.camera.position.z = 10
    this.camera.position.set(6.29, -1.36, 4.29)
    this.controls.enabled = false
    this.controls.enableDamping = true
    this.controls.dampingFactor = 0.15
  }

  private createPass() {
    const main = new Main(this)
    const occlusion = new Occlusion(this, main.texture)
    const godray = new Godray(this, occlusion.texture)
    this.offscreens.push(main, occlusion, godray)

    this.createOutput(main.texture, occlusion.texture, godray.texture)
  }

  private createOutput(main: THREE.Texture, occlusion: THREE.Texture, godray: THREE.Texture) {
    const geometry = new THREE.PlaneGeometry(2, 2)
    const material = new THREE.RawShaderMaterial({
      uniforms: {
        tMain: { value: main },
        tOcclusion: { value: occlusion },
        tGodray: { value: godray },
      },
      vertexShader,
      fragmentShader,
    })
    const mesh = new THREE.Mesh(geometry, material)
    this.scene.add(mesh)
    return mesh
  }

  private setControls() {
    this.stats.dom.style.setProperty('display', 'none')

    const obj = {
      fps: false,
    }

    const showFpsCounter = (is: boolean) => {
      if (is) {
        this.stats.dom.style.removeProperty('display')
      } else {
        this.stats.dom.style.setProperty('display', 'none')
      }
    }

    showFpsCounter(obj.fps)

    gui.add(obj, 'fps').onChange((v: boolean) => showFpsCounter(v))
    gui.add(this.controls, 'enabled').name('camera control')
  }

  private resize() {
    this.offscreens.forEach((o) => o.resize())
  }

  private anime() {
    this.stats.update()
    this.controls.update()
    this.updateTime()

    console.log(this.camera.position, this.controls.target)

    this.offscreens.forEach((o) => o.render())
    this.render()
  }
}
