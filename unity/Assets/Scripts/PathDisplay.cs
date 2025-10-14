using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathDisplay : MonoBehaviour
{
    private static PathDisplay instance;

    public static PathDisplay Instance {
        get { return instance; }
    }

    public float scale = 1;
    public float altitude_scale = 0;

    public float sphere_scale = 0.1f;
    public float line_width = 0.1f;

    public GameObject tree;

    public Transform aprsParent;

    public static float GetScale() {
        return Instance.scale;
    }

    enum DisplayState {
        HIDDEN,
        SHOWING
    }

    private DisplayState state = DisplayState.HIDDEN;

    public AnimationManager animationManager;
    
    // Start is called before the first frame update
    void Start() 
    {
        instance = this;

        FileReader.awaitForLoad(FrameLoaded);
    }

    void FrameLoaded()
    {
        Coordinates lastPos = null;
        Coordinates firstPos = null;

        Vector3 lastGPosition = new Vector3(0,0,0);

        float highestAltitude = 0;
        
        for (int i = 0; i < FileReader.instance.getFrameCount(); i++) {
            FileReader.frame frame = FileReader.instance.getFrame(i);
            Coordinates pos = frame.coordinates;

            if (pos.Latitude == 0 || pos.Longitude == 0) continue;

            if (lastPos == null) {
                lastPos = pos;
                firstPos = pos;
                continue;
            }

            Vector3 gPosition = new Coordinates(pos.Latitude - firstPos.Latitude, pos.Longitude - firstPos.Longitude, pos.Altitude).FindPosition();

            gPosition *= scale;

            if (Vector3.Distance(gPosition, lastGPosition) < 0.0001) {
                continue;
            }

            Vector3 adjust = new Vector3(6376.187f, 0, 0);

            CreateSphere(gPosition - adjust);
            if (lastGPosition.x > 0) CreateLine(lastGPosition - adjust, gPosition - adjust);

            lastGPosition = gPosition;

            if ((float)pos.Altitude > highestAltitude) highestAltitude = (float)pos.Altitude;
        }
        
        Debug.Log("Made Lines");

        float lastAltitude = 0;

        for (int i = 0; i < FileReader.instance.getAPRS().Length; i++) {
            Coordinates pos = FileReader.instance.getAPRS()[i];

            if (pos.Latitude == 0 || pos.Longitude == 0) continue;

            if ((float)pos.Altitude < highestAltitude) {
                continue;
            }

            if ((float)pos.Altitude < lastAltitude) {
                continue;
            }

            Vector3 gPosition = new Coordinates(pos.Latitude - firstPos.Latitude, pos.Longitude - firstPos.Longitude, pos.Altitude).FindPosition();

            gPosition *= scale;

            Vector3 adjust = new Vector3(6376.187f, 0, 0);

            GameObject sphere = CreateSphere(gPosition - adjust, new Color(1,1,0,1));

            sphere.transform.parent = aprsParent;

            GameObject line = CreateLine(lastGPosition - adjust, gPosition - adjust, new Color(1,0,1,1));

            line.transform.parent = aprsParent;

            lastAltitude = (float)pos.Altitude;

            lastGPosition = gPosition;
        }

        Debug.Log("Made second lines");
    }

    GameObject CreateLine(Vector3 pos1, Vector3 pos2) {
        return CreateLine(pos1, pos2, new Color(0, 0, 1, 1));
    }

    GameObject CreateLine(Vector3 pos1, Vector3 pos2, Color color) {
        GameObject line = new GameObject("Line");

        line.transform.parent = transform;

        LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        
        //make the line blue
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;

        lineRenderer.widthMultiplier = line_width;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);

        line.SetActive(false);

        return line;
    }

    GameObject CreateSphere(Vector3 pos) {
        return CreateSphere(pos, new Color(1, 0, 0, 1));
    }

    GameObject CreateSphere(Vector3 pos, Color color) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        sphere.transform.parent = transform;

        sphere.transform.position = pos;
        
        //set red
        sphere.GetComponent<Renderer>().material.color = color;

        //set radius
        sphere.transform.localScale = new Vector3(sphere_scale, sphere_scale, sphere_scale);

        sphere.SetActive(false);

        return sphere;
    }

    public void Display() {
        //displayedOnce = true;
        StartCoroutine(punchTree());
        StartCoroutine(DisplayWait());
    }

    bool cancelCoroutine = false;

    //bool displayedOnce = false;

    public void Hide() {
        cancelCoroutine = true;

        state = DisplayState.HIDDEN;
        foreach(Transform child in transform) {
            child.gameObject.SetActive(false);
        }
    }

    IEnumerator punchTree() {
        //add a mesh collider and rigidbody
        tree.AddComponent<MeshCollider>();
        //set mesh collider to convex
        tree.GetComponent<MeshCollider>().convex = true;

        yield return new WaitForSeconds(1f);

        tree.AddComponent<Rigidbody>();

        tree.GetComponent<Rigidbody>().mass = 1000;

        //add motion off to the side
        tree.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 3);
    }

    IEnumerator DisplayWait()
    {
        cancelCoroutine = false;

        state = DisplayState.SHOWING;
        float wait_time = 30f / transform.childCount;

        foreach (Transform child in transform) {
            yield return new WaitForSeconds(wait_time);

            if (cancelCoroutine) {
                cancelCoroutine = false;
                yield break;
            }

            child.gameObject.SetActive(true);
        }

        animationManager.enable();
    }

    public void flip() {
        if (state == DisplayState.HIDDEN) {
            //if (!displayedOnce) 
            Display();
        } else {
            Hide();
        }
    }

    // Update is called once per frame
    void Update()
    {
     
    }
}
