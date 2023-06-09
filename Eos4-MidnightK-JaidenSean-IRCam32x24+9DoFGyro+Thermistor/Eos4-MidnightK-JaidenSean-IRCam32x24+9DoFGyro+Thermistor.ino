/**

-- Overview --

This code is originally made by Jaiden Grimminck and Sean Kuwamoto, ©2023.

This was made as a part of the Atmospheric Science Immersive II at the Bay School of San Francisco.

This includes code for a 9 DoF gyroscope, accelerometer, and magnometer, an IR camera, and a thermistor.

-- Customization --

Note: Use CMD/CTRL + F to find the location of the sections.

Do the search in the format of:

    * Section Name *

to find the section.


Change following pins in the "Pins" section:
    - STATUS_PIN: Status Light Pin
    - THERMISTOR_PIN : Thermistor Pin

Change the following settings in the "Settings" section:
    - runGPS : boolean : Get information from the GPS and print it? Yes (true, [default]), No (false)
    - readable : boolean : Should the serial and SD card be human readable, or in a CSV format? Yes (true), No (false, [default])
    - useSeperateCameraFile : boolean : Should the camera data be stored in a seperate file? Yes (true), No (false, [default])
    - restartOnErrors : boolean : Restart on an initialization error after a few seconds. Recommended for flight.
    - ignoreGPSError : boolean : Ignores any parsing issues with the GPS, and continues on in the loop.
    - fileName : String : The name of the main data file. Name must be less or equal to 8 characters.
    - cameraFile : String : The name of the camera file, if useSeperateCameraFile is enabled. Name must be less or equal to 8 characters.
    - serialOutput : byte : What information goes to the serial. Options are stated in a comment above the setting. Initialization and error messages are exempt from this setting.
    - SWITCH_TO_STILL : integer : How many frames of live camera there are until switching to the camera being still.
    - SWITCH_TO_LIVE : integer : How many frames of still camera there are until switching to the camera being live.

-- Status/Light Codes --

Light generally stays off except for a quick blink approximately every second: Working as intended.
    - Make sure that the SD card is collecting data.
    - Check serial monitor to make sure that the data is correct, and no SD issues occured.

Light stays solid for about a second during the main loop: Issue with saving data.
    - Is the SD card fully in?
    - Check the files... has the datalog file been removed?
    - Do you need to reformat the SD card?

Blink every half second, then stops for a second and a half: SD Card initialization issue.
    - Is the SD Card in the flight computer?
    - Is the SD Card fully in?
    - Is there an issue with the SD card itself?

Blink 3 times, then wait a second while off: Gyroscope initialization issue.
Blink 3 times, then wait a second while on: Magaccel initialization issue.
    - Are the wires going to the correct ports?
    - Are the wires all the way in?
    - Is the module itself broken?

Rapidly blink (10 times a second): IR Camera issue
    - Is the camera wired correctly?
    - Are the wires flipped?
    - Is there an issue with the plug in?
*/


/* Imports */

#include <Adafruit_GPS.h>

#include <SPI.h>
#include <SD.h>

#include <Wire.h>

// Gyroscope
#include <Adafruit_FXAS21002C.h>

// Magnometer/Accelerometer
#include <Adafruit_FXOS8700.h>

// Camera
#include <Adafruit_MLX90640.h>

// Just general adafruit sensor stuff
#include <Adafruit_Sensor.h>

/* Settings */

const bool runGPS = true; //Recommended: true
const bool readable = true; //Recommended: false (for flight)
const bool useSeperateCameraFile = false; //Recomended: false (unless you can parse both)

//These two settings were added after the flight
const bool restartOnErrors = true; //Recommended: true
const bool ignoreGPSError = false; //Recommended: false

const String fileName = "datalog.csv"; //Rename this file to whatever you want. Name must be less than or equal to 8 characters.
const String cameraFile = "camera.txt"; //Rename this file to whatever you want. Rule above applies. Only used if useSeperateCameraFile is enabled.

// 0 is for all messages, 1 is only info messages (gps/gyro/accelmag), 2 is only camera data.
const byte serialOutput = 0;

// How many live frames there are. When the counter reaches the number below, it will change the camera from live -> still.
#define SWITCH_TO_STILL 24
// How many still frames there are. When the counter reaches this number plus the number above, it will change the camera from still -> live and reset the counter.
#define SWITCH_TO_LIVE 6

/* Pins */

//The status LED pin. Mirrors the built in LED.
#define STATUS_PIN 12

// Set equal to 63 to tell the code there is no thermistor.
const byte THERMISTOR_PIN = 0;

/* Components */

// GPS Object

#define GPSSerial Serial1
Adafruit_GPS GPS(&GPSSerial);

// Gyroscope

Adafruit_FXAS21002C gyro = Adafruit_FXAS21002C(0x0021002C);
Adafruit_FXOS8700 accelmag = Adafruit_FXOS8700(0x8700A, 0x8700B);

// IR Camera

Adafruit_MLX90640 mlx;

/* Variables */

// Definitions

#define AVG_READING 5
#define SERIAL_TIMEOUT 3000
#define SEPERATOR ","
#define thermR 9800
#define resisR 9700

// SD Card Selection

const byte chipSelect = 4;

// Timer

unsigned long timer = millis();

// Empty Memory Addresses

double value; // Used for sensor readings
double *reading; // Used for long sensor readings

float frame[32*24]; // Buffer for full frame of temperatures

String output; // General output string
String timeStr; // String containing the time

File dataFile; // The main data file
File camFile; // The main camera file

int counter = 0;

int errorCounter = 0;

// Flags/Triggers

bool cardError = false; // Triggers if there's an error with the SD card
bool gyroscopeError = false; // Triggers if there's an error with the gyroscope
bool accelmagError = false; // Triggers if there's an error with the accelmag
bool irCamError = false; // Triggers if there's an error with the IR camera.

bool firstFrame = true; // Flag for the first loop() run. Disables itself at the end of loop().

/* Functions */

void(* resetFunc) (void) = 0; //declare reset function @ address 0

void setup() {
    // Setup the LED
    pinMode(LED_BUILTIN, OUTPUT);
    pinMode(STATUS_PIN, OUTPUT);

    // Start the GPS
    GPS.begin(9600);
    GPS.sendCommand(PMTK_SET_NMEA_OUTPUT_RMCGGA);
    GPS.sendCommand(PMTK_SET_NMEA_UPDATE_1HZ);

    // Begin the serial
    Serial.begin(9600);

    // Wait till Serial is enabled, or wait until the serial timeout is done since the flight computer might not be connected to serial.
    while (!Serial) {
        delay(100);
        if (millis() - timer > SERIAL_TIMEOUT) {
            break;
        }
    }

    Serial.println("Serial enabled.");

    // Enable the gyroscope
    if (!gyro.begin()) {
        Serial.println("There was an issue with the gyroscope.");

        gyroscopeError = true;

        return;
    }

    Serial.println("Gyroscope successfully initialized.");

    // Enable the accelmag
    if (!accelmag.begin()) {
        Serial.println("There was an issue with the accelerometer.");

        accelmagError = true;

        return;
    }

    Serial.println("Accelerometer successfully initialized.");

    //Enable the IR Camera
    if (!mlx.begin(MLX90640_I2CADDR_DEFAULT, &Wire)) {
        Serial.println("There was an issue with the IR camera.");

        irCamError = true;

        return;
    }

    Serial.println("IR Camera successfully initialized.");

    // Print out the IR camera serial number to confirm
    Serial.print("IR Camera Serial number: ");
    Serial.print(mlx.serialNumber[0], HEX);
    Serial.print(mlx.serialNumber[1], HEX);
    Serial.println(mlx.serialNumber[2], HEX);

    // Customize the camera settings
    mlx.setMode(MLX90640_INTERLEAVED);
    mlx.setResolution(MLX90640_ADC_18BIT);
    mlx.setRefreshRate(MLX90640_2_HZ);

    Serial.println("Successfully configured the IR Camera.");

    // Small Delay
    delay(500);

    // Check if the SD Card is avaliable.
    if (!SD.begin(chipSelect)) {
        // If there is NOT an SD card, we'll enable the cardError flag.
        Serial.println("There was an issue with the SD card.");
        cardError = true;
    } else {
        // If the SD card is avaliable, we'll successfully run the program.
        Serial.println("Card has been successfully initialized!");

        Serial.println("Successfully initialized program. Made by Sean Kuwamoto and Jaiden Grimminck.");
    }
}

/**
    * Turn the LED on or off.
    * @param on Whether the built in LED should be on or off.
*/
void led(bool on) {
    digitalWrite(LED_BUILTIN, on ? HIGH : LOW);
    if (LED_BUILTIN != STATUS_PIN) {
        digitalWrite(STATUS_PIN, on ? HIGH : LOW);
    }
}

/**
    * Do a read of a pin using the AVG_READING definition.
    * @param pin The pin of the sensor.
    * @return A list of the readings.
*/
double * MultiRead(byte pin) {
    static double cnt[AVG_READING];
    for (int i = 0; i < AVG_READING; i++) {
        cnt[i] = analogRead(pin);
        delay(10);
    }
    return cnt;
}

/**
    * Returns the length of an double[] array. Easier to think about coming from a non-C++ background.
    * @param array The double[] array in question.
    * @return The length of the double[] array.
*/
int len(double* array) {
    return sizeof array;
}

/**
    * Returns the average value of a double[] array.
    * @param values The list of values in question.
    * @return The average value of the values.
*/
double Avg(double* values) {
    double value = 0;

    for (int i = 0; i < len(values); i++) {
        value += values[i];
    }

    return value / len(values);
}

/**
    * Converts the raw thermistor value to celcius.
    * @param value The raw thermistor value.
    * @return The converted celcius value.
*/
double thermistorToC(double value) {
    double innerLog = (-resisR * value) / ((thermR * value) - (1023 * thermR));
    return (3950 / (log( innerLog ) + 13.2484)) - 273.15;
}

/**
    * This was added AFTER the flight. Restart the program if there is an issue with a sensor (SD, etc).
*/
void errorTick() {
    if (!restartOnErrors) return;

    errorCounter += 1;
    if (errorCounter >= 3) {
        Serial.println("Restarting...");
        resetFunc();
    }
}

/**
    * Loop is the main loop of the program. Contains all of the main code.
*/
void loop() {
    /* Error Catches */
    
    // If there's a SD card error, turn the LED on and off instead of running the rest of the code.
    if (cardError) {
        led(true);
        delay(500);
        Serial.println("Please plug in the SD card and restart the program.");
        led(false);
        delay(300);
        led(true);
        delay(500);
        led(false);
        delay(1500);
        errorTick();
        return;
    }

    // If there's a gyroscope error, turn the LED on and off then pause instead of running the rest of the code.
    if (gyroscopeError) {
        for (int i = 0; i < 3; i++) {
            led(true);
            delay(50);
            led(false);
            delay(283);
        }
        Serial.println("Please check the gyroscope.");
        delay(1000);
        errorTick();
        return;
    }
    
    // If there's an accelmag error, turn the LED on and off then pause instead of running the rest of the code.
    if (accelmagError) {
        for (int i = 0; i < 3; i++) {
            led(true);
            delay(50);
            led(false);
            delay(283);
        }
        Serial.println("Please check the accelmag.");
        led(true);
        delay(950);
        led(false);
        delay(50);
        errorTick();
        return;
    }

    // If there's an IR camera error, constantly blink the camera.
    if (irCamError) {
        Serial.println("Please check the IR Camera.");
        led(true);
        delay(100);
        led(false);
        delay(100);
        errorTick();
        return;
    }

    // If the runGPS setting is enabled
    if (runGPS) {
        // Read the GPS.
        char c = GPS.read();

        // If a new NMEA was recieved, but unable to parse the new NMEA, just return only if we don't ignore the GPS error.
        if (GPS.newNMEAreceived()) {
            if (!GPS.parse(GPS.lastNMEA()) && !ignoreGPSError) {
                return;
            }
        }
    }

    // Every second, do this loop.
    if (millis() - timer < 1000) return;

    // We'll record the difference in order to get precision on the time.
    long difference = millis() - timer;

    // Update the timer.
    timer = millis();

    // Clear the output
    output = "";

    // If the runGPS option is enabled.
    if (runGPS) { 
        timeStr = "";

        timeStr += readable ? "Cycle: " : "";

        // First, we'll output the date.
        timeStr += GPS.month;
        timeStr += "/";
        timeStr += GPS.day;
        timeStr += "/";
        timeStr += GPS.year;

        timeStr += readable ? " at " : SEPERATOR;

        // Next, we'll print out the time.
        timeStr += GPS.hour;
        timeStr += ":";
        timeStr += GPS.minute;
        timeStr += ":";
        timeStr += GPS.seconds;
        timeStr += ":";
        timeStr += GPS.milliseconds;

        timeStr += readable ? "\n" : SEPERATOR;

        // Since the time isn't always reliable, we'll also print out the difference since the last cycle.
        timeStr += readable ? "Time since last cycle: " : "";
        timeStr += difference;
        timeStr += readable ? "\n" : SEPERATOR;

        output += timeStr;

        // Next, if the GPS is fixed, we'll print out the location information.
        // If not, we'll just output empty data.
        if (GPS.fix) {
            output += readable ? "GPS is: Fixed.\n" : "1";

            output += !readable ? SEPERATOR : "";

            output += readable ? "Lat/Long/Alt: " : "";
            output += GPS.latitude;
            output += ",";
            output += GPS.longitude;
            output += ",";
            output += GPS.altitude;

            output += readable ? "\nSpeed/Ang/Satellites: " : SEPERATOR;

            output += GPS.speed;
            output += ",";
            output += GPS.angle;
            output += ",";
            output += GPS.satellites;
        } else {
            output += readable ? "GPS is: Not Fixed.\n" : "0";

            output += !readable ? SEPERATOR : "";

            output += readable ? "Lat/Long/Alt: " : "";
            output += "0,0,0";

            output += readable ? "\nSpeed/Ang/Satellites: " : SEPERATOR;

            output += "0,0,";
            output += GPS.satellites;
        }

        // Finally, we'll end the GPS data.
        output += readable ? "\n" : SEPERATOR;
    }

    // Turn on the built in LED.
    led(true);   

    /* -- Data Collection -- */

    // Thermistor

    if (THERMISTOR_PIN != 63) {
        double averageTemperature = analogRead(THERMISTOR_PIN);

        output += readable ? "Thermistor Temperature (RAW/C): " : "";
        output += averageTemperature;
        output += readable ? ", " : ",";
        output += thermistorToC(averageTemperature);
        output += readable ? "\n" : SEPERATOR;
    }

    // Gyroscope

    sensors_event_t gevent;
    gyro.getEvent(&gevent);

    output += readable ? "Gyroscope (m/s) \nX: " : "";
    output += gevent.gyro.x;
    output += readable ? "\nY: " : ",";
    output += gevent.gyro.y;
    output += readable ? "\nZ: " : ",";
    output += gevent.gyro.z;

    output += readable ? "\n" : SEPERATOR;

    // Accelerometer and Magnet

    sensors_event_t aevent, mevent;
    accelmag.getEvent(&aevent, &mevent);

    output += readable ? "Accelerometer (m/s^2):\nX: " : "";
    output += aevent.acceleration.x;
    output += readable ? "\nY: " : ",";
    output += aevent.acceleration.y;
    output += readable ? "\nZ: " : ",";
    output += aevent.acceleration.z;

    output += readable ? "\n" : SEPERATOR;

    output += readable ? "Magnometer (uT):\n X: " : "";
    output += mevent.magnetic.x;
    output += readable ? "\nY: " : ",";
    output += mevent.magnetic.y;
    output += readable ? "\nZ: " : ",";
    output += mevent.magnetic.z;
    output += readable ? "\n" : SEPERATOR;

    // Small delay (actually prob not needed)
    delay(20);

    /* -- Saving to SD Card -- */

    // We'll open the SD card first frame in order to create the file.
    if (firstFrame) {
        if (SD.exists(fileName)) {
            Serial.println("File already exists! No need to create file.");
        } else {
            Serial.println("File will be created!");
        }
        dataFile = SD.open(fileName, FILE_WRITE);
        dataFile.close();
        Serial.println("File should be guarenteed to be created! (If it didn't exist beforehand.)");
    }

    // If the file does exist, save the data and print it. If not, send an error message.
    // We don't want to enable the flag though, just in case it's a freak error and can be re-enabled.
    if (SD.exists(fileName)) {
        dataFile = SD.open(fileName, FILE_WRITE);

        if (dataFile) {
            led(false);

            // Print the headers on the first frame
            if (firstFrame) {
                dataFile.println("BEGIN DATA COLLECTION");
                dataFile.println("-- JAIDEN GRIMMINCK | SEAN KUWAMOTO --");
                //Print GPS headers if we're using the GPS.
                if (runGPS) {
                    dataFile.print("date,time,ms_since_last_cycle,fixed,latitude,longitude,altitude,speed,angle,satellites,");
                }
                //Only print the thermistor header if we're using it.
                if (THERMISTOR_PIN != 63) {
                    dataFile.print("avg_thermistor,thermistor_c,");
                }
                dataFile.print("gyro_x,gyro_y,gyro_z,accel_x,accel_y,accel_z,mag_x,mag_y,mag_z,live_cam,cam_data");

                dataFile.println();
            }
            
            dataFile.print(output);
            if (readable || useSeperateCameraFile) {
                dataFile.println(readable ? "\n" : "\n\n");
            }

            dataFile.close();
        } else {
            Serial.println("Save error! File doesn't exist?");
            led(true);
        }
        if (serialOutput == 1 || serialOutput == 0) {
            Serial.println(output);
        }
    } else {
        Serial.println("Save error! File has not been created?");
        led(true);
    }

    /* -- IR Camera Collection -- */

    // If we're using a seperate camera file
    if (useSeperateCameraFile) {
        // Create a file on the first frame.
        if (firstFrame) {
            // Small info string for serial, checking if the file exists.
            if (SD.exists(cameraFile)) {
                Serial.println("Camera file already exists! No need to create camera file.");
            } else {
                Serial.println("Camera file does not exist. File will be created!");
            }

            // Create the file.
            camFile = SD.open(cameraFile, FILE_WRITE);
            camFile.close();

            Serial.println("Camera file has been created!");

            // Write an extra string to the file, as a line break.
            camFile = SD.open(cameraFile, FILE_WRITE);
            
            if (camFile) {
                camFile.println("\n---- NEW STREAM ----");
            } else {
                Serial.println("Something went wrong, the cam file wasn't created?");
            }
            

            camFile.close();
        }
    }

    // Open the file again.
    camFile = SD.open(useSeperateCameraFile ? cameraFile : fileName, FILE_WRITE);
    
    // If the file 
    if (camFile) {
        if (useSeperateCameraFile) {
            camFile.print("(" + timeStr + ")");
        }

        if (serialOutput == 2 || serialOutput == 0) {
            Serial.print("(" + timeStr + ")");
        }

        timeStr = "";

        int mlxStatus = counter < SWITCH_TO_STILL ? mlx.getFrame(frame) : -1;
        
        // Print whether the camera is live or not.
        camFile.print(counter < SWITCH_TO_STILL ? "1" : "0");
        camFile.print(SEPERATOR);

        // Check if there's a camera frame.
        if (mlxStatus != 0 && counter < SWITCH_TO_STILL) {
            // If not, print the error to the file.
            Serial.println("Camera frame failed!");
            camFile.println("CAM_ERR");         
        } else {
            // If so, loop through it and save to the file.
            for (uint8_t h = 0; h < 24; h++) { // Height of 24 pixels
                for (uint8_t w = 0; w < 32; w++) { // Width of 32 pixels
                    float t = frame[h*32 + w];
                    
                    camFile.print(t);
                    camFile.print(",");

                    if (serialOutput == 2 || serialOutput == 0) {
                        Serial.print(t);
                        Serial.print(",");
                    }
                }
            }
        }
        if (serialOutput == 2 || serialOutput == 0) {
            Serial.println();
        }
        camFile.println();
    } else {
        Serial.println("Error: The camera file didn't exist!");
    }

    counter++;
    
    if (counter == SWITCH_TO_STILL) {
        Serial.println("Switched to still.");
    }

    if (counter >= SWITCH_TO_STILL + SWITCH_TO_LIVE) {
        counter = 0;
        Serial.println("Switched back to live!");
    }

    camFile.close();

    if (firstFrame) firstFrame = false;
}
