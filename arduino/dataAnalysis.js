// Number of data rows to use. 0 means all.
const NUM_ROWS = 0;

// How many samples for the average upward speed
const UPWARD_SPEED_SAMPLES = 100;

/*
Whose data to use.
SEANJAIDEN: Sean and Jaiden's data -- Midnight balloon
KAIEVAN: Kaievan's data -- Dusk balloon
BAKER: Baker's data -- Dawn balloon
*/
const MODE = "BAKER";

// Libraries
const express = require('express');
const app = express();
const http = require('http');
const server = http.createServer(app);
const { Server } = require("socket.io");
const fs = require('fs');
const colormap = require('colormap');
// inferno colormap function from 0 to 1
const colors = colormap({
    colormap: 'inferno',
    nshades: 256,
    format: 'hex',
    alpha: 1
});
const extraColors = colormap({
    colormap: 'jet',
    nshades: 256,
    format: 'hex',
    alpha: 1
})

// Socket.io
const io = new Server(server);
function sendData(data) {
    io.emit('dt', { data });
}
app.get('/', (req, res) => {
  res.sendFile(__dirname + '/public/index.html');
});
app.get('/script.js', (req, res) => {
    res.sendFile(__dirname + '/public/script.js');
});
app.get('/styles.css', (req, res) => {
    res.sendFile(__dirname + '/public/styles.css');
});
app.get('/camera.mp4', (req, res) => {
    res.sendFile(__dirname + '/public/camera.mp4');
});
io.on('connection', (socket) => {
    console.log('a user connected');
    sendData(MODE);
    sendData(data);
    sendData(colors);
    sendData(extraColors);
    socket.on('disconnect', () => {
        console.log('user disconnected');
    });
});
server.listen(3000, () => {
    console.log('listening on *:3000');
});

// Reading data


// Processing data
function getData() {
    if (MODE === "SEANJAIDEN") {
        let dat = fs.readFileSync('MIDNIGHT.csv', { encoding: 'utf8', flag: 'r' });
        let e = dat.split('\r\n').slice(3);
        if (NUM_ROWS == 0) return e.slice(0, e.length - 1);
        else return e.slice(0, NUM_ROWS);
    }
    else if (MODE === "KAIEVAN") {
        let dat = fs.readFileSync('DUSK_CORRECTED.csv', { encoding: 'utf8', flag: 'r' });
        let e = dat.split('\r\n').slice(2);
        if (NUM_ROWS == 0) return e.slice(0, e.length - 1);
        else return e.slice(0, NUM_ROWS);
    }
    else if (MODE === "BAKER") {
        let dat = fs.readFileSync('DAWN.csv', { encoding: 'utf8', flag: 'r' });
        let e = dat.split('\r\n').slice(3);
        if (NUM_ROWS == 0) return e.slice(0, e.length - 1);
        else return e.slice(0, NUM_ROWS);
    }
    
}
const DATA = getData();

class dataStruct {
    constructor(date="6/7/23" /* data correction */, time="0:0:0:0", ms_since_last_cycle=0, fixed=false, latitude=0, longitude=0, altitude=0, speed=0, angle=0, satellites=0, avg_thermistor=0, thermistor_c=0, gyro_x=0, gyro_y=0, gyro_z=0, accel_x=0, accel_y=0, accel_z=0, mag_x=0, mag_y=0, mag_z=0, live_cam=true, cam_data=[]) {
        this.date = date;
        this.time = time;
        this.ms_since_last_cycle = ms_since_last_cycle;
        this.fixed = fixed;
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = altitude;
        this.speed = speed;
        this.angle = angle;
        this.satellites = satellites;
        this.avg_thermistor = avg_thermistor;
        this.thermistor_c = thermistor_c;
        this.gyro_x = gyro_x;
        this.gyro_y = gyro_y;
        this.gyro_z = gyro_z;
        this.accel_x = accel_x;
        this.accel_y = accel_y;
        this.accel_z = accel_z;
        this.mag_x = mag_x;
        this.mag_y = mag_y;
        this.mag_z = mag_z;
        this.live_cam = live_cam;
        this.cam_data = cam_data;
        this.max_temp;
        this.min_temp;
        this.humidity = 0;
        this.pressure = 0;
        this.upward_speed = 0;
        this.avg_upward_speed = 0;
    }
}
let data = [];
let mostRecentCameraData = new Array(768).fill(0);
mostRecentCameraData[0] = 1;
for (let i = 0; i < DATA.length; i++) {
    const row = DATA[i];
    let dataObj;

    if (MODE === "SEANJAIDEN") {
        rowItems = row.split(',').map((x, i) => {
            // Date and time
            if (i == 0 || i == 1) return String(x);
            // fixed and live cam
            if (i == 3 ||  i == 21) return Boolean(+x);
            // The rest are just numbers
            else return parseFloat(x);
        });
        dataObj = new dataStruct(...rowItems);
        let camData = row.split(',').slice(22);
        camData.pop();
        dataObj.cam_data = camData;
        
    
        // Data correction
        dataObj.live_cam = (i % 30 < 24);
    }
    else if (MODE === "KAIEVAN") {
        rowItems = row.split(',');
        dataObj = new dataStruct();
        dataObj.time = rowItems[0] + ":000";
        dataObj.latitude = rowItems[1];
        dataObj.longitude = rowItems[2];
        dataObj.altitude = rowItems[3];
        dataObj.speed = rowItems[4];
        dataObj.angle = rowItems[5];
        dataObj.satellites = rowItems[6];
        dataObj.avg_thermistor = rowItems[7];
        dataObj.thermistor_c = rowItems[8];
        dataObj.pressure = rowItems[10];
        dataObj.humidity = rowItems[11];
        let cameraVals = rowItems.slice(13).map(parseFloat).slice(0, 768);
        if (isNaN(cameraVals[0])) {
            dataObj.cam_data = mostRecentCameraData;
        }
        else {
            dataObj.cam_data = cameraVals;
            mostRecentCameraData = cameraVals;
        }
    }
    else if (MODE === "BAKER") {
        rowItems = row.split(',').map((x, i) => {
            // Date and time
            if (i == 0 || i == 1) return String(x);
            // fixed and live cam
            if (i == 3 ||  i == 19) return Boolean(+x);
            // The rest are just numbers
            else return parseFloat(x);
        });
        dataObj = new dataStruct();
        dataObj.date = rowItems[0];
        dataObj.time = rowItems[1];
        dataObj.ms_since_last_cycle = rowItems[2];
        dataObj.fixed = rowItems[3];
        dataObj.latitude = rowItems[4];
        dataObj.longitude = rowItems[5];
        dataObj.altitude = rowItems[6];
        dataObj.speed = rowItems[7];
        dataObj.angle = rowItems[8];
        dataObj.satellites = rowItems[9];
        dataObj.gyro_x = rowItems[10];
        dataObj.gyro_y = rowItems[11];
        dataObj.gyro_z = rowItems[12];
        dataObj.accel_x = rowItems[13];
        dataObj.accel_y = rowItems[14];
        dataObj.accel_z = rowItems[15];
        dataObj.mag_x = rowItems[16];
        dataObj.mag_y = rowItems[17];
        dataObj.mag_z = rowItems[18];
        dataObj.live_cam = rowItems[19];
        dataObj.cam_data = rowItems.slice(20);
        dataObj.cam_data.pop();

        for (let i = 0; i < dataObj.cam_data.length; i++) {
            let pixel = dataObj.cam_data[i];
            if (isNaN(pixel) || pixel === undefined || pixel === null) {
                dataObj.cam_data[i] = mostRecentCameraData[i];
            }
            else {
                mostRecentCameraData[i] = pixel;
            }
        }
    }

    dataObj.max_temp = Math.max(...dataObj.cam_data);
    dataObj.min_temp = Math.min(...dataObj.cam_data);
    if (i > 0) {
        dataObj.upward_speed = (dataObj.altitude - data[i-1].altitude);
    }
    let samples = [];
    for (let j = -UPWARD_SPEED_SAMPLES; j < 0; j++) {
        if (i + j < 0) continue;
        let sample = data[i + j];
        if (sample) {
            samples.push(sample.upward_speed);
        }
    }
    if (samples.length > 0) {
        dataObj.avg_upward_speed = samples.reduce((a, b) => a + b, 0) / samples.length;
    }
    data.push(dataObj);
    // It takes a WHILE to convert the image data to colors.
    if (i % 24 == 0) console.log("Loading: " + Math.round(i / DATA.length * 1000) / 10 + "%");
}


