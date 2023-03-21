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
    
    [SerializeField]
    private NetworkManager netManager;
    
    [Command]
    private void StartServer(){
        netManager.StartServer();
    }
    [Command]
    private void StartClient(){
        netManager.StartClient();
    }
    [Command]
    private void StartHost(){
        netManager.StartHost();
    }
}
