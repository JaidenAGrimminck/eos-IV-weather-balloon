using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrinkinator : MonoBehaviour
{
    public ButtonStand button;
    public Laser laser;

    public float distanceDown = 1f;

    Vector3 originalPosition;

    Vector3 underPosition;

    public GrabHouse house;

    // Start is called before the first frame update
    void Start()
    {
        button.AddAction(OnButtonClick);

        originalPosition = transform.position;
        underPosition = new Vector3(transform.position.x, transform.position.y - distanceDown, transform.position.z);

        InstantMoveDown();
    }

    bool on = false;

    void OnButtonClick() {
        if (on) return;
        laser.StartLaser();
        on = true;
        StartCoroutine(wait());
    }

    IEnumerator wait() {
        Debug.Log("Shrinkinator button clicked");

        yield return new WaitForSeconds(5f);
        laser.StopLaser();
        on = false;

        house.moveOver();

        Debug.Log("Shrinkinator button stopped");

        MoveDown();
    }

    public void MoveUp() {
        button.MovementState(true);
        state = movestate.Up;
    }

    public void MoveDown() {
        button.MovementState(false);
        state = movestate.Down;
    }

    public void InstantMoveDown() {
        button.MovementState(false);
        transform.position = underPosition;
    }

    enum movestate {
        Up,
        Down,
        None
    }

    movestate state = movestate.None;

    // Update is called once per frame
    void Update()
    {
        if (state == movestate.Up) {
            transform.position = Vector3.Lerp(transform.position, originalPosition, Time.deltaTime * 2f);
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f) {
                state = movestate.None;
            }
        } else if (state == movestate.Down) {
            transform.position = Vector3.Lerp(transform.position, underPosition, Time.deltaTime * 2f);
            if (Vector3.Distance(transform.position, underPosition) < 0.01f) {
                state = movestate.None;
            }
        }
    }
}
