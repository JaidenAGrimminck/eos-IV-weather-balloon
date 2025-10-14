using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraUtils
{ 
    public static readonly double CAM_FOV_WIDTH = 55;
    public static readonly double CAM_FOV_HEIGHT = 35;

    public static double flatDistanceFromAltitude(double altitude, double fov) {
        double convertedFOV = (fov / 2) * Math.PI / 180;

        return 2 * (altitude / Math.Sin((Math.PI / 2) - convertedFOV)) * Math.Sin(convertedFOV);
    }
}
