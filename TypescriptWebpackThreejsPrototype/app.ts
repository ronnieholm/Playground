import * as THREE from 'three';

class App {
    private readonly renderer = new THREE.WebGLRenderer({
        canvas:<HTMLCanvasElement>document.getElementById("mainCanvas"),
        antialias:true,
    }); 

    private readonly scene = new THREE.Scene();
    private readonly camera = new THREE.PerspectiveCamera(45, innerWidth / innerHeight, 0.1, 10000);

    constructor() {
        const axisHelper = new THREE.AxisHelper(150);
        this.scene.add(axisHelper);

        const geometry = new THREE.CylinderGeometry(50, 50, 50, 100, 100);
        const material = new THREE.MeshLambertMaterial(/*{ color: 0x0000ff }*/);
        // material.wireframe = true;
        // material.transparent = true;
        // material.opacity = 0.1;
        const mesh = new THREE.Mesh(geometry, material);
        mesh.position.set(0, 0, 0); // default is (0, 0, 0)
        this.scene.add(mesh);

        var geometry1 = new THREE.Geometry();
        var material1 = new THREE.LineBasicMaterial({ color: 0x0000ff });
        // geometry1.vertices.push(new THREE.Vector3(-100, 0, 0));
        // geometry1.vertices.push(new THREE.Vector3(0, 100, 0));
        // geometry1.vertices.push(new THREE.Vector3(100, 0, 0));

        for (let i = 75; i < 150; i += 0.1) {
            geometry1.vertices.push(new THREE.Vector3(i, 10 * Math.sin(i), 0));
        }        

        var curve = new THREE.Line(geometry1, material1);
        this.scene.add(curve);

        // const ambient = new THREE.AmbientLight(0x999999, 0.5);
        // this.scene.add(ambient);

        const point = new THREE.PointLight(0xffff00, 50, 125);
        point.position.y = 100;
        point.position.z = -100;
        const pointHelper = new THREE.PointLightHelper(point, 5);
        this.scene.add(point);
        this.scene.add(pointHelper);

        const directional = new THREE.DirectionalLight(0xaa00bb, 0.5);
        directional.position.x = 100;
        directional.position.y = 100;
        const directionalHelper = new THREE.DirectionalLightHelper(directional, 5);
        directional.target = mesh;        
        this.scene.add(directional);
        this.scene.add(directionalHelper);

        const spotlight = new THREE.SpotLight(0xffffff, 50, 60);
        spotlight.position.z = 100;
        spotlight.position.y = 50;
        const spotlightHelper = new THREE.SpotLightHelper(spotlight);
        this.scene.add(spotlight);
        this.scene.add(spotlightHelper);

        this.camera.position.set(100, 100, 500); // default is (0, 0, 0)
        this.camera.lookAt(<THREE.Vector3>{x:0, y:0, z:0});       

        this.renderer.setSize(innerWidth, innerHeight);
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.renderer.setClearColor(new THREE.Color(0x000000));

        this.render();
        //this.renderer.render(this.scene, this.camera);
    }

    private delta = 0.0;
    private render() {
        this.renderer.render(this.scene, this.camera);

        // Rorate camera at distance 500
        this.delta += 0.01;
        this.camera.lookAt(<THREE.Vector3>{x:0, y:0, z:0});
        this.camera.position.x = Math.sin(this.delta) * 500;
        this.camera.position.z = Math.cos(this.delta) * 500;

        requestAnimationFrame(() => { this.render() });
        this.renderer.setSize(innerWidth, innerHeight);
        this.camera.aspect = innerWidth / innerHeight;
    }
}

const app = new App();