using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using QFSW.QC;

public class Commands : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    [Command]
    private void StartServer(){
        NetworkManager.Singleton.StartServer();
    }
    [Command]
    private void StartClient(){
        NetworkManager.Singleton.StartClient();
    }
    [Command]
    private void StartHost(){
        NetworkManager.Singleton.StartHost();
    }
}
