using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonStand : MonoBehaviour
{

    [SerializeField]
    private bool pressed;

    enum ButtonType {
        NextFrame,
        LastFrame,
        None
    }

    enum Direction {
        Down,
        X,
        Z
    }

    [SerializeField]
    private ButtonType buttonType;

    [SerializeField]
    private Direction direction;

    public AnimationManager animManager;

    public float movementDown = 0.02f;

    private float minimumWaitTime = 1f;

    public float SetMinimumWaitTime {
        set { minimumWaitTime = value; }
    }

    private bool readyToPress = true;

    //click sound
    public AudioClip clickDown;

    public AudioClip clickUp;

    private Vector3 pressedPosition;
    private Vector3 unpressedPosition;

    private List<Action> actions = new List<Action>();
    
    // Start is called before the first frame update
    void Start()
    {
        unpressedPosition = transform.position;
        if (direction == Direction.Down) {
            pressedPosition = new Vector3(transform.position.x, transform.position.y - movementDown, transform.position.z);
        } else if (direction == Direction.X) {
            pressedPosition = new Vector3(transform.position.x - movementDown, transform.position.y, transform.position.z);
        } else if (direction == Direction.Z) {
            pressedPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - movementDown);
        }
    }

    public void AddAction(Action action) {
        actions.Add(action);
    }

    List<Action> pressActions = new List<Action>();
    List<Action> releaseActions = new List<Action>();

    public void OnPress(Action action) {
        pressActions.Add(action);
    }

    public void OnRelease(Action action) {
        releaseActions.Add(action);
    }

    public bool isPressed() {
        return pressed;
    }

    [SerializeField]
    bool active = true;

    public void MovementState(bool on) {
        active = on;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) return;

        if (pressed) {
            transform.position = Vector3.Lerp(transform.position, pressedPosition, Time.deltaTime * 10);
        } else {
            transform.position = Vector3.Lerp(transform.position, unpressedPosition, Time.deltaTime * 10);
        }

        //check all active colliders and triggers
        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2, transform.rotation);
        bool foundHand = false;
        foreach (Collider col in colliders) {
            if (col.gameObject.tag == "hand") {
                foundHand = true;
                break;
            }
        }

        if (!foundHand && this.pressed) {
            setPressed(false);
            StartCoroutine(WaitTillPress());
        }
    }
    public void setPressed(bool pressed) {
        if (this.pressed) {
            if (buttonType == ButtonType.NextFrame) {
                animManager.NextFrame();
            } else if (buttonType == ButtonType.LastFrame) {
                animManager.LastFrame();
            }

            for (int i = 0; i < actions.Count; i++) {
                actions[i].Invoke();
            }

            for (int i = 0; i < releaseActions.Count; i++) {
                releaseActions[i].Invoke();
            }
            
            if (clickUp != null) {
                //play sound
                AudioSource.PlayClipAtPoint(clickUp, Camera.main.transform.position);
            }
        } else {
            if (clickDown != null) {
                //play sound
                AudioSource.PlayClipAtPoint(clickDown, Camera.main.transform.position);
            }

            for (int i = 0; i < pressActions.Count; i++) {
                pressActions[i].Invoke();
            }
        }

        this.pressed = pressed;
    }

    IEnumerator WaitTillPress() {
        readyToPress = false;
        yield return new WaitForSeconds(minimumWaitTime);
        readyToPress = true;
    }

    int objectsColliding = 0;

    //On trigger enter
    void OnTriggerEnter(Collider other) {
        if (!active) return;
        
        if (other.gameObject.tag == "hand") {
            if (!readyToPress) return;

            if (objectsColliding == 0) {
                setPressed(true);
            }

            objectsColliding++;
        }
    }

    //On trigger exit
    void OnTriggerExit(Collider other) {
        if (!active) return;

        //check the tag
        if (other.gameObject.tag == "hand") {
            if (objectsColliding == 1) {
                setPressed(false);
                StartCoroutine(WaitTillPress());
            }

            objectsColliding--;

            if (objectsColliding < 0) {
                objectsColliding = 0;
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        OnTriggerEnter(collision.collider);
    }

    void OnCollisionExit(Collision collision) {
        OnTriggerExit(collision.collider);
    }
}
