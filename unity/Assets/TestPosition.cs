using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TestPosition : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        text.text = "x: " + transform.position.x + ", y: " + transform.position.y + ", z: " + transform.position.z;
    }
}
