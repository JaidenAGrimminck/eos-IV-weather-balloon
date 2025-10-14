using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Graph : MonoBehaviour
{
    public TextMeshProUGUI x_label;
    public TextMeshProUGUI y_label;

    public Transform bottomLeft;
    public Transform topRight;

    public Transform dataPointHolder;
    public GameObject dataPointPrefab;

    private List<float> x_values;
    private List<float> y_values;
    
    // Start is called before the first frame update
    void Start()
    {
        FileReader.awaitForLoad(onFileLoad);
    }

    void onFileLoad() {
        List<float> xvalues = new List<float>();
        List<float> yvalues = new List<float>();
        
        for (int i = 0; i < FileReader.instance.getFrameCount(); i += 100) {
            FileReader.frame frame = FileReader.instance.getFrame(i);

            if (frame.coordinates.Altitude < 50) continue;

            Debug.Log(frame.time.final / 1000);

            xvalues.Add((float) (frame.time.final / 1000));
            yvalues.Add((float)frame.coordinates.Altitude);
        }

        GraphData("Time", "Altitude", xvalues, yvalues);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void createDataPoint(float x, float y, float percentageX, float percentageY) {
        GameObject dataPoint = Instantiate(dataPointPrefab, dataPointHolder);

        dataPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            Mathf.Lerp(bottomLeft.GetComponent<RectTransform>().anchoredPosition.x, topRight.GetComponent<RectTransform>().anchoredPosition.x, percentageX),
            Mathf.Lerp(bottomLeft.GetComponent<RectTransform>().anchoredPosition.y, topRight.GetComponent<RectTransform>().anchoredPosition.y, percentageY)
        );

        dataPoint.GetComponent<DataPoint>().SetData(x, y);
    }

    public void GraphData(string xlabel, string ylabel, List<float> xvalues, List<float> yvalues) {
        if (xvalues.Count != yvalues.Count) {
            Debug.LogError("xvalues and yvalues must be the same length");
            return;
        }
        
        x_label.text = xlabel;
        y_label.text = ylabel;

        x_values = xvalues;
        y_values = yvalues;

        //clear the data points
        foreach (Transform child in dataPointHolder) {
            Destroy(child.gameObject);
        }

        //find the minimum and maximum values
        float min_x = Mathf.Infinity;
        float max_x = -Mathf.Infinity;

        float min_y = Mathf.Infinity;
        float max_y = -Mathf.Infinity;

        foreach (float x in x_values) {
            if (x < min_x) {
                min_x = x;
            }

            if (x > max_x) {
                max_x = x;
            }
        }

        foreach (float y in y_values) {
            if (y < min_y) {
                min_y = y;
            }

            if (y > max_y) {
                max_y = y;
            }
        }

        //create the data points
        for (int i = 0; i < xvalues.Count; i++) {
            float x = x_values[i];
            float y = y_values[i];

            float percentageX = (x - min_x) / (max_x - min_x);
            float percentageY = (y - min_y) / (max_y - min_y);

            createDataPoint(x, y, percentageX, percentageY);
        }
    }
}
