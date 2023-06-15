// Settings
const settings = {
    skipGPSFrames: true,
    //angleSpan: 2.5,
    textColor: "#7CFC00",
    alternateColor: "#c1fc00",
    graphColors: ["#ff0000", "#00ff00", "#0000ff"],
    showHitboxes: false,
    maxZoom: 20,
    minZoom: 1,
    connectedLines: true,
}
const dataInfo = {
    frame: {
        label: "Current frame",
        range: [0, 16500],
        colors: [settings.textColor],
        data: ["frame"]
    },
    time: {
        label: "Time (UTC)",
        range: ["13:39:11:0", "17:20:00:0"],
        colors: [settings.textColor],
        data: ["time"]
    },
    ms_since_last_cycle: {
        label: "Time since last cycle (ms)",
        range: [1000, 1050],
        colors: [settings.textColor],
        data: ["ms_since_last_cycle"]
    },
    latitude: {
        label: "Latitude",
        range: [3910, 3940],
        colors: [settings.textColor],
        data: ["latitude"]
    },
    longitude: {
        label: "Longitude",
        range: [12130, 12160],
        colors: [settings.textColor],
        data: ["longitude"]
    },
    altitude: {
        label: "Altitude (m)",
        range: [0, 32200],
        colors: [settings.textColor],
        data: ["altitude"]
    }, 
    speed: {
        label: "Speed (km/h)",
        range: [0, 40],
        colors: [settings.textColor],
        data: ["speed"]
    },
    satellites: {
        label: "Number of Satellites",
        range: [0, 20],
        colors: [settings.textColor],
        data: ["satellites"]
    },
    avg_thermistor: {
        label: "Average Thermistor Reading (Volts)",
        range: [400, 1100],
        colors: [settings.textColor],
        data: ["avg_thermistor"]
    },
    thermistor_c: {
        label: "Temperature (ºC)",
        range: [-60 , 20],
        colors: [settings.textColor],
        data: ["thermistor_c"]
    },
    gyro_xyz: {
        label: "Gyroscope (º/s)",
        range: [-0.3, 0.3],
        colors: settings.graphColors,
        data: ["gyro_x", "gyro_y", "gyro_z"]
    },
    accel_xyz: {
        label: "Accelerometer (g)",
        range: [-1, 1],
        colors: settings.graphColors,
        data: ["accel_x", "accel_y", "accel_z"]
    },
    mag_xyz: {
        label: "Magnetometer (uT)",
        range: [-50, 50],
        colors: settings.graphColors,
        data: ["mag_x", "mag_y", "mag_z"]
    },
    pressure: {
        label: "Pressure (hPa)",
        range: [0, 1200],
        colors: [settings.textColor],
        data: ["pressure"]
    },
    humidity: {
        label: "Relative humidity (%)",
        range: [0, 100],
        colors: [settings.textColor],
        data: ["humidity"]
    },
    upward_speed: {
        label: "Upward speed (m/s)",
        range: [-15, 5],
        colors: [settings.textColor],
        data: ["upward_speed"]
    },
    avg_upward_speed: {
        label: "Average upward speed (m/s)",
        range: [-15, 5],
        colors: [settings.textColor],
        data: ["avg_upward_speed"]
    },
}
const hitboxes = {
    x_axis: [300, 370, 220, 20],
    y_axis: [220, 100, 20, 220],
    time: [10, 6, 100, 22],
    frame: [240, 8, 160, 20],
    ms_since_last_cycle: [10, 28, 210, 20],
    latitude: [10, 70, 160, 22],
    longitude: [10, 92, 180, 22],
    altitude: [10, 116, 120, 22],
    speed: [10, 139, 110, 22],
    satellites: [10, 162, 120, 22],
    avg_thermistor: [10, 185, 210, 22],
    thermistor_c: [10, 207, 160, 22],
    gyro_xyz: [10, 231, 210, 22],
    accel_xyz: [10, 253, 210, 22],
    mag_xyz: [10, 275, 210, 22],
    pressure: [10, 297, 210, 22],
    humidity: [10, 319, 210, 22],
    upward_speed: [10, 341, 210, 22],
    avg_upward_speed: [10, 363, 210, 22],
    graph: [250, 50, 340, 310],
}
let DATA = [];
let values = [];
let colors = [];
let dataLoaded = false;
let sliderClicked = false;
let frameSlider;
let watermark = false;
let selectedAxes = ["altitude", "thermistor_c"];
let selected = "none";
function cmap(v, index=0) {
    if (colors.length === 0) return '#ff000000';
    return colors[index][Math.floor(v * 255)];
}
function clamp(x) {
    return Math.max(0, Math.min(1, x));
}
let index = 10;
let playPause = false;
let p5Time = 0;
let mostRecentClick = {clickType: "none", p5Time: 0};
for (let i = 0; i < 32; i++) {
    for (let j = 0; j < 24; j++) {
        values.push('#000000');
    }
}
let camera = function( sketch ) {
    sketch.setup = function() {
        let cnv = sketch.createCanvas(640, 480);
        cnv.parent('sketch-holder');
        sketch.textSize(24);
        sketch.textAlign(sketch.CENTER, sketch.CENTER);
        sketch.text("Loading data...", 320, 240);
        frameSlider = sketch.createSlider(0, 100, 0, 1);
        frameSlider.id("frame-slider");
        frameSlider.parent("slider-holder");
    };

    sketch.draw = function() {
        p5Time = sketch.millis();
        document.getElementById('loading').innerHTML = "Loading" + [...Array((Math.floor(sketch.frameCount/40)%4)).keys()].map(_ => ".").join('');
        if (sliderClicked) {
            index = frameSlider.value();
        }
        sketch.background(0);
        sketch.noStroke();
        for (let i = 0; i < 32; i++) {
            for (let j = 0; j < 24; j++) {
                sketch.fill(values[32*j + i]);
                sketch.rect(20*i, 20*j, 20, 20);
            }
        }
        dataLoaded&&updateInfo(index);
        if(playPause && !sliderClicked && index < DATA.length - 1) {
            index++;
            
        }
        // sketch.stroke(255);
        // if (dataLoaded) {
        //     let angle = mangetometerToStuff(DATA[index].mag_x, DATA[index].mag_y, DATA[index].mag_z);
        //     sketch.line(300, 300, 300+100*Math.cos(angle), 300+100*Math.sin(angle));
        // }
        sketch.fill(settings.textColor + "88");
        watermark&&sketch.text("Created by Sean Kuwamoto", 320, 20);
    }

    sketch.keyPressed = function() {
        if (sketch.keyCode === sketch.LEFT_ARROW) {
            if (p5Time - mostRecentClick.p5Time < 150 && mostRecentClick.clickType === "left") {
                index -= Math.floor(DATA.length / 50);
            }
            else {
                index--;
            }
            mostRecentClick = {clickType: "left", p5Time: p5Time};
            if (index < 0) index = 0;
        }
        else if (sketch.keyCode === sketch.RIGHT_ARROW) {
            if (p5Time - mostRecentClick.p5Time < 150 && mostRecentClick.clickType === "right") {
                index += Math.floor(DATA.length / 50);
            }
            else {
                index++;
            }
            mostRecentClick = {clickType: "right", p5Time: p5Time};
            if (index >= DATA.length) index = DATA.length - 1;
        }
        else if (sketch.keyCode === " ".charCodeAt(0)) {
            playPause = !playPause;
        }
        updateInfo(index);

    }

};
let p5one = new p5(camera);


const controls = {
    view: {x: 0, y: 0, zoom: 1},
    viewPos: {prevX: null, prevY: null, isDragging: false},
}
let info = function( sketch ) {
    sketch.setup = function() {
        let cnv = sketch.createCanvas(640, 480);
        cnv.mouseWheel(e => detectHover(sketch, hitboxes.graph)&&Controls.zoom(controls).worldZoom(e));
        cnv.id("info-canvas");
        cnv.parent('info-holder');
        sketch.background(30);
        sketch.textSize(24);
        
    };

    sketch.draw = function() {
        sketch.background(30);
        sketch.fill(255);
        sketch.noStroke();  

        // Graph predraw
        sketch.noFill()
        for (let line = 0; line < Math.max(dataInfo[selectedAxes[0]].data.length, dataInfo[selectedAxes[1]].data.length); line++) {
            sketch.stroke(dataInfo[selectedAxes[1]].colors[line % dataInfo[selectedAxes[1]].colors.length]);
            settings.connectedLines&&sketch.beginShape();
            for (let i = 0; i < index; i++) {
                sketch.strokeWeight(1);
                let [px, py] = transform(
                    250 + inverseLerp(selectedAxes[0], i, line)*(590 - 250),
                    360 + inverseLerp(selectedAxes[1], i, line)*(50 - 360)
                );
                settings.connectedLines?sketch.vertex(px, py):sketch.point(px, py);
            }
            settings.connectedLines&&sketch.endShape();
        }
        sketch.noStroke();
        // Covering rectangles
        sketch.fill(30);
        sketch.rect(0, 0, 250, 480);
        sketch.rect(250, 360, 390, 120);
        sketch.rect(0, 0, 640, 50);
        sketch.rect(590, 50, 50, 310);

        // Hover detector
        let hovered;
        for (let [key, value] of Object.entries(hitboxes)) {
            if (detectHover(sketch, value)) {
                hovered = key;
            }
        }
        
        // Time
        sketch.textAlign(sketch.LEFT, sketch.TOP);
        sketch.fill(hovered==="time"?settings.alternateColor:settings.textColor);
        sketch.textSize(18);
        sketch.text(dataLoaded?DATA[index].time:"0:0:0:0", 10, 10);

        // Date
        sketch.textAlign(sketch.RIGHT, sketch.TOP);
        sketch.text(dataLoaded?DATA[index].date:"0/0/0", 630, 10);

        // frameCount
        sketch.textAlign(sketch.CENTER, sketch.TOP);
        sketch.fill(hovered==="frame"?settings.alternateColor:settings.textColor);
        sketch.text("current frame: " + (dataLoaded?index:"0")/* + " mouse pos: " + "(" + sketch.mouseX + "," + sketch.mouseY + ")"*/, 320, 10);

        

        sketch.textAlign(sketch.LEFT, sketch.CENTER);
        sketch.fill(hovered==="ms_since_last_cycle"?settings.alternateColor:settings.textColor);
        sketch.text("ms since last cycle: " + (dataLoaded?DATA[index].ms_since_last_cycle:"0"), 10, 37);
        sketch.fill(hovered==="fixed"?settings.alternateColor:settings.textColor);
        sketch.text("fixed: " + (dataLoaded?DATA[index].fixed:"0"), 10, 60);
        sketch.fill(hovered==="latitude"?settings.alternateColor:settings.textColor);
        sketch.text("latitude: " + (dataLoaded?DATA[index].latitude:"0"), 10, 83);
        sketch.fill(hovered==="longitude"?settings.alternateColor:settings.textColor);
        sketch.text("longitude: " + (dataLoaded?DATA[index].longitude:"0"), 10, 104);
        sketch.fill(hovered==="altitude"?settings.alternateColor:settings.textColor);
        sketch.text("altitude: " + (dataLoaded?DATA[index].altitude:"0"), 10, 128);
        sketch.fill(hovered==="speed"?settings.alternateColor:settings.textColor);
        sketch.text("speed: " + (dataLoaded?DATA[index].speed:"0"), 10, 151);
        sketch.fill(hovered==="satellites"?settings.alternateColor:settings.textColor);
        sketch.text("satellites: " + (dataLoaded?DATA[index].satellites:"0"), 10, 175);
        sketch.fill(hovered==="avg_thermistor"?settings.alternateColor:settings.textColor);
        sketch.text("avg thermistor: " + (dataLoaded?DATA[index].avg_thermistor:"0"), 10, 198);
        sketch.fill(hovered==="thermistor_c"?settings.alternateColor:settings.textColor);
        sketch.text("thermistor c: " + (dataLoaded?DATA[index].thermistor_c:"0"), 10, 219);
        sketch.fill(hovered==="pressure"?settings.alternateColor:settings.textColor);
        sketch.text("pressure: " + (dataLoaded?DATA[index].pressure:"0"), 10, 310);
        sketch.fill(hovered==="humidity"?settings.alternateColor:settings.textColor);
        sketch.text("humidity: " + (dataLoaded?DATA[index].humidity:"0"), 10, 332);
        sketch.fill(hovered==="upward_speed"?settings.alternateColor:settings.textColor);
        sketch.text("upward speed: " + (dataLoaded?Math.round(10*DATA[index].upward_speed)/10:"0"), 10, 355);
        sketch.fill(hovered==="avg_upward_speed"?settings.alternateColor:settings.textColor);
        sketch.text("avg upward speed: " + (dataLoaded?Math.round(10*DATA[index].avg_upward_speed)/10:"0"), 10, 378);

        function drawtext( x, y, text_array ) {
            let pos_x = x;
            for ( let part of text_array ) {
                let t = part[0];
                let c = part[1];
                let w = sketch.textWidth( t );
                sketch.fill( c );
                sketch.text( t, pos_x, y);
                pos_x += w;
            }
        }
        let gyro = [
            ["gyro xyz: ", hovered==="gyro_xyz"?settings.alternateColor:settings.textColor],
            [(dataLoaded?DATA[index].gyro_x:"0") + " ", hovered==="gyro_xyz"?settings.alternateColor:settings.graphColors[0]],
            [(dataLoaded?DATA[index].gyro_y:"0") + " ", hovered==="gyro_xyz"?settings.alternateColor:settings.graphColors[1]],
            [(dataLoaded?DATA[index].gyro_z:"0"), hovered==="gyro_xyz"?settings.alternateColor:settings.graphColors[2]],
        ]
        drawtext(10, 243, gyro);
        let accel = [
            ["accel xyz: ", hovered==="accel_xyz"?settings.alternateColor:settings.textColor],
            [(dataLoaded?DATA[index].accel_x:"0") + " ", hovered==="accel_xyz"?settings.alternateColor:settings.graphColors[0]],
            [(dataLoaded?DATA[index].accel_y:"0") + " ", hovered==="accel_xyz"?settings.alternateColor:settings.graphColors[1]],
            [(dataLoaded?DATA[index].accel_z:"0"), hovered==="accel_xyz"?settings.alternateColor:settings.graphColors[2]],
        ]
        drawtext(10, 266, accel);
        let mag = [
            ["mag xyz: ", hovered==="mag_xyz"?settings.alternateColor:settings.textColor],
            [(dataLoaded?DATA[index].mag_x:"0") + " ", hovered==="mag_xyz"?settings.alternateColor:settings.graphColors[0]],
            [(dataLoaded?DATA[index].mag_y:"0") + " ", hovered==="mag_xyz"?settings.alternateColor:settings.graphColors[1]],
            [(dataLoaded?DATA[index].mag_z:"0"), hovered==="mag_xyz"?settings.alternateColor:settings.graphColors[2]],
        ]
        drawtext(10, 289, mag);
        // sketch.text("gyro xyz: " + (dataLoaded?DATA[index].gyro_x:"0") + " " + (dataLoaded?DATA[index].gyro_y:"0") + " " + (dataLoaded?DATA[index].gyro_z:"0"), 10, 243);
        // sketch.fill(hovered==="accel_xyz"?settings.alternateColor:settings.textColor);
        // sketch.text("accel xyz: " + (dataLoaded?DATA[index].accel_x:"0") + " " + (dataLoaded?DATA[index].accel_y:"0") + " " + (dataLoaded?DATA[index].accel_z:"0"), 10, 266);
        // sketch.fill(hovered==="mag_xyz"?settings.alternateColor:settings.textColor);
        // sketch.text("mag xyz: " + (dataLoaded?DATA[index].mag_x:"0") + " " + (dataLoaded?DATA[index].mag_y:"0") + " " + (dataLoaded?DATA[index].mag_z:"0"), 10, 288);


        sketch.textSize(24);
        sketch.textAlign(sketch.CENTER, sketch.CENTER);
        sketch.fill(255, 255, 255, 60);
        // sketch.rect(0, 320, 160, 480);
        sketch.rect(160, 400, 640, 480);
        sketch.fill(cmap(0));
        sketch.text(dataLoaded?Math.round(DATA[index].min_temp)+"º C":"-Infinity", 200, 440);
        sketch.fill(cmap(1));
        sketch.text(dataLoaded?Math.round(DATA[index].max_temp)+"º C":"Infinity", 600, 440);

        for(let i = 0; i < 255; i++) {
            sketch.fill(cmap(i/255));
            sketch.rect(240 + 300 * i/255, 410, 20, 60);
        }

        // for (let i = -settings.angleSpan; i <= -settings.angleSpan + 2 * settings.angleSpan * clamp(((dataLoaded?DATA[index].thermistor_c:0) - dataInfo.thermistor_c.range[0])/(dataInfo.thermistor_c.range[1] - dataInfo.thermistor_c.range[0])); i+= 0.05) {
        //     sketch.fill(cmap(i/(2*settings.angleSpan) + 0.5, 1));
        //     sketch.ellipse(80 + 60*sketch.sin(i), 400 - 60*sketch.cos(i), 20, 20);
        // }
        // sketch.text((dataLoaded?sketch.round(DATA[index].thermistor_c):"??") + "º C", 80, 400);


        // Graph

        // Title
        sketch.textAlign(sketch.CENTER, sketch.TOP);
        sketch.textSize(18);
        sketch.fill(settings.textColor);
        sketch.text(dataInfo[selectedAxes[1]].label + " vs. " + dataInfo[selectedAxes[0]].label, 440, 40);
        let cnvRect = document.getElementById('info-canvas').getBoundingClientRect();
        // sketch.text("Zoom: " + controls.view.zoom.toFixed(2) + " Translate: " + 
        // ((controls.view.x - hitboxes.graph[0])/controls.view.zoom + hitboxes.graph[0])
        //  + ", " + 
        // ((controls.view.y - hitboxes.graph[1])/controls.view.zoom+ hitboxes.graph[1]), 440, 70);
        // sketch.text("Real:" + controls.view.x + ", " + controls.view.y, 440, 100);

        sketch.stroke(settings.textColor);
        sketch.fill(settings.textColor);
        // left line
        sketch.line(250, 360, 250, 50);
        // bottom line
        sketch.line(250, 360, 590, 360);
        sketch.noStroke();
        sketch.textAlign(sketch.CENTER, sketch.TOP);
        sketch.textSize(18);
        sketch.fill(hovered==="x_axis"||selected==="x_axis"?settings.alternateColor:settings.textColor);
        sketch.text(selected==="x_axis"?"Selecting...":dataInfo[selectedAxes[0]].label, 420, 370);
        

        // Tickmarks
        sketch.stroke(settings.textColor);
        sketch.noFill();


        // Hitboxes
        if (settings.showHitboxes) {
            for (let [_, value] of Object.entries(hitboxes)) {
                sketch.rect(...value);
            }
        }
        

        // bottom line
        sketch.line(590, 350, 590, 370);
        sketch.line(250, 350, 250, 370);
        sketch.textAlign(sketch.CENTER, sketch.TOP);
        sketch.textSize(14);
        sketch.fill(settings.textColor);
        let xMax = lerp(selectedAxes[0], 1/controls.view.zoom - (((controls.view.x - hitboxes.graph[0])/controls.view.zoom + hitboxes.graph[0]))/hitboxes.graph[2]);
        let xMin = lerp(selectedAxes[0], -(((controls.view.x - hitboxes.graph[0])/controls.view.zoom + hitboxes.graph[0]))/hitboxes.graph[2]);
        sketch.text(selectedAxes[0]==="time"?xMax:Math.round(xMax*100)/100, 590, 380);
        sketch.text(selectedAxes[0]==="time"?xMin:Math.round(xMin*100)/100, 250, 380);
        
        // left line
        sketch.line(240, 50, 260, 50);
        sketch.line(240, 360, 260, 360);
        sketch.textAlign(sketch.RIGHT, sketch.CENTER);
        sketch.textSize(14);
        sketch.fill(settings.textColor);
        let yMax = lerp(selectedAxes[1], 1 - (-((controls.view.y - hitboxes.graph[1])/controls.view.zoom + hitboxes.graph[1])/hitboxes.graph[3]));
        let yMin = lerp(selectedAxes[1], 1 - (1/controls.view.zoom - (((controls.view.y - hitboxes.graph[1])/controls.view.zoom + hitboxes.graph[1]))/hitboxes.graph[3]));
        sketch.text(selectedAxes[1]==="time"?yMax:Math.round(yMax*100)/100, 230, 50);
        sketch.text(selectedAxes[1]==="time"?yMin:Math.round(yMin*100)/100, 230, 360);
        sketch.textSize(18);
        
        // Gridlines
        // sketch.stroke(settings.textColor + "60");
        // for(let i = 0; i <= 10; i++) {
        //     sketch.line(...transform(250 + 34*i, 360), ...transform(250 + 34*i, 50));
        //     sketch.line(...transform(250, 360 - 31*i), ...transform(590, 360 - 31*i));
        // }

        
        // actual graph
        // sketch.translate(controls.view.x, controls.view.y);
        // sketch.scale(controls.view.zoom);
        sketch.noFill();
        for (let line = 0; line < Math.max(dataInfo[selectedAxes[0]].data.length, dataInfo[selectedAxes[1]].data.length); line++) {
            sketch.stroke(dataInfo[selectedAxes[1]].colors[line % dataInfo[selectedAxes[1]].colors.length]);

            // Zero lines
            if (selectedAxes[0] !== "time") 
            {
                let xZero = 250 +((0 - dataInfo[selectedAxes[0]].range[0])/(dataInfo[selectedAxes[0]].range[1] - dataInfo[selectedAxes[0]].range[0]))*(590-250);
                let transformed = controls.view.x + (controls.view.zoom * xZero);
                if (!(transformed <= 250 || transformed >= 590)) {
                    sketch.line(transformed, 50, transformed, 370);
                    sketch.textSize(14);
                    sketch.fill(settings.textColor);
                    sketch.text(0, transformed, 380);
                    sketch.noFill();
                    sketch.textSize(18);
                }
            }
            if (selectedAxes[1] !== "time") {
                let yZero = 360 + ((0 - dataInfo[selectedAxes[1]].range[0])/(dataInfo[selectedAxes[1]].range[1] - dataInfo[selectedAxes[1]].range[0]))*(50-360);
                let transformed = controls.view.y + (controls.view.zoom * yZero);
                if (!(transformed <= 50 || transformed >= 360)) {
                    sketch.line(240, transformed, 590, transformed);
                    sketch.textSize(14);
                    sketch.fill(settings.textColor);
                    sketch.text(0, 230, transformed);
                    sketch.noFill();
                    sketch.textSize(18);
                }

            }
            
        }
        sketch.noStroke();


        sketch.fill(hovered==="y_axis"||selected==="y_axis"?settings.alternateColor:settings.textColor);

        sketch.textAlign(sketch.CENTER, sketch.BOTTOM);
        sketch.rotate(-sketch.PI/2);
        sketch.text(selected==="y_axis"?"Selecting...":dataInfo[selectedAxes[1]].label, -205, 240);

    }

    sketch.mousePressed = function(e) {
        detectHover(sketch, hitboxes.graph)&&Controls.move(controls).mousePressed(e);
        for (let [key, value] of Object.entries(hitboxes)) {
            if (detectHover(sketch, value)) {
                if (key === "graph") {
                    selected = "none";
                    return;
                }
                if (key === "x_axis") {
                    selected = "x_axis";
                    return;
                }
                if (key === "y_axis") {
                    selected = "y_axis";
                    return;
                }
                if (selected === "x_axis") selectedAxes[0] = key;
                if (selected === "y_axis") selectedAxes[1] = key;
                selected = "none";
                return;
            }
        }
        selected = "none";
    }
    sketch.mouseDragged = function(e) {
        detectHover(sketch, hitboxes.graph)&&Controls.move(controls).mouseDragged(e);
    }
    sketch.mouseReleased = function(e) {
        detectHover(sketch, hitboxes.graph)&&Controls.move(controls).mouseReleased(e);
    }
};
let p5two = new p5(info);

function transform(x, y) {
    return [
        controls.view.x + (controls.view.zoom * x),
        controls.view.y + (controls.view.zoom * y)
    ];
}

function clampTranslations() {
    function deconvert(x, y) {
        return [
            controls.view.zoom * (x - hitboxes.graph[0]) + hitboxes.graph[0],
            controls.view.zoom * (y - hitboxes.graph[1]) + hitboxes.graph[1]
        ];
    }
    let [maxX, maxY] = deconvert(0, 0);
    let [minX, minY] = deconvert(-hitboxes.graph[2] * (controls.view.zoom - 1)/(controls.view.zoom), -hitboxes.graph[3] * (controls.view.zoom - 1)/(controls.view.zoom));

    if (controls.view.x < minX) {
        controls.view.x = minX
    }
    else if (controls.view.x > maxX) {
        controls.view.x = maxX;
    }
    else if(controls.view.y < minY) {
        controls.view.y = minY
    }
    else if (controls.view.y > maxY) {
        controls.view.y = maxY;
    }
    return;
    
}
class Controls {
    static move(controls) {
      function mousePressed(e) {
        controls.viewPos.isDragging = true;
        controls.viewPos.prevX = e.clientX;
        controls.viewPos.prevY = e.clientY;
      }
  
      function mouseDragged(e) {
        const {prevX, prevY, isDragging} = controls.viewPos;
        if(!isDragging) return;
        const pos = {x: e.clientX, y: e.clientY};
        const dx = pos.x - prevX;
        const dy = pos.y - prevY;        
        if(prevX || prevY) {  
            controls.view.x += dx;
            controls.view.y += dy;
            clampTranslations();
            controls.viewPos.prevX = pos.x, controls.viewPos.prevY = pos.y
        }
      }
  
      function mouseReleased(e) {
        controls.viewPos.isDragging = false;
        controls.viewPos.prevX = null;
        controls.viewPos.prevY = null;
      }
   
      return {
        mousePressed, 
        mouseDragged, 
        mouseReleased
      }
    }
  
    static zoom(controls) {
      function worldZoom(e) {
        let {x, y, deltaY} = e;
        const factor = 0.003;
        let zoom = -1 * deltaY * factor;
        if (controls.view.zoom + zoom > settings.maxZoom) {
            zoom = settings.maxZoom - controls.view.zoom;
        }
        if (controls.view.zoom + zoom < settings.minZoom) {
            zoom = settings.minZoom - controls.view.zoom;
        }
  
        
        x = x - document.getElementById('info-canvas').getBoundingClientRect().left;
        y = y - document.getElementById('info-canvas').getBoundingClientRect().top;
        
        const wx = (x-controls.view.x)/(controls.view.zoom);
        const wy = (y-controls.view.y)/(controls.view.zoom);

        
        controls.view.x -= wx*zoom;
        controls.view.y -= wy*zoom;
        clampTranslations();
        controls.view.zoom += zoom;
        
      }
  
      return {worldZoom}
    }
}

function updateInfo(i) {
    if (!dataLoaded) return;
    if (!colors.length) return;
    values = DATA[i].cam_data.map(x => cmap((x - DATA[i].min_temp) / (DATA[i].max_temp - DATA[i].min_temp)));
    // values = DATA[i].cam_data.map(x => cmap(clamp((x - (10)) / (20 - 10), 0, 1)));
    // console.log(values)
    frameSlider.value(index);
    //incrementFrame();
    // document.getElementById("cube-video").currentTime = (694 + 1*(3600/1.45061852762));// 1080 + (index - 5928) / 1.5;
}

// function incrementFrame() {
//     document.getElementById("cube-video").currentTime = (694 + ((index - 988)/2827)*(3600/1.45061852762));
// }

window.onrecieve = (data) => {
    if (typeof data.data === 'string') {
        if (data.data === "SEANJAIDEN") {
            document.getElementById('title').innerHTML = "Midnight Balloon Data - Sean Kuwamoto & Jaiden Grimminck";
            dataInfo.frame.range = [0, 10700];
            // dataInfo.frame.range = [5400, 6500];
            dataInfo.altitude.range = [0, 12000];
            return;
        }
        if (data.data === "KAIEVAN") {
            document.getElementById('title').innerHTML = "Dusk Balloon Data - Kai Spada & Evan Kuo";
            dataInfo.time.range = ["15:37:13:0", "20:11:52:0"]
            watermark = true;
            return;
        }
        if (data.data === "BAKER") {
            document.getElementById('title').innerHTML = "Dawn Balloon Data - Baker Simmons";
            dataInfo.time.range  = ["13:37:00:0", "22:35:00:0"]
            watermark = true;
            return;
        }
        console.error("You done messed up");
        return;
    }
    if (typeof data.data[0] === 'string') {
        colors.push(data.data);
        return;
    }
    if (data.data.includes(null) || data.data.includes(undefined)) return;
    console.log(data.data[0]);
    DATA = data.data;
    if (settings.skipGPSFrames) {
        DATA = DATA.filter(d => d.live_cam == 1);
    }
    document.getElementById('frame-slider').max = DATA.length - 1;
    updateInfo(index);
    document.getElementById('frame-slider').addEventListener('mousedown', () => {   
        sliderClicked = true;
        playPause = false;
    }); 
    document.getElementById('frame-slider').addEventListener('mouseup', () => {
        sliderClicked = false;
    });

    document.getElementById('loader').style.visibility = "hidden";
    document.getElementById('main').style.visibility = "visible";

    dataLoaded = true;
}

function lerp(dataType, time) {
    if (!dataLoaded) return 0;
    if (dataType === "time") {
        // console.log(UTC(milis(dataInfo[dataType].range[0]) + time * (milis(dataInfo[dataType].range[1]) - milis(dataInfo[dataType].range[0]))))
        return UTC(milis(dataInfo[dataType].range[0]) + time * (milis(dataInfo[dataType].range[1]) - milis(dataInfo[dataType].range[0])));
    }
    return dataInfo[dataType].range[0] + time * (dataInfo[dataType].range[1] - dataInfo[dataType].range[0]);
}
function inverseLerp(dataType, frame, line) {
    if (!dataLoaded) return 0;
    if (dataType === "time") {
        return Math.min(Math.max((milis(DATA[frame][dataType]) - milis(dataInfo[dataType].range[0]))/(milis(dataInfo[dataType].range[1]) - milis(dataInfo[dataType].range[0])), 0), 1);
    }
    let dataPiece;
    if (dataType === "frame") {
        dataPiece = frame - dataInfo[dataType].range[0];
    }
    else {
        dataPiece = DATA[frame][dataInfo[dataType].data[line % dataInfo[dataType].data.length]] - dataInfo[dataType].range[0];
    }
    return Math.min(Math.max(dataPiece/(dataInfo[dataType].range[1] - dataInfo[dataType].range[0]), 0), 1);
}

function detectHover(sketch, hitbox) {
    return sketch.mouseX > hitbox[0] && sketch.mouseX < hitbox[0] + hitbox[2] && sketch.mouseY > hitbox[1] && sketch.mouseY < hitbox[1] + hitbox[3];
}

function milis(UTC) {
    // replaces the last colon with a period
    UTC = UTC.split(":")[0] + ":" + UTC.split(":")[1].padStart(2, '0') + ":" + UTC.split(":")[2].padStart(2, '0') + "." + UTC.split(":")[3].padStart(3, '0');
    return new Date("2023-06-07T" + UTC + "Z").getTime();
}

function UTC(milis) {
    let date = new Date(milis);
    return date.getUTCHours() + ":" + date.getUTCMinutes() + ":" + date.getUTCSeconds() + ":" + date.getUTCMilliseconds();
}

function mangetometerToStuff(x, y, z) {

    let headingRadians = Math.atan2(y, x);
    let headingDegrees = headingRadians * 180 / Math.PI;
    let declinationAngle = 11.41666666666667;
    headingDegrees += declinationAngle;
    if (headingDegrees < 0) {
      headingDegrees += 360;
    }
  
    // console.log("Heading: " + headingDegrees + " " + cardinal);
    // return headingDegrees * (Math.PI / 180);
}
