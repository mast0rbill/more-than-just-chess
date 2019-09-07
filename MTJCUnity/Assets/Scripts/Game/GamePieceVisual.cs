using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePieceVisual : MonoBehaviour {
    public RectTransform canvas;
    public Text attack;
    public Text defense;
    public GameObject frozenIndicator;

    public void UpdateVisual(bool team, int defense, int attack, bool frozen) {
        if(team) {
            canvas.rotation = Quaternion.Euler(-45f, 0f, 0f);
        } else {
            canvas.rotation = Quaternion.Euler(45f, 0f, 180f);
        }

        // set texts here
        this.attack.text = "" + attack;
        this.defense.text = "" + defense;
        frozenIndicator.SetActive(frozen);
    }
}
