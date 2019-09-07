using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour 
{
    private void Awake() {
        GeneralManager.isClient = true;
    }

	public void StartSingleplayer()
	{
        SceneManager.LoadScene("Main Game Scene");
    }

    public void StartMultiplayer() {
        /*BoltLauncher.StartClient();
        string address = "localhost";
        UdpKit.UdpIPv4Address ad = UdpKit.UdpIPv4Address.Parse(address);
        BoltNetwork.Connect(new UdpKit.UdpEndPoint(ad, 25001));
        StartCoroutine(JoinServerRoutine());*/
    }

    /*private IEnumerator JoinServerRoutine() {
        float time = Time.time;
        bool flag = false;

        if(!BoltNetwork.isConnected) {
            if(Time.time - time >= 10000f) {
                Debug.LogError("Unable to connect.");
                flag = true;
            }
            else {
                yield return null;
            }
        }

        if(!flag)
            SceneManager.LoadScene("Main Game Scene");
    }*/
}
