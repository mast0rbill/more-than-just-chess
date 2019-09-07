using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowingWater : MonoBehaviour {

    public Transform[] waterPieces;
    public float teleportThreshold = 1f;
    public float moveSpeed = 10f;

    public List<Transform> queue;

    void Start() {
        queue = new List<Transform>();

        foreach(Transform t in waterPieces) {
            queue.Add(t);
        }
    }
        
    void Update() {
        for(int i = queue.Count - 1; i >= 0; i--) {
            queue[i].position += new Vector3(moveSpeed * Time.deltaTime, 0f, 0f);

            if(i == 0) {
                if(queue[i].position.x >= teleportThreshold) {
                    queue[i].position += new Vector3(-teleportThreshold * 2f, 0f, 0f);
                    queue.Add(queue[i]);
                    queue.RemoveAt(0);
                }
            }
        }
    }
}
