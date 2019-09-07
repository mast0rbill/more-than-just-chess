using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientAction {
    public byte actionIndex; // 0 = Spawn Pawn, 1 = Move/Attack, 2 = Upgrade, 3 = Heal, 4 = Upgrade Defense, 5 = Upgrade Damage;
    public Vector2Int actionFromPos;
    public Vector2Int actionToPos;
    public byte newID; // Corresponds to game piece ID.
}

public class GeneralManager {
    public const int MAX_MANA = 10;

    public const int MANA_COST_0 = 5; // Spawn pawn
    public const int MANA_COST_1_0 = 3; // Move/attack w/ pawn
    public const int MANA_COST_1_1 = 0; // Move/attack w/ other
    public const int MANA_COST_2_1 = 12; // Upgrade pawn to Queen
    public const int MANA_COST_2_2 = 7; // Upgrade Pawn to Knight
    public const int MANA_COST_2_3 = 7; // Upgrade Pawn to Bishop
    public const int MANA_COST_2_4 = 7; // Upgrade Pawn to Rook
    public const int MANA_COST_3 = 7; // Heal King
    public const int MANA_COST_4 = 7; // Upgrade denfense
    public const int MANA_COST_5 = 7; // Upgrade attack

    // Mana boost tiles
    public const int MANA_BOOST_X1 = 1;
    public const int MANA_BOOST_Y1 = 3;
    public const int MANA_BOOST_X2 = 6;
    public const int MANA_BOOST_Y2 = 4;

    public enum GamePieceEnum {
        King = 0, Queen = 1, Knight = 2, Bishop = 3, Rook = 4, Pawn = 5
    }

    [System.Serializable]
    public class GamePiece {
        public GamePieceEnum piece = GamePieceEnum.King;
        public Vector2Int position;
        public bool team = false; // false = black, true = white
        public GamePieceVisual visual; // Null for server
        public int health, attack;
        public bool frozen = true;

        public GamePiece(GamePieceEnum p, Vector2Int pos, bool t) {
            piece = p;
            health = 1;
            attack = 1;
            position = pos;
            team = t;

            if(p == GamePieceEnum.King) {
                health = 5;
                attack = 3;
            }
        }
    }

    [System.Serializable]
    public class Board {
        public List<GamePiece> whitePieces;
        public List<GamePiece> blackPieces;

        public Board() {
            whitePieces = new List<GamePiece>();
            blackPieces = new List<GamePiece>();

            GamePiece whiteKing = new GamePiece(GamePieceEnum.King, new Vector2Int(4, 0), true);
            whiteKing.frozen = false;
            whitePieces.Add(whiteKing);

            GamePiece blackKing = new GamePiece(GamePieceEnum.King, new Vector2Int(3, 7), false);
            blackKing.frozen = false;
            blackPieces.Add(blackKing);
        }

        public GamePiece GetPieceAtPosition(Vector2Int position) {
            foreach(GamePiece p in whitePieces) {
                if(p.position == position) {
                    return p;
                }
            }

            foreach(GamePiece p in blackPieces) {
                if(p.position == position) {
                    return p;
                }
            }

            return null;
        }

        public void CreatePawn(bool team, Vector2Int position) {
            GamePiece newPawn = new GamePiece(GamePieceEnum.Pawn, position, team);

            if(team) {
                whitePieces.Add(newPawn);
            } else {
                blackPieces.Add(newPawn);
            }
        }

        public void RemovePiece(GamePiece p) {
            Object.Destroy(p.visual.gameObject);

            if(p.team) {
                whitePieces.Remove(p);
            } else {
                blackPieces.Remove(p);
            }
        }

        public bool[,] GetAvailableMovePositions(bool team, GamePiece piece) {
            bool[,] board = new bool[8, 8];

            Vector2Int piecePos = piece.position;
            if(piece.piece == GamePieceEnum.Pawn) { // Pawn
                if(PositionIsAvailable(team, piecePos + new Vector2Int(-1, 0), piece.attack, piece) > 0)
                    board[piecePos.x - 1, piecePos.y] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(1, 0), piece.attack, piece) > 0)
                    board[piecePos.x + 1, piecePos.y] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(0, -1), piece.attack, piece) > 0)
                    board[piecePos.x, piecePos.y - 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(0, 1), piece.attack, piece) > 0)
                    board[piecePos.x, piecePos.y + 1] = true;

            } else if(piece.piece == GamePieceEnum.Rook) { // Rook
                int p;

                // Right
                for(int i = piecePos.x + 1; i < GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(i, piecePos.y), piece.attack, piece);

                    if(p > 0) {
                        board[i, piecePos.y] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Left
                for(int i = piecePos.x - 1; i >= 0; i--) {
                    p = PositionIsAvailable(team, new Vector2Int(i, piecePos.y), piece.attack, piece);

                    if(p > 0) {
                        board[i, piecePos.y] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }
                
                // Up
                for(int i = piecePos.y + 1; i < GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x, i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x, i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Down
                for(int i = piecePos.y - 1; i >= 0; i--) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x, i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x, i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }
            } else if(piece.piece == GamePieceEnum.Knight) { // Knight
                if(PositionIsAvailable(team, piecePos + new Vector2Int(-1, 2), piece.attack, piece) > 0)
                    board[piecePos.x - 1, piecePos.y + 2] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(1, 2), piece.attack, piece) > 0)
                    board[piecePos.x + 1, piecePos.y + 2] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(-2, 1), piece.attack, piece) > 0)
                    board[piecePos.x - 2, piecePos.y + 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(2, 1), piece.attack, piece) > 0)
                    board[piecePos.x + 2, piecePos.y + 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(-1, -2), piece.attack, piece) > 0)
                    board[piecePos.x - 1, piecePos.y - 2] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(1, -2), piece.attack, piece) > 0)
                    board[piecePos.x + 1, piecePos.y - 2] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(-2, -1), piece.attack, piece) > 0)
                    board[piecePos.x - 2, piecePos.y - 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(2, -1), piece.attack, piece) > 0)
                    board[piecePos.x + 2, piecePos.y - 1] = true;
            } else if(piece.piece == GamePieceEnum.Bishop) {
                int p;

                // Right up
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x + i, piecePos.y + i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x + i, piecePos.y + i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Left up
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x - i, piecePos.y + i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x - i, piecePos.y + i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Right down
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x + i, piecePos.y - i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x + i, piecePos.y - i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Left down
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x - i, piecePos.y - i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x - i, piecePos.y - i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }
            } else if(piece.piece == GamePieceEnum.Queen) {
                int p;

                // Right
                for(int i = piecePos.x + 1; i < GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(i, piecePos.y), piece.attack, piece);

                    if(p > 0) {
                        board[i, piecePos.y] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Left
                for(int i = piecePos.x - 1; i >= 0; i--) {
                    p = PositionIsAvailable(team, new Vector2Int(i, piecePos.y), piece.attack, piece);

                    if(p > 0) {
                        board[i, piecePos.y] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Up
                for(int i = piecePos.y + 1; i < GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x, i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x, i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Down
                for(int i = piecePos.y - 1; i >= 0; i--) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x, i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x, i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Right up
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x + i, piecePos.y + i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x + i, piecePos.y + i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Left up
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x - i, piecePos.y + i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x - i, piecePos.y + i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Right down
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x + i, piecePos.y - i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x + i, piecePos.y - i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }

                // Left down
                for(int i = 1; i <= GAME_BOARD_SIZE; i++) {
                    p = PositionIsAvailable(team, new Vector2Int(piecePos.x - i, piecePos.y - i), piece.attack, piece);

                    if(p > 0) {
                        board[piecePos.x - i, piecePos.y - i] = true;

                        if(p == 2)
                            break;
                    }
                    else
                        break;
                }
            } else if(piece.piece == GamePieceEnum.King) {
                if(PositionIsAvailable(team, piecePos + new Vector2Int(-1, 0), piece.attack, piece) > 0)
                    board[piecePos.x - 1, piecePos.y] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(1, 0), piece.attack, piece) > 0)
                    board[piecePos.x + 1, piecePos.y] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(0, -1), piece.attack, piece) > 0)
                    board[piecePos.x, piecePos.y - 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(0, 1), piece.attack, piece) > 0)
                    board[piecePos.x, piecePos.y + 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(-1, -1), piece.attack, piece) > 0)
                    board[piecePos.x - 1, piecePos.y - 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(1, -1), piece.attack, piece) > 0)
                    board[piecePos.x + 1, piecePos.y - 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(-1, 1), piece.attack, piece) > 0)
                    board[piecePos.x - 1, piecePos.y + 1] = true;

                if(PositionIsAvailable(team, piecePos + new Vector2Int(1, 1), piece.attack, piece) > 0)
                    board[piecePos.x + 1, piecePos.y + 1] = true;
            }

            return board;
        }

        public bool PositionIsEmpty(Vector2Int pos) {
            foreach(GamePiece piece in whitePieces) {
                if(piece.position == pos) {
                    return false;
                }
            }

            foreach(GamePiece piece in blackPieces) {
                if(piece.position == pos) {
                    return false;
                }
            }

            return true;
        }

        private int PositionIsAvailable(bool team, Vector2Int pos, int attack, GamePiece myPiece) {
            if(pos.x < 0 || pos.x > GAME_BOARD_SIZE - 1 || pos.y < 0 || pos.y > GAME_BOARD_SIZE - 1)
                return 0;

            foreach(GamePiece piece in whitePieces) {
                if(piece.position == pos) {
                    if(!team) {
                        if(piece.piece == GamePieceEnum.King) {
                            if(myPiece.piece != GamePieceEnum.Knight) {
                                return 2;
                            } else {
                                Vector2Int diff = piece.position - myPiece.position;
                                int newX = diff.x == 0 ? 0 : diff.x / Mathf.Abs(diff.x);
                                int newY = diff.y == 0 ? 0 : diff.y / Mathf.Abs(diff.y);

                                if(PositionIsEmpty(pos - new Vector2Int(newX, 0)) || PositionIsEmpty(pos - new Vector2Int(0, newY))) {
                                    return 2;
                                }

                                return 0;
                            }
                        }
                        else {
                            if(piece.health <= attack)
                                return 2;
                            else
                                return 0;
                        }
                    } else {
                        return 0;
                    }
                }
            }

            foreach(GamePiece piece in blackPieces) {
                if(piece.position == pos) {
                    if(team) {
                        if(piece.piece == GamePieceEnum.King) {
                            if(myPiece.piece != GamePieceEnum.Knight) {
                                return 2;
                            }
                            else {
                                Vector2Int diff = piece.position - myPiece.position;
                                int newX = diff.x == 0 ? 0 : diff.x / Mathf.Abs(diff.x);
                                int newY = diff.y == 0 ? 0 : diff.y / Mathf.Abs(diff.y);

                                if(PositionIsEmpty(pos - new Vector2Int(newX, 0)) || PositionIsEmpty(pos - new Vector2Int(0, newY))) {
                                    return 2;
                                }

                                return 0;
                            }
                        }
                        else {
                            if(piece.health <= attack)
                                return 2;
                            else
                                return 0;
                        }
                    }
                    else {
                        return 0;
                    }
                }
            }

            return 1;
        }
    }

    public static bool isClient = false;
    public static bool isServer = false;
    public static Board gameBoard = new Board();

    public static void NextTurn(bool newTeamTurn) {
        if(newTeamTurn) {
            foreach(GamePiece p in gameBoard.whitePieces) {
                p.frozen = false;
            }
        } else {
            foreach(GamePiece p in gameBoard.blackPieces) {
                p.frozen = false;
            }
        }
    }

    public static void ExecuteAction(bool t, ClientAction action) {
        GamePiece pieceAtPos = gameBoard.GetPieceAtPosition(action.actionFromPos);

        switch(action.actionIndex) {
            case 0:
                if(pieceAtPos == null) {
                    gameBoard.CreatePawn(t, action.actionFromPos);
                }

                break;
            case 1:
                if(pieceAtPos != null) {
                    GamePiece pieceAtNewPos = gameBoard.GetPieceAtPosition(action.actionToPos);

                    if(pieceAtNewPos == null)
                        pieceAtPos.position = action.actionToPos;
                    else {
                        if(pieceAtNewPos.team != t) {
                            if(pieceAtNewPos.piece != GamePieceEnum.King) {
                                // Eat
                                gameBoard.RemovePiece(pieceAtNewPos);
                                pieceAtPos.position = action.actionToPos;
                            } else {
                                // Damage King
                                pieceAtNewPos.health -= pieceAtPos.attack;

                                if(pieceAtNewPos.health > 0) {
                                    if(pieceAtPos.piece != GamePieceEnum.Knight) {
                                        Vector2Int diff = pieceAtNewPos.position - pieceAtPos.position;
                                        int newX = diff.x == 0 ? 0 : diff.x / Mathf.Abs(diff.x);
                                        int newY = diff.y == 0 ? 0 : diff.y / Mathf.Abs(diff.y);
                                        diff = new Vector2Int(newX, newY);

                                        pieceAtPos.position = pieceAtNewPos.position - diff;
                                    } else {
                                        // Knight special mechanics
                                        Vector2Int diff = pieceAtNewPos.position - pieceAtPos.position;
                                        bool dominantX = Mathf.Abs(diff.x) > 1;
                                        int newX = diff.x == 0 ? 0 : diff.x / Mathf.Abs(diff.x);
                                        int newY = diff.y == 0 ? 0 : diff.y / Mathf.Abs(diff.y);

                                        if(dominantX) {
                                            Vector2Int pref = pieceAtNewPos.position - new Vector2Int(newX, 0);
                                            if(gameBoard.PositionIsEmpty(pref)) {
                                                pieceAtPos.position = pref;
                                            } else {
                                                pieceAtPos.position = pieceAtNewPos.position - new Vector2Int(0, newY);
                                            }
                                        } else {
                                            Vector2Int pref = pieceAtNewPos.position - new Vector2Int(0, newY);
                                            if(gameBoard.PositionIsEmpty(pref)) {
                                                pieceAtPos.position = pref;
                                            }
                                            else {
                                                pieceAtPos.position = pieceAtNewPos.position - new Vector2Int(newX, 0);
                                            }
                                        }
                                    }
                                } else {
                                    gameBoard.RemovePiece(pieceAtNewPos);
                                    pieceAtPos.position = action.actionToPos;

                                    EndGame(t);
                                }
                            }
                        }
                    }

                    pieceAtPos.frozen = true;
                }

                break;
            case 2:
                if(pieceAtPos != null && pieceAtPos.piece == GamePieceEnum.Pawn) {
                    pieceAtPos.piece = (GamePieceEnum)action.newID;
                    pieceAtPos.frozen = true;
                }

                break;

            case 3:
                if(pieceAtPos != null && pieceAtPos.piece == GamePieceEnum.King) {
                    pieceAtPos.health++;

                    if(pieceAtPos.health > 5) {
                        pieceAtPos.health = 5;
                    }
                }

                break;
            case 4:
                if(pieceAtPos != null && pieceAtPos.piece != GamePieceEnum.King) {
                    pieceAtPos.health++;

                    if(pieceAtPos.health > 3) {
                        pieceAtPos.health = 3;
                    }
                }

                break;
            case 5:
                if(pieceAtPos != null && pieceAtPos.piece != GamePieceEnum.King) {
                    pieceAtPos.attack++;

                    if(pieceAtPos.attack > 3) {
                        pieceAtPos.attack = 3;
                    }
                }

                break;
        }
    }

    private static void EndGame(bool winnerTeam) {
        if(isClient) {
            // PLACEHOLDER
            if(winnerTeam) {
                Debug.LogError("WHITE WON");
            }
            else {
                Debug.LogError("BLACK WON");
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
        }
    }

    public static Vector3 GetTileCenterPos(Vector2Int boardCoords) {
        return new Vector3((boardCoords.x + 0.5f) * 0.125f - 0.5f, (boardCoords.y + 0.5f) * 0.125f - 0.5f, 0f);
    }

    public const int GAME_BOARD_SIZE = 8;
}
