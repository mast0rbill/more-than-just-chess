using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClientTurn {
    public List<ClientAction> actions = new List<ClientAction>();
}

public class ServerManager {//: Bolt.GlobalEventListener {
    public const int BOARD_SIDE_LENGTH = 8;


    // Represents a match going through the server
    public class GameInstance {
        
    }

    public List<GameInstance> currentInstances = new List<GameInstance>();

    void Awake() {
        StartServer();
    }

    public void StartServer() {
        //BoltConfig config = BoltRuntimeSettings.instance.GetConfigCopy();

        //BoltLauncher.StartServer(25001, config);
    }

    //public override void Connected(BoltConnection connection) {
        
    //}
}
