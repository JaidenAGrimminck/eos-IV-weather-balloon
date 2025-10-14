using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GrabHouse : MonoBehaviour
{

    private Rigidbody rb;

    private OVRGrabbable ovrg;

    private bool grabbed = false;

    public Transform scenery;

    private Vector3 startPosition;
    private Vector3 awayPosition;

    public PathDisplay path;

    public TextMeshPro text;

    public Shrinkinator shrink;


    void Start() {
        startPosition = transform.position;

        awayPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + 30f);

        transform.position = awayPosition;

        rb = GetComponent<Rigidbody>();
        ovrg = GetComponent<OVRGrabbable>();
    }

    void Update() {
        if (moving) {
            if (!rb.isKinematic) {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            transform.position = Vector3.Lerp(transform.position, startPosition, Time.deltaTime * 1.1f);

            float dist = Vector3.Distance(transform.position, startPosition);

            text.text = ((1 - (dist / 50)) * 1000) + " Meters";

            if (dist < 0.01f) {
                text.text = "1000 Meters";
                moving = false;

                rb.isKinematic = false;
            }

            return;
        }

        if (ovrg.isGrabbed) {
            grabbed = true;
        } else {
            if (grabbed) {
                rb.useGravity = true;
                rb.isKinematic = false;
            }

            grabbed = false;
        }
    }
    
    [SerializeField]
    bool moving = false;

    public void moveOver() {
        moving = true;
    }

    void OnCollisionEnter(Collision collision) {
        if (!rb.useGravity) return;

        if (collision.gameObject.tag == "ground") {
            //remove the OVRGrabbable component
            Destroy(GetComponent<OVRGrabbable>());

            foreach (Transform obj in scenery) {
                if (obj.gameObject.tag == "stuck_tree") continue;

                //check if has mesh collider
                if (obj.GetComponent<MeshCollider>() == null) {
                    //add mesh collider
                    obj.gameObject.AddComponent<MeshCollider>();

                    //set mesh collider to convex
                    obj.GetComponent<MeshCollider>().convex = true;
                }

                //check if has rigidbody
                if (obj.GetComponent<Rigidbody>() == null) {
                    //add rigidbody
                    obj.gameObject.AddComponent<Rigidbody>();
                }

                //set mass
                obj.GetComponent<Rigidbody>().mass = 1000;
                
                //add velocity away from the house
                Vector3 direction = obj.position - transform.position;

                obj.GetComponent<Rigidbody>().velocity = direction.normalized * 5;

                Destroy(obj.gameObject, 10f);
            }

            path.Display();
        }
    }
}
