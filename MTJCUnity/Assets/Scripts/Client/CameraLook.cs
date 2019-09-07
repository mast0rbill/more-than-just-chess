using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLook : MonoBehaviour {

    public float degreesPerSec = 25f;
    public Transform cameraPivot;
    public Vector3 whiteRot;
    public Vector3 blackRot;

    void Update() {
        if(ClientManager.team) {
            cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, Quaternion.Euler(whiteRot), Time.deltaTime * 10f);
        } else {
            cameraPivot.rotation = Quaternion.Slerp(cameraPivot.rotation, Quaternion.Euler(blackRot), Time.deltaTime * 10f);
        }
    }
}
