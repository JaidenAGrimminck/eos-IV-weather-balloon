const express = require('express');
const app = express();
const http = require('http');
const { send } = require('process');
const server = http.createServer(app);
const { Server } = require("socket.io");
const { SerialPort } = require('serialport');
const fs = require('fs');
let dat = fs.readFileSync('SKJGDT.csv',
    { encoding: 'utf8', flag: 'r' });
const DATA = dat.split('\n').slice(3).map(x => x.split(',').slice(22).join(','));
// console.log(DATA);
const port = new SerialPort({
    path: "/dev/cu.usbmodem1201",
    baudRate: 9600
});
const colormap = require('colormap');
const io = new Server(server);

const doubleLines = true;
// inferno colormap function from 0 to 1
let inferno = (x) => {
    const inferno = colormap({
        colormap: 'jet',
        nshades: 256,
        format: 'hex',
        alpha: 1
    });
    return inferno[Math.floor(x * 255)];
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

io.on('connection', (socket) => {
    console.log('a user connected');
    socket.on('disconnect', () => {
        console.log('user disconnected');
    });
});

server.listen(3000, () => {
  console.log('listening on *:3000');
});

function sendData(data) {
    io.emit('dt', { data });
}

readArduino();



function readArduino() {
    let stream = "";
    let count = 0;
    port.on('readable', function () {
        let data = port.read().toString();
        stream += data;
        if (data.includes('\n')) {
            count+=1;
            computeThenSend(stream);
            stream = "";
        }
    });
}


function computeThenSend(stream_, liveMode=true) {
    // console.log('computing');
    const stream = stream_ + "";
    let values = [];
    if (liveMode && !stream.includes(')')) return;
    let cameraData;
    if (liveMode) {
        cameraData = stream.split(')')[1];
    }
    else {
        cameraData = stream;
    }
    const cameraArray = cameraData.split(',');
    cameraArray.pop();
    const WIDTH = 32;
    const HEIGHT = 24;
    const max = Math.max(...cameraArray);
    const min = Math.min(...cameraArray);


    for (let i = 0; i < HEIGHT; i++) {
        for (let j = 0; j < WIDTH; j++) {
            let index = i * WIDTH + j;
            let value;
            value = cameraArray[index];
            let relative = (value - min) / (max - min);
            values.push(relative);
        }
    }

    values = values.map(v => {
        return inferno(v);
    });


    sendData(values);
}

/*
const WIDTH = 32;
    const HEIGHT = 24;

    function printStream(stream) {
        streamArray = stream.split(',');
        streamArray.pop();
        streamArray = streamArray.map((val) => parseInt(val));
        // max val of stream array
        const max = Math.max(...streamArray);
        // min val of stream array
        const min = Math.min(...streamArray);
            

        for (let i = 0; i < HEIGHT; i++) {
            let row = "";
            for (let j = 0; j < WIDTH; j++) {
                let index = i * WIDTH + j;
                let value = streamArray[index];
                // Find value relative to max and min
                let relative = (value - min) / (max - min);
            }
            console.log(row);
        }
    }
    */