var Mandelbrot = function (canvas, xMin, xMax, yMin, yMax, maxIter) {
    this.image = null;

    this.setup = function () {
        var ctx = canvas.getContext("2d");
        this.image = ctx.createImageData(canvas.width, canvas.height);
    };

    this.setPixel = function (x, y, r, g, b) {
        var idx = (x + y * this.image.width) * 4;
        var data = this.image.data
        data[idx + 0] = r;
        data[idx + 1] = g;
        data[idx + 2] = b;
        data[idx + 3] = 255;
    };

    this.computeIterations = function (cx, cy, maxIter) {
        var x = 0.0;
        var y = 0.0;

        for (var i = 0; i < maxIter && x * x + y * y <= 4; ++i) {
            var tmp = 2 * x * y;
            x = x * x - y * y + cx;
            y = tmp + cy;
        }
        return i;
    };

    this.draw = function () {
        var w = this.image.width;
        var h = this.image.height;

        for (var x = 0; x < w; ++x) {
            for (var y = 0; y < h; ++y) {
		this.setPixel(x, y, 127, 127, 127);
                var mx = xMin + (xMax - xMin) * x / (this.image.width - 1);
                var my = yMin + (yMax - yMin) * y / (this.image.height - 1);
                var i = this.computeIterations(mx, my, maxIter);

                if (i === maxIter) {
                    this.setPixel(x, y, 0, 0, 0);
                } else {
                    var c = 3 * Math.log(i) / Math.log(maxIter - 1.0);
                    if (c < 1) {
                        this.setPixel(x, y, 255 * c, 0, 0);
                    }
                    else if (c < 2) {
                        this.setPixel(x, y, 255, 255 * (c - 1), 0);
                    }
                    else {
                        this.setPixel(x, y, 255, 255, 255 * (c - 2))
                    }
                }
            }
        }
    };
};

function run() {
    var canvasId = "canvas";
    var canvas = document.getElementById(canvasId);
    var ctx = canvas.getContext("2d");
    var m = new Mandelbrot(canvas, -2, 1, -1, 1, 10000);
    m.setup();
    var image = m.draw();
    ctx.putImageData(m.image, 0, 0);
}
