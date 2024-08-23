using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reactions : MonoBehaviour
{
    public void DestroyReactions() {
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }
    }
}
