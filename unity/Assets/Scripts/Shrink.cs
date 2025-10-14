using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrink : MonoBehaviour
{
    public float scale = 0.001f;

    bool shrinking = false;

    bool disableOnceFinished = false;
    
    // Update is called once per frame
    void Update()
    {
        if (shrinking) {
            transform.localScale = new Vector3(
                Mathf.Lerp(transform.localScale.x, scale, Time.deltaTime * 10), 
                Mathf.Lerp(transform.localScale.y, scale, Time.deltaTime * 10),
                Mathf.Lerp(transform.localScale.z, scale, Time.deltaTime * 10)
            );

            if (Mathf.Abs(transform.localScale.x - scale) < 0.001f && Mathf.Abs(transform.localScale.y - scale) < 0.001f && Mathf.Abs(transform.localScale.z - scale) < 0.001f) {
                shrinking = false;
                if (disableOnceFinished) {
                    gameObject.SetActive(false);
                }
            }
        }
    }

    public void DoShrink(bool disableOnceFinished) {
        this.disableOnceFinished = disableOnceFinished;
        shrinking = true;
    }
}
