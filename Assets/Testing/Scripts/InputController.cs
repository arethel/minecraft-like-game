using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{

    public static InputController Singleton;

    public enum Modes{
        PLayer,
        Menu,
        Chat
    }
    
    public PlayerControls playerControls;
    
    private void Awake() {
        Singleton = this;
        playerControls = new PlayerControls();
    }
    
    
    public void SetModeOfInput(Modes mode){
        switch(mode){
            case Modes.PLayer:
                playerControls.Player.Enable();
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case Modes.Menu:

                break;
            case Modes.Chat:

                break;
        }
    }
    

}
