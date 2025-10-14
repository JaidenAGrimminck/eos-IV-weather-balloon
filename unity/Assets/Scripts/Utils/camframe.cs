using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camframe
{
    public static readonly int HEIGHT = 24;
    public static readonly int WIDTH = 32;

    private List<double> temperatures;

    private bool live = false;

    private bool incomplete = false;

    public static camframe fromFile(string line, bool live) {
        string[] values = line.Split(',');

        List<double> temperatures = new List<double>();

        for (int i = 0; i < HEIGHT * WIDTH; i++) {
            //check if i is out of bounds
            if (values.Length - ((HEIGHT * WIDTH) + 1) + i > values.Length - 1 || values.Length - ((HEIGHT * WIDTH) + 1) + i < 0) {
                break;
            }
            
            temperatures.Add(
                double.Parse(values[values.Length - ((HEIGHT * WIDTH) + 1) + i])
            );
        }

        return new camframe(temperatures, live);
    }

    public camframe(List<double> temperatures, bool live) {
        this.temperatures = temperatures;
        this.live = live;

        if (temperatures.Count != HEIGHT * WIDTH) {
            incomplete = true;

            for (int i = temperatures.Count; i < HEIGHT * WIDTH; i++) {
                temperatures.Add(0);
            }
        }
    }
}
