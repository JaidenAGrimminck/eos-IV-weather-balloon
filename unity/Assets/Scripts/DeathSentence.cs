using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathSentence : MonoBehaviour
{
    public void Enter() {
        StartCoroutine(wait());
    }

    IEnumerator wait() {
        yield return new WaitForSeconds(10f);
        Destroy(gameObject);
    }
}
