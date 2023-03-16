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
    
    private void LateUpdate() {
        CameraRotation();
    }


    public float sensitivity = 0.1f;

    float yView = 0;
    public void CameraRotation(){

        Vector2 lookImput = playerControls.Player.Look.ReadValue<Vector2>();

        firstPersonCamera.transform.Rotate(Vector3.up * lookImput.x * sensitivity);

        yView += -lookImput.y * sensitivity;
        Vector3 angels = firstPersonCamera.transform.eulerAngles;
        
        firstPersonCamera.transform.eulerAngles = new Vector3(Mathf.Clamp(yView, -80f, 80f), angels.y, 0f);


    }


    float maxForce = 2;
    public void Move(Vector2 inputValue){

        playerRigidbody.angularVelocity = new Vector3(0f, 0f, 0f);

        if(inputValue.magnitude>0)
            RotatePlayerToHead();


        Vector3 currentVelocity = playerRigidbody.velocity;
        Vector3 targetVelocity = new Vector3(inputValue.x, 0f, inputValue.y);

        targetVelocity *= 10f;
        targetVelocity = transform.TransformDirection(targetVelocity);

        Vector3 velocityChange = targetVelocity-currentVelocity;
        velocityChange.y = 0;
        Vector3.ClampMagnitude(velocityChange, maxForce);


        playerRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    
    public void RotatePlayerToHead(){
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y+firstPersonCamera.transform.localEulerAngles.y, 0f);
        firstPersonCamera.transform.localEulerAngles = new Vector3(firstPersonCamera.transform.localEulerAngles.x, 0f, 0f);
        Debug.Log(firstPersonCamera.transform.localEulerAngles);
    }
    
    
    
    public void Jump(InputAction.CallbackContext context){
        if (!context.performed) return;

        playerRigidbody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
    }
    
    
}
