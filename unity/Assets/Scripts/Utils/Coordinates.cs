using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinates
{
    private static double R = 6378137; // metres
    private double latitude;
    private double longitude;

    private double altitude;

    public Coordinates(double latitude, double longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = 0;
    }

    public Coordinates(double latitude, double longitude, double altitude) {
        this.latitude = latitude;
        this.longitude = longitude;
        this.altitude = altitude;
    }

    public double Latitude
    {
        get { return latitude; }
        set { latitude = value; }
    }

    public double Longitude
    {
        get { return longitude; }
        set { longitude = value; }
    }

    public double Altitude
    {
        get { return altitude; }
        set { altitude = value; }
    }

    /**
        * Returns the distance to another set of coordinates in meters.
        * Credit to: https://www.movable-type.co.uk/scripts/latlong.html
        * @param other The other set of coordinates.
        * @return The distance to the other set of coordinates in meters.
    */
    public double DistanceTo(Coordinates other) {
        double φ1 = latitude * Math.PI / 180;
        double φ2 = other.latitude * Math.PI / 180;
        double Δφ = (other.latitude - latitude) * Math.PI / 180;
        double Δλ = (other.longitude - longitude) * Math.PI / 180;

        double a = Math.Sin(Δφ/2) * Math.Sin(Δφ/2) + Math.Cos(φ1) * Math.Cos(φ2) * Math.Sin(Δλ/2) * Math.Sin(Δλ/2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));

        double d = R * c;

        return d;
    }

    /**
        * Returns the bearing to another set of coordinates in meters, contained in a Vector2.
    */
    public Vector2 Offset(Coordinates other) {
        Coordinates C = new Coordinates(other.latitude, longitude);
        Coordinates D = new Coordinates(latitude, other.longitude);

        double x = DistanceTo(C);
        double y = DistanceTo(D);

        if (other.latitude < latitude) {
            y *= -1;
        }

        if (other.longitude < longitude) {
            x *= -1;
        }

        return new Vector2((float)x, (float)y);
    }

    public Vector2 NearDistance(Coordinates other) {
        float y_dist = (float)Math.Abs(this.latitude - other.latitude) * 111000f;
        float x_dist = (float)Math.Abs(this.longitude - other.longitude) * 111000f * (float)Math.Cos(this.latitude);

        return new Vector2((float)x_dist, (float)y_dist);
    }

    public Vector3 FindPosition() {
        double r = this.altitude + R;

        double φ = latitude * Math.PI / 180;
        double λ = longitude * Math.PI / 180;

        double x = r * Math.Cos(φ) * Math.Cos(λ);
        double y = r * Math.Cos(φ) * Math.Sin(λ);
        double z = r * Math.Sin(φ);

        return new Vector3((float)x, (float)y, (float)z);
    }

    public bool EqualsNoAltitude(Coordinates other) {
        return this.latitude == other.latitude && this.longitude == other.longitude;
    }

    public bool CloseEnough(Coordinates other, float leniency) {
        return this.latitude - other.latitude < leniency && this.longitude - other.longitude < leniency;
    }

    public double PDist(Coordinates other) {
        return Math.Sqrt(Math.Pow(this.latitude - other.latitude, 2) + Math.Pow(this.longitude - other.longitude, 2));
    }
}
