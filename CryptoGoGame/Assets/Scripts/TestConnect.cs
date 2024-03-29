using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TestConnect : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting to the server ");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = MasterManager.GameSettings.NickName;
        PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
        PhotonNetwork.ConnectUsingSettings();
        
        
    }

    public override void OnConnectedToMaster () {
        Debug.Log("OnConnectedToMaster() was called by PUN ");
        Debug.Log(PhotonNetwork.LocalPlayer.NickName);
     //   PhotonNetwork.JoinRandomRoom();
        if(!PhotonNetwork.InLobby){
            PhotonNetwork.JoinLobby();

        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from sever for reason " + cause.ToString());
    }

}
