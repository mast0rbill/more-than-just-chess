using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaFieldUI : MonoBehaviour {

    public GameObject[] blueMana;
    public GameObject[] yellowMana;

	public void UpdateManaVisuals(int mana) {
        for(int i = 0; i < GeneralManager.MAX_MANA + yellowMana.Length; i++) {
            if(i < mana) {
                if(i < GeneralManager.MAX_MANA) {
                    blueMana[i].SetActive(true);
                } else {
                    yellowMana[i - GeneralManager.MAX_MANA].SetActive(true);
                }
            } else {
                if(i < GeneralManager.MAX_MANA) {
                    blueMana[i].SetActive(false);
                } else {
                    yellowMana[i - GeneralManager.MAX_MANA].SetActive(false);
                }
            }
        }        
    }
}
