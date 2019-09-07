using UnityEngine;
using System.IO;

public class SerializationHandler {

    public byte[] Serialize(ClientTurn turn) {
        byte[] serialized = new byte[0];

        using(MemoryStream stream = new MemoryStream(serialized)) {
            stream.WriteByte((byte)turn.actions.Count);

            foreach(ClientAction action in turn.actions) {
                stream.WriteByte(action.actionIndex); // 0 = Spawn Pawn, 1 = Move/Attack, 2 = Upgrade, 3 = Heal;
                WriteVector2Int(stream, action.actionFromPos);

                // 0, 2, need new ID | 1 needs actionToPos
                if(action.actionIndex == 0 || action.actionIndex == 2) {
                    stream.WriteByte(action.newID);
                } else if(action.actionIndex == 1) {
                    WriteVector2Int(stream, action.actionToPos);
                }
            }
        }

        return serialized;
    }

    public ClientTurn Parse(byte[] bytes) {
        ClientTurn returned = new ClientTurn();

        using(MemoryStream stream = new MemoryStream(bytes)) {
            int actions = stream.ReadByte();
            returned.actions = new System.Collections.Generic.List<ClientAction>(actions);

            for(int i = 0; i < actions; i++) {
                ClientAction action = new ClientAction();

                action.actionIndex = (byte)stream.ReadByte();
                action.actionFromPos = ReadVector2Int(stream);

                if(action.actionIndex == 0 || action.actionIndex == 2) {
                    action.newID = (byte)stream.ReadByte();
                } else if(action.actionIndex == 1) {
                    action.actionToPos = ReadVector2Int(stream);
                }

                returned.actions.Add(action);
            }
        }

        return returned;
    }

    private void WriteVector2Int(MemoryStream stream, Vector2Int v) {
        stream.WriteByte((byte)v.x);
        stream.WriteByte((byte)v.y);
    }

    private Vector2Int ReadVector2Int(MemoryStream stream) {
        int x = stream.ReadByte();
        int y = stream.ReadByte();

        return new Vector2Int(x, y);
    }

}
