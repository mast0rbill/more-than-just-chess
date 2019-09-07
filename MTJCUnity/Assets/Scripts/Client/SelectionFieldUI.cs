using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionFieldUI : MonoBehaviour {


    // Selection field buttons
    public GameObject spawnPawn;
    public GameObject healKing;
    public GameObject upgradePawn1;
    public GameObject upgradePawn2;
    public GameObject upgradePawn3;
    public GameObject upgradePawn4;
    public GameObject upgradeDef;
    public GameObject upgradeAtt;
    public Vector3 enabledPos;
    public Vector3 disabledPos;
    public float enabledXMin;
    public float enabledXMax;
    public GameObject canMoveIndicatorPrefab;
    public bool[,] currentPossibleMovements;
    public Material moveIndicatorMat;
    public Material attackIndicatorMat;

    private RectTransform trans;
    private bool menuEnabled = false;
    private List<GameObject> canMoveIndicators;

    void Awake() {
        trans = GetComponent<RectTransform>();
        canMoveIndicators = new List<GameObject>();
    }

    void Update() {
        if(menuEnabled) {
            trans.anchorMin = new Vector2(Mathf.Lerp(trans.anchorMin.x, enabledXMin, Time.deltaTime * 10f), trans.anchorMin.y);
            trans.anchorMax = new Vector2(Mathf.Lerp(trans.anchorMax.x, enabledXMax, Time.deltaTime * 10f), trans.anchorMax.y);
        } else {
            trans.anchorMin = new Vector2(Mathf.Lerp(trans.anchorMin.x, enabledXMin - 0.2f, Time.deltaTime * 10f), trans.anchorMin.y);
            trans.anchorMax = new Vector2(Mathf.Lerp(trans.anchorMax.x, enabledXMax - 0.2f, Time.deltaTime * 10f), trans.anchorMax.y);
        }
    }

    public void EnableMenu(Vector2Int tileCoord, int mana) {
        menuEnabled = true;

        GeneralManager.GamePiece piece = GeneralManager.gameBoard.GetPieceAtPosition(tileCoord);

        if(piece == null || piece.team == ClientManager.team) {
            if(piece == null) {
                // Nothing on this tile
                if((ClientManager.team && tileCoord.y == 0) || (!ClientManager.team && tileCoord.y == 7)) {
                    spawnPawn.SetActive(true);
                }

                spawnPawn.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_0);
            }
            else {
                if(piece.piece == GeneralManager.GamePieceEnum.Pawn) {
                    upgradePawn1.SetActive(true);
                    upgradePawn2.SetActive(true);
                    upgradePawn3.SetActive(true);
                    upgradePawn4.SetActive(true);

                    upgradePawn1.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_2_1);
                    upgradePawn2.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_2_2);
                    upgradePawn3.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_2_3);
                    upgradePawn4.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_2_4);
                } else {
                    if(piece.piece != GeneralManager.GamePieceEnum.King) {
                        upgradeAtt.SetActive(true);
                        upgradeDef.SetActive(true);

                        upgradeAtt.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_5);
                        upgradeDef.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_4);
                    } else {
                        healKing.SetActive(true);

                        healKing.GetComponent<Button>().interactable = (mana >= GeneralManager.MANA_COST_3);
                    }
                }

                // Possible movements
                int cost = (piece.piece == GeneralManager.GamePieceEnum.Pawn) ? GeneralManager.MANA_COST_1_0 : GeneralManager.MANA_COST_1_1;

                if(!piece.frozen && mana >= cost) {
                    // Movement indicator
                    currentPossibleMovements = GeneralManager.gameBoard.GetAvailableMovePositions(ClientManager.team, piece);

                    for(int y = 0; y < GeneralManager.GAME_BOARD_SIZE; y++) {
                        for(int x = 0; x < GeneralManager.GAME_BOARD_SIZE; x++) {
                            if(currentPossibleMovements[x, y]) {
                                GameObject indicator = GetPooledMoveIndicator();

                                if(indicator != null) {
                                    indicator.transform.position = GeneralManager.GetTileCenterPos(new Vector2Int(x, y));
                                    indicator.SetActive(true);
                                }
                                else {
                                    indicator = Instantiate(canMoveIndicatorPrefab, GeneralManager.GetTileCenterPos(new Vector2Int(x, y)), Quaternion.Euler(new Vector3(-90f, 0f, 0f)));
                                    canMoveIndicators.Add(indicator);
                                }

                                GeneralManager.GamePiece pAtPos = GeneralManager.gameBoard.GetPieceAtPosition(new Vector2Int(x, y));
                                if(pAtPos != null && pAtPos.team != ClientManager.team) {
                                    indicator.GetComponent<MoveIndicator>().renderer.material = attackIndicatorMat;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void DisableMenu() {
        menuEnabled = false;

        spawnPawn.SetActive(false);
        healKing.SetActive(false);

        upgradePawn1.SetActive(false);
        upgradePawn2.SetActive(false);
        upgradePawn3.SetActive(false);
        upgradePawn4.SetActive(false);

        upgradeAtt.SetActive(false);
        upgradeDef.SetActive(false);

        foreach(GameObject go in canMoveIndicators) {
            go.GetComponent<MoveIndicator>().renderer.material = moveIndicatorMat;
            go.SetActive(false);
        }

        currentPossibleMovements = new bool[GeneralManager.GAME_BOARD_SIZE, GeneralManager.GAME_BOARD_SIZE];
    }

    // Button presses
    public void SpawnPawnButton() {
        ClientManager.instance.InvokeAction(0, Vector2Int.zero);
    }

    public void UpgradePawnButton(int newID) {
        ClientManager.instance.InvokeAction(2, Vector2Int.zero, newID);
    }

    public void HealKingButton() {
        ClientManager.instance.InvokeAction(3, Vector2Int.zero);
    }

    public void UpgradeDefenseButton() {
        ClientManager.instance.InvokeAction(4, Vector2Int.zero);
    }

    public void UpgradeDamageButton() {
        ClientManager.instance.InvokeAction(5, Vector2Int.zero);
    }

    private GameObject GetPooledMoveIndicator() {
        foreach(GameObject go in canMoveIndicators) {
            if(!go.activeInHierarchy) {
                return go;
            }
        }

        return null;
    }
}
