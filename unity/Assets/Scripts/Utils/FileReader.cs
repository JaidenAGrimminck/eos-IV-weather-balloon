using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileReader : MonoBehaviour
{
    public static FileReader instance;

    //allow the user to select a file via the inspector
    public TextAsset file;

    public TextAsset aprs;

    List<frame> frames;

    private static List<Action> onload = new List<Action>();

    private static bool loaded = false;

    public static void awaitForLoad(Action loadaction) {
        if (loaded) {
            loadaction();

            return;
        }
        
        onload.Add(loadaction);
    }

    // Start is called before the first frame update
    void Start() {
        instance = this;

        readAPRS();

        //read the text file
        string text = file.text;

        //split the text file into lines
        string[] lines = text.Split('\n');

        //create a list of frames
        frames = new List<frame>();

        string header = "";

        //loop through each line
        foreach (string line in lines) {
            if (!line.Contains(',')) continue;

            //split the line into values
            string[] values = line.Split(',');

            if (values[0] == "date") {
                header = line;

                continue;
            }

            //get the values
            TimePoint time = new TimePoint(frame.headerget(header, line, "time"));

            if (time.hours == 0) {
                continue;
            }

            string lat = values[frame.headerindxget(header, "latitude")];
            string lon = values[frame.headerindxget(header, "longitude")];

            if (lat == "NaN") {
                lat = "0";
            }

            if (lon == "NaN") {
                lon = "0";
            }

            Coordinates coordinates = new Coordinates(
                double.Parse(lat),
                (double.Parse(lon)),
                double.Parse(values[frame.headerindxget(header, "altitude")])
            );

            int ms_since_last_frame = int.Parse(values[frame.headerindxget(header, "ms_since_last_frame")]);

            double speed = double.Parse(values[frame.headerindxget(header, "speed")]);
            
            double angle = double.Parse(values[frame.headerindxget(header, "angle")]);

            Vector3 gyroscope = new Vector3(
                float.Parse(values[frame.headerindxget(header, "gyro_x")]),
                float.Parse(values[frame.headerindxget(header, "gyro_y")]),
                float.Parse(values[frame.headerindxget(header, "gyro_z")])
            );

            Vector3 acceleration = new Vector3(
                float.Parse(values[frame.headerindxget(header, "accel_x")]),
                float.Parse(values[frame.headerindxget(header, "accel_y")]),
                float.Parse(values[frame.headerindxget(header, "accel_z")])
            );

            Vector3 magnetometer = new Vector3(
                float.Parse(values[frame.headerindxget(header, "mag_x")]),
                float.Parse(values[frame.headerindxget(header, "mag_y")]),
                float.Parse(values[frame.headerindxget(header, "mag_z")])
            );

            bool live = values[frame.headerindxget(header, "live_cam")] == "1";

            camframe c_frame = camframe.fromFile(line, live);

            frames.Add(new frame(time, coordinates, ms_since_last_frame, speed, angle, gyroscope, acceleration, magnetometer, c_frame));
        }

        Debug.Log("Finished reading file. Created " + frames.Count + " frames.");

        FileReader.loaded = true;

        foreach (Action loadaction in FileReader.onload) {
            loadaction();
        }
    }

    private Coordinates[] aprs_coordinates;

    void readAPRS() {
        string text = aprs.text;

        string[] lines = text.Split('\n');
        
        List<Coordinates> coordinates = new List<Coordinates>();

        string[] headers = lines[0].Split(",");

        int lat_indx = Array.IndexOf(headers, "latitude");
        int lon_indx = Array.IndexOf(headers, "longitude");
        int alt_indx = Array.IndexOf(headers, "altitude");

        for (int i = 1; i < lines.Length; i++) {
            if (!lines[i].Contains(",")) continue;

            double feet_altitude = double.Parse(lines[i].Split(",")[alt_indx]);

            coordinates.Add(new Coordinates(
                double.Parse(lines[i].Split(",")[lat_indx]),
                double.Parse(lines[i].Split(",")[lon_indx]),
                feetToMetres(feet_altitude)
            ));
        }

        aprs_coordinates = coordinates.ToArray();
    }

    public Coordinates[] getAPRS() {
        return this.aprs_coordinates;
    }

    public frame getFrame(int index) {
        return frames[index];
    }

    public int getFrameCount() {
        return frames.Count;
    }

    public struct frame {
        public TimePoint time;
        public Coordinates coordinates;
        public int ms_since_last_frame;
        public double speed;
        public double angle;

        public Vector3 gyroscope;
        public Vector3 acceleration;
        public Vector3 magnetometer;

        public camframe cframe;

        public static int headerindxget(string header, string title) {
            string[] headers = header.Split(',');

            for (int i = 0; i < headers.Length; i++) {
                if (headers[i] == title) return i;
            }

            return -1;
        }

        public static string headerget(string header, string line, string title) {
            string[] headers = header.Split(',');

            for (int i = 0; i < headers.Length; i++) {
                if (headers[i] == title) return line.Split(',')[i];
            }

            return "";
        }

        public frame(TimePoint time, Coordinates coordinates, int ms_since_last_frame, double speed, double angle, Vector3 gyroscope, Vector3 acceleration, Vector3 magnetometer, camframe frame) {
            this.time = time;
            this.coordinates = coordinates;
            this.ms_since_last_frame = ms_since_last_frame;
            this.speed = speed;
            this.angle = angle;
            this.gyroscope = gyroscope;
            this.acceleration = acceleration;
            this.magnetometer = magnetometer;
            this.cframe = frame;
        }
    }

    public struct TimePoint {
        private string time;
        public int hours;
        public int minutes;
        public int seconds;
        public int milliseconds;

        public int raw;

        public int final;

        public static int first = 0;

        public TimePoint(string time) {
            this.time = time;

            string[] times = time.Split(':');

            this.hours = int.Parse(times[0]);
            this.minutes = int.Parse(times[1]);
            this.seconds = int.Parse(times[2]);
            this.milliseconds = int.Parse(times[3]);

            this.raw = (hours * 60 * 60) + (minutes * 60) + seconds + (milliseconds / 1000);

            if (this.hours > 0 && first == 0) {
                first = raw;
                final = 0;
            }

            this.final = raw - first;
        }
    }

    public double feetToMetres(double feet) {
        return feet * 0.3048;
    }
}
