using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderController : MonoBehaviour
{
    public GameObject slider;
    public GameObject constraints;

    public ButtonStand buttonLeft;
    public ButtonStand buttonRight;

    float percentageThrough = 0;

    public static int totalFrames = 0;

    int currentFrame = 0;

    // Start is called before the first frame update
    void Start()
    {
        buttonLeft.OnPress(ButtonLeftPress);
        buttonRight.OnPress(ButtonRightPress);

        buttonLeft.OnRelease(LeftRelease);
        buttonRight.OnRelease(RightRelease);

        buttonLeft.SetMinimumWaitTime = 0.5f;
        buttonRight.SetMinimumWaitTime = 0.5f;
    }

    bool waitForLeftHold = false;

    void ButtonLeftPress() {
        StartCoroutine(LeftHold());
    }

    IEnumerator LeftHold() {
        yield return new WaitForSeconds(3f);
        if (!buttonLeft.isPressed()) yield break;
        waitForLeftHold = true;
    }

    void LeftRelease() {
        if (!waitForLeftHold) {
            currentFrame--;
        }

        waitForLeftHold = false;
    }

    bool waitForRightHold = false;

    void ButtonRightPress() {
        StartCoroutine(RightHold());
    }

    IEnumerator RightHold() {
        yield return new WaitForSeconds(3f);
        if (!buttonRight.isPressed()) yield break;
        waitForRightHold = true;
    }

    void RightRelease() {
        if (!waitForRightHold) {
            currentFrame++;
        }

        waitForRightHold = false;
    }

    public void NextFrame() {
        currentFrame++;

        if (currentFrame >= totalFrames) {
            currentFrame = totalFrames - 1;
        }
    }

    public void LastFrame() {
        currentFrame--;

        if (currentFrame < 0) {
            currentFrame = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //slider can't go past x and z constraints of constraints
        Vector3 sliderPos = slider.transform.position;
        Vector3 constraintsPos = constraints.transform.position;
        Vector3 constraintsScale = constraints.transform.localScale;

        Vector3 lowerBound = new Vector3(constraintsPos.x - constraintsScale.x / 2, sliderPos.y, constraintsPos.z - constraintsScale.z / 2);
        Vector3 upperBound = new Vector3(constraintsPos.x + constraintsScale.x / 2, sliderPos.y, constraintsPos.z + constraintsScale.z / 2);

        //use percentageThrough to determine where the slider is
        slider.transform.position = new Vector3(
            Mathf.Lerp(lowerBound.x, upperBound.x, 1 - percentageThrough),
            sliderPos.y,
            Mathf.Lerp(lowerBound.z, upperBound.z, 1 - percentageThrough)
        );
    }

    bool flipflop = false; //reduce rate to 25fps
    void FixedUpdate() {
        if (flipflop) {
            flipflop = false;
            return;
        }

        if (waitForLeftHold && buttonLeft.isPressed()) {
            currentFrame--;
        }

        if (waitForRightHold && buttonRight.isPressed()) {
            currentFrame++;
        }

        //update currentFrame
        currentFrame = Mathf.Clamp(currentFrame, 0, totalFrames);

        ImageDisplay.instance.MoveActiveIndex(currentFrame);

        //update percentageThrough
        percentageThrough = (float)currentFrame / (float)totalFrames;

        flipflop = true;
    }
}
