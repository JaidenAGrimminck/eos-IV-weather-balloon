using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public GameObject setupImage;
    public PathDisplay pathDisplay;

    public GameObject tree;

    public Shrinkinator shrink;
    public GameObject meter;

    public ExpandUIElement flightComputer;

    public ExpandUIElement altTimeGraph;
    public ExpandUIElement externalTempGraph;
    public ExpandUIElement avgIrTempGraph;

    public ExpandUIElement warmColdFront;
    public ExpandUIElement infaredPool;
    public ExpandUIElement IRCup;

    public ExpandUIElement landMassIR;
    public ExpandUIElement cloudIR;

    public ExpandUIElement reflectionDepiction;
    public ExpandUIElement nationalGeographicExcerpt;
    public ExpandUIElement thankYou;

    int frame = 0;

    // Start is called before the first frame update
    void Start()
    {
        FrameChange(true);
    }

    bool enabled = true;
    public void disable() {
        enabled = false;
    }

    public void enable() {
        enabled = true;
    }

    public void NextFrame() {
        if (!enabled) return;

        frame++;
        FrameChange(false);
    }

    public void LastFrame() {
        if (!enabled) return;
        
        frame--;
        if (frame < 0) {
            frame = 0;
        }
        FrameChange(true);
    }

    /**
    1/2 = image
    3 = path
    */
    void FrameChange(bool back) {
        if (frame == 1 || frame == 2) {
            flightComputer.flip();
        }

        if (frame == 2 || frame == 3) {
            setupImage.GetComponent<ExpandUIElement>().flip();
        }

        if (frame == 3) {
            shrink.MoveUp();
            meter.SetActive(true);
            this.disable();
        }

        if (frame == 4 || frame == 5) {
            altTimeGraph.flip();
        }

        if (frame == 5 || frame == 6) {
            externalTempGraph.flip();
        }

        if (frame == 6 || frame == 7) {
            avgIrTempGraph.flip();
        }

        if (frame == 7 || frame == 8) {
            warmColdFront.flip();
        }

        if (frame == 8 || frame == 9) {
            infaredPool.flip();
        }

        if (frame == 9 || frame == 10) {
            IRCup.flip();
        }

        if (frame == 10 || frame == 11) {
            landMassIR.flip();
        }

        if (frame == 11 || frame == 12) {
            cloudIR.flip();
        }

        if (frame == 12 || frame == 13) {
            reflectionDepiction.flip();
        }

        if (frame == 13 || frame == 14) {
            nationalGeographicExcerpt.flip();
        }

        if (frame == 14) {
            avgIrTempGraph.flip();
        }

        if (frame == 15) {
            avgIrTempGraph.flip();
            thankYou.flip();
        }
    }
}
