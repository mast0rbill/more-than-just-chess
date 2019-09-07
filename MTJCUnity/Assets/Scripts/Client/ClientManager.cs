using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour {

    public static ClientManager instance;
    public static bool team = true; // false = black, true = white 
    public static bool isSingleplayer = true;

    public Camera cam;
    public Transform hoverPlane;
    public Transform selectorPlane;
    public RectTransform selectionTrans;
    public SelectionFieldUI selectionFieldUI;
    public ManaFieldUI manaFieldUI;

    private Vector2Int curHovered;
    private Vector2Int curSelected;
    private RaycastHit raycastHit;
    private bool moveHover = false;
    private int currentMana = 10;

    private bool hasSelected {
        get {
            return curSelected != new Vector2Int(-1, -1);
        }
    }

    void Awake() {
        if(instance == null) {
            instance = this;
        } else {
            Debug.LogError("There should only be one client manager!");
        }

        curHovered = new Vector2Int(-1, -1);
        curSelected = new Vector2Int(-1, -1);
        hoverPlane.position = new Vector3(0f, -100f, 0f);
        selectorPlane.position = new Vector3(0f, -100f, 0f);
        GeneralManager.isClient = true;
    }

    void Start() {
        GeneralManager.gameBoard = new GeneralManager.Board();
        UpdateBoardVisuals();
    }
 
	void Update() {
        UpdateSelection();

        if(!RectTransformUtility.RectangleContainsScreenPoint(selectionTrans, Input.mousePosition)) {
            if(Input.GetMouseButtonUp(0)) {
                // Hovered
                if(!hasSelected) {
                    if(curHovered != new Vector2Int(-1, -1)) {
                        curSelected = curHovered;
                        selectorPlane.position = new Vector3((curSelected.x + 0.5f) * 0.125f - 0.5f, (curSelected.y + 0.5f) * 0.125f - 0.5f, raycastHit.point.z);

                        // Update selection menu here
                        selectionFieldUI.EnableMenu(curSelected, currentMana);
                    }
                }
                else {
                    if(moveHover) {
                        InvokeAction(1, curHovered);
                    }
                    else {
                        curSelected = new Vector2Int(-1, -1);
                        selectorPlane.position = new Vector3(0f, -100f, 0f);
                        selectionFieldUI.DisableMenu();
                    }
                }
            }
        }
    }

    public void EndTurn() {
        // TEMP. not true for multiplayer later.
        if(isSingleplayer) {
            StartNextTurn();
        }
    }

    public void StartNextTurn() {
        if(isSingleplayer)
            team = !team;

        currentMana = GeneralManager.MAX_MANA;

        GeneralManager.GamePiece p1 = GeneralManager.gameBoard.GetPieceAtPosition(new Vector2Int(GeneralManager.MANA_BOOST_X1, GeneralManager.MANA_BOOST_Y1));
        GeneralManager.GamePiece p2 = GeneralManager.gameBoard.GetPieceAtPosition(new Vector2Int(GeneralManager.MANA_BOOST_X2, GeneralManager.MANA_BOOST_Y2));

        if(p1 != null && p1.team == team) {
            currentMana += 2;
        }

        if(p2 != null && p2.team == team) {
            currentMana += 2;
        }

        GeneralManager.NextTurn(team);
        UpdateBoardVisuals();
    }

    private void UpdateSelection() {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        moveHover = false;

        // Bounds
        if(Physics.Raycast(ray, out raycastHit) && raycastHit.point.x >= -0.5f && raycastHit.point.x <= 0.5f && raycastHit.point.y >= -0.5f && raycastHit.point.y <= 0.5f) {
            int x = (int)Mathf.Floor((raycastHit.point.x + 0.5f) * 8f);
            int y = (int)Mathf.Floor((raycastHit.point.y + 0.5f) * 8f);
            curHovered = new Vector2Int(x, y);

            if(!hasSelected || selectionFieldUI.currentPossibleMovements[curHovered.x, curHovered.y]) {
                hoverPlane.position = new Vector3((x + 0.5f) * 0.125f - 0.5f, (y + 0.5f) * 0.125f - 0.5f, raycastHit.point.z);
                moveHover = true;
            } else {
                hoverPlane.position = new Vector3(0f, -100f, 0f);
                curHovered = new Vector2Int(-1, -1);
            }
        }
        else {
            hoverPlane.position = new Vector3(0f, -100f, 0f);
            curHovered = new Vector2Int(-1, -1);
        }
    }

    public void UpdateBoardVisuals() {
        // Clear old visuals
        foreach(GeneralManager.GamePiece piece in GeneralManager.gameBoard.whitePieces) {
            if(piece.visual != null)
                Destroy(piece.visual.gameObject);
        }

        foreach(GeneralManager.GamePiece piece in GeneralManager.gameBoard.blackPieces) {
            if(piece.visual != null)
                Destroy(piece.visual.gameObject);
        }

        foreach(GeneralManager.GamePiece piece in GeneralManager.gameBoard.whitePieces) {
            piece.visual = Instantiate(MasterDatabase.GetVisualsByID((int)piece.piece, true), GeneralManager.GetTileCenterPos(piece.position), Quaternion.identity).GetComponent<GamePieceVisual>();
            piece.visual.UpdateVisual(team, piece.health, piece.attack, piece.frozen);
        }

        foreach(GeneralManager.GamePiece piece in GeneralManager.gameBoard.blackPieces) {
            piece.visual = Instantiate(MasterDatabase.GetVisualsByID((int)piece.piece, false), GeneralManager.GetTileCenterPos(piece.position), Quaternion.identity).GetComponent<GamePieceVisual>();
            piece.visual.UpdateVisual(team, piece.health, piece.attack, piece.frozen);
        }

        manaFieldUI.UpdateManaVisuals(currentMana);

        // Unselect
        curSelected = new Vector2Int(-1, -1);
        selectorPlane.position = new Vector3(0f, -100f, 0f);
        selectionFieldUI.DisableMenu();
    }

    public void InvokeAction(byte actionID, Vector2Int actionToPos, int newID = -1) {
        int manaCost = 0;

        if(actionID == 0) {
            manaCost = GeneralManager.MANA_COST_0;
        } else if(actionID == 1) {
            GeneralManager.GamePiece myPiece = GeneralManager.gameBoard.GetPieceAtPosition(curSelected);

            if(myPiece.piece == GeneralManager.GamePieceEnum.Pawn) {
                manaCost = GeneralManager.MANA_COST_1_0;
            } else {
                manaCost = GeneralManager.MANA_COST_1_1;
            }
        } else if(actionID == 2) {
            if(newID == 1) {
                manaCost = GeneralManager.MANA_COST_2_1;
            }
            else if(newID == 2) {
                manaCost = GeneralManager.MANA_COST_2_2;
            }
            else if(newID == 3) {
                manaCost = GeneralManager.MANA_COST_2_3;
            }
            else if(newID == 4) {
                manaCost = GeneralManager.MANA_COST_2_4;
            }
        }
        else if(actionID == 3) {
            manaCost = GeneralManager.MANA_COST_3;
        }
        else if(actionID == 4) {
            manaCost = GeneralManager.MANA_COST_4;
        }
        else if(actionID == 5) {
            manaCost = GeneralManager.MANA_COST_5;
        }

        if(manaCost <= currentMana) {
            ClientAction newAction = new ClientAction();
            newAction.actionIndex = actionID;
            newAction.actionFromPos = curSelected;
            newAction.actionToPos = actionToPos;
            newAction.newID = (byte)newID;

            // Temp
            GeneralManager.ExecuteAction(team, newAction);
            currentMana -= manaCost;
            UpdateBoardVisuals();
        }
    }
}
