using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public GameObject laser;

    public AudioClip laserSound;

    public float distanceDown = 1f;

    [SerializeField]
    bool laserEnabled = false;

    bool moveUp = false;

    Vector3 originalPosition;
    Vector3 underPosition;

    public GameObject bigHouse;

    // Start is called before the first frame update
    void Start()
    {
        laser.SetActive(false);

        originalPosition = transform.position;
        underPosition = new Vector3(transform.position.x, transform.position.y - distanceDown, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (laserEnabled) {
            //change the y value of the rotation back and forth from -20 to 20
            transform.rotation = Quaternion.Euler(0, Mathf.PingPong(Time.time * 20, 20) - 10, 0);
        }
    }

    public void StopLaser() {
        bigHouse.GetComponent<Shrink>().DoShrink(true);

        laser.SetActive(false);
        laserEnabled = false;

        //stop audio
        AudioSource laserAudio = GetComponent<AudioSource>();
        laserAudio.Stop();
    }

    public void StartLaser() {
        laser.SetActive(true);
        laserEnabled = true;

        //play audio
        AudioSource laserAudio = GetComponent<AudioSource>();
        laserAudio.clip = laserSound;
    }
}
