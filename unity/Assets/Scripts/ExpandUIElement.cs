using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpandUIElement : MonoBehaviour
{
    float originalWidth;
    float originalHeight;

    public bool expanding = false;
    public bool minimizing = false;
    
    enum ExpansionState {
        EXPANDED,
        MINIMIZED
    }

    ExpansionState state = ExpansionState.MINIMIZED;
    
    // Start is called before the first frame update
    void Start()
    {
        originalWidth = GetComponent<RectTransform>().rect.width;
        originalHeight = GetComponent<RectTransform>().rect.height;
        //set the width and height to 0
        GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        //expand the width and height using lerp
        if (expanding) {
            GetComponent<RectTransform>().sizeDelta = new Vector2(
                Mathf.Lerp(GetComponent<RectTransform>().rect.width, originalWidth, Time.deltaTime * 10), 
                Mathf.Lerp(GetComponent<RectTransform>().rect.height, originalHeight, Time.deltaTime * 10)
            );
        }

        //if the width and height are close to the original width and height, stop expanding
        if (expanding && Mathf.Abs(GetComponent<RectTransform>().rect.width - originalWidth) < 0.1f && Mathf.Abs(GetComponent<RectTransform>().rect.height - originalHeight) < 0.1f) {
            expanding = false;
        }

        if (minimizing) {
            GetComponent<RectTransform>().sizeDelta = new Vector2(
                Mathf.Lerp(GetComponent<RectTransform>().rect.width, 0, Time.deltaTime * 10), 
                Mathf.Lerp(GetComponent<RectTransform>().rect.height, 0, Time.deltaTime * 10)
            );
        }

        //if the width and height are close to the original width and height, stop expanding
        if (minimizing && Mathf.Abs(GetComponent<RectTransform>().rect.width) < 0.001f && Mathf.Abs(GetComponent<RectTransform>().rect.height) < 0.1f) {
            minimizing = false;
        }
    }

    public void minimize() {
        minimizing = true;
        expanding = false;
        state = ExpansionState.MINIMIZED;
    }

    public void expand() {
        expanding = true;
        minimizing = false;
        state = ExpansionState.EXPANDED;
    }

    public void flip() {
        if (state == ExpansionState.EXPANDED) {
            minimize();
        } else {
            expand();
        }
    }
}
