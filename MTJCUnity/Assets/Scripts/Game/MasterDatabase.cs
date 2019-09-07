using UnityEngine;

public class MasterDatabase : MonoBehaviour {

    private static MasterDatabase instance;

    public static GameObject GetVisualsByID(int pieceID, bool team) {
        if(instance == null) {
            instance = ((GameObject)Resources.Load("Master Database")).GetComponent<MasterDatabase>();
        }

        if(team)
            return instance.gamePieceVisualsWhite[pieceID];

        return instance.gamePieceVisualsBlack[pieceID];
    }

    public GameObject[] gamePieceVisualsWhite;
    public GameObject[] gamePieceVisualsBlack;

}
