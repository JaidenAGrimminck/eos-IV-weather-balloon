using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bob : MonoBehaviour
{
    public float delay = 1f;
    public float yOffset = 0.1f;
    public float speed = 5f;

    public bool isGUI = false;

    private Vector3 originalPosition;
    private Vector3 newPosition;

    private bool ready = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!isGUI) {
            originalPosition = transform.position;
            newPosition = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        } else {
            originalPosition = GetComponent<RectTransform>().anchoredPosition3D;
            newPosition = new Vector3(GetComponent<RectTransform>().anchoredPosition3D.x, GetComponent<RectTransform>().anchoredPosition3D.y + yOffset, GetComponent<RectTransform>().anchoredPosition3D.z);
        }
        StartCoroutine(WaitTillStart());
    }

    IEnumerator WaitTillStart() {
        yield return new WaitForSeconds(delay);

        ready = true;
    }

    void Update() {
        if (!ready) return;

        if (!isGUI) {
            transform.position = Vector3.Lerp(originalPosition, newPosition, Mathf.PingPong(Time.time * speed, 1));
        } else {
            GetComponent<RectTransform>().anchoredPosition3D = Vector3.Lerp(originalPosition, newPosition, Mathf.PingPong(Time.time * speed, 1));
        }
    }
}
