using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageDisplay : MonoBehaviour
{
    public static ImageDisplay instance;

    public float scale = 1f;

    public GameObject spritePrefab;

    public Texture2D[] irCamImages;

    private List<Coordinates> coords;

    private List<GameObject> images;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        coords = new List<Coordinates>();

        foreach (Texture2D text in irCamImages) {
            coords.Add(coordsFromTextureName(text));
        }

        FileReader.awaitForLoad(FrameLoaded);
    }

    void FrameLoaded() {
        images = new List<GameObject>();
        Coordinates firstPos = null;

        float y = 0;

        int numberMade = 0;
        for (int i = 0; i < FileReader.instance.getFrameCount(); i++) {
            FileReader.frame frame = FileReader.instance.getFrame(i);
            Coordinates pos = frame.coordinates;

            if (pos.Latitude == 0 || pos.Longitude == 0) continue;

            Coordinates textCoords = CoordinateInList(pos);

            if (textCoords == null) continue;

            Texture2D text = getTextureFromCoords(textCoords);

            float altitude = (float)pos.Altitude;

            float flatDistMetresWidth = (float)CameraUtils.flatDistanceFromAltitude(altitude, CameraUtils.CAM_FOV_WIDTH);
            float flatDistMetresHeight = (float)CameraUtils.flatDistanceFromAltitude(altitude, CameraUtils.CAM_FOV_HEIGHT);

            float triangleBaseDistanceWidthKM = flatDistMetresWidth / 1000;
            float triangleBaseDistanceHeightKM = flatDistMetresHeight / 1000;

            if (firstPos == null) {
                firstPos = pos;
                continue;
            }

            float angle = (float)frame.angle;

            Vector3 gPosition = new Coordinates(pos.Latitude - firstPos.Latitude, pos.Longitude - firstPos.Longitude, pos.Altitude).FindPosition();

            gPosition *= scale;

            Vector3 adjust = new Vector3(6376.187f, 0, 0);

            Vector3 gPosition2 = gPosition - adjust;

            GameObject sprite = CreateSprite(text);

            sprite.transform.position = new Vector3(gPosition2.x, y, gPosition2.z);
            
            sprite.transform.rotation = Quaternion.Euler(0, angle, 0);

            sprite.transform.localScale = new Vector3(triangleBaseDistanceWidthKM, 0.001f, triangleBaseDistanceHeightKM);

            sprite.SetActive(false);

            images.Add(sprite);
            
            numberMade++;

            y += 0.01f;
        }

        SliderController.totalFrames = numberMade;
    }

    GameObject CreateSprite(Texture2D text) {
        //Create a new cube GameObject
        GameObject sprite = GameObject.CreatePrimitive(PrimitiveType.Cube);

        //set the position of the cube
        sprite.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        //set the scale of the cube
        sprite.transform.localScale = new Vector3(0.96f, 0.001f, 0.72f);
        

        sprite.transform.parent = transform;

        //sprite is a square, so we need to add a material to it
        Material mat = new Material(Shader.Find("Sprites/Default"));

        //set the texture of the material to the texture we loaded
        mat.mainTexture = text;

        //set the smoothness to 0
        mat.SetFloat("_Glossiness", 0);

        mat.SetTexture("_MainTex", text);

        //set the material of the cube to the material we just created
        sprite.GetComponent<Renderer>().material = mat;

        return sprite;
    }

    List<Coordinates> usedCoords = new List<Coordinates>();

    public Coordinates CoordinateInList(Coordinates other) {
        for (int i = 0; i < coords.Count; i++) {
            if (coords[i].CloseEnough(other, 0.001f)) {
                if (usedCoords.Contains(coords[i])) return null;

                usedCoords.Add(coords[i]);
                return coords[i];
            }
        }

        return null;
    }

    int activeIndex = 0;

    public void MoveActiveIndex(int indx) {
        images[activeIndex].SetActive(false);

        activeIndex = indx;

        images[activeIndex].SetActive(true);
    }
    
    public Texture2D getTextureFromCoords(Coordinates coords) {
        for (int i = 0; i < irCamImages.Length; i++) {
            if (coordsFromTextureName(irCamImages[i]).EqualsNoAltitude(coords)) return irCamImages[i];
        }

        return null;
    }

    Coordinates coordsFromTextureName(Texture2D text) {
        string name = text.name;

        string[] split = name.Split('_');

        float lat = float.Parse(split[1]);
        float lon = float.Parse(split[2]);

        return new Coordinates(lat, lon, 0);
    }
}
