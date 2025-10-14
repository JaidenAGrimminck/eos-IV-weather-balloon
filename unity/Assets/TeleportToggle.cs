using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportToggle : MonoBehaviour
{
    Vector3 originalPosition;
    public Transform teleportPosition;

    public GameObject teleportCylinder;

    enum position {
        original,
        teleport
    }

    position currentPosition = position.original;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    public void toggle() {
        if (currentPosition == position.original) {
            transform.position = teleportPosition.position;
            currentPosition = position.teleport;
        } else {
            transform.position = originalPosition;
            currentPosition = position.original;
        }
    }
}
