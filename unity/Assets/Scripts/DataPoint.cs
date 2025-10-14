using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPoint : MonoBehaviour
{
    [SerializeField]
    private float x;

    [SerializeField]
    private float y;

    public void SetData(float x, float y) {
        this.x = x;
        this.y = y;
    }
}
