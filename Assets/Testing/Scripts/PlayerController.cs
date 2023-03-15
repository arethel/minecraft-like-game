using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    

    private Rigidbody playerRigidbody;
    private PlayerInput playerInput;
    
    [SerializeField]
    private GameObject firstPersonCamera;
    
    private PlayerControls playerControls;

    private void Awake() {
        playerRigidbody = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        
        playerControls = new PlayerControls();
        
        playerControls.Player.Enable();
        playerControls.Player.Jump.performed += Jump;
        
    }
    
    private void FixedUpdate() {
        
        Move(playerControls.Player.Move.ReadValue<Vector2>());
        
    }
    
    public void Move(Vector2 inputValue){
        inputValue *= 10f;
        gameObject.transform.Rotate(new Vector3(firstPersonCamera.transform.rotation.x, firstPersonCamera.transform.rotation.y, firstPersonCamera.transform.rotation.z));
        firstPersonCamera.transform.rotation = new Quaternion(0, 0, 0, 0);
        playerRigidbody.velocity = gameObject.transform.InverseTransformVector(new Vector3(inputValue.x, playerRigidbody.velocity.y, inputValue.y));
    }
    
    
    
    public void Jump(InputAction.CallbackContext context){
        if (!context.performed) return;

        playerRigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
    }
    
    
    
    
}
