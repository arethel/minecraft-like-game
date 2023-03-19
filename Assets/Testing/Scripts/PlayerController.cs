using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    

    private PlayerInput playerInput;
    
    [SerializeField]
    private GameObject firstPersonCamera;


    private PlayerControls playerControls;
    
    private CharacterController characterController;
    
    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        
        playerControls = new PlayerControls();
        
        playerControls.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        playerControls.Player.Jump.performed += Jump;

        characterController = GetComponent<CharacterController>();
    }

    private Vector3 moveDiraction = Vector3.zero;
    private void FixedUpdate() {
        moveDiraction = Vector3.zero;
        
        Move(playerControls.Player.Move.ReadValue<Vector2>());
        ApplyGravity();
        
        characterController.Move(moveDiraction);

        TestSphereCast();
    }
    
    private void LateUpdate() {
        CameraRotation();
    }


    public float sensitivity = 0.025f;

    float yView = 0;
    public void CameraRotation(){

        Vector2 lookImput = playerControls.Player.Look.ReadValue<Vector2>();

        firstPersonCamera.transform.Rotate(Vector3.up * lookImput.x * sensitivity);

        yView += -lookImput.y * sensitivity;
        Vector3 angels = firstPersonCamera.transform.eulerAngles;
        
        firstPersonCamera.transform.eulerAngles = new Vector3(Mathf.Clamp(yView, -80f, 80f), angels.y, 0f);


    }

    [SerializeField]
    private float movementSpeed = 4f;
    public void Move(Vector2 inputValue){
        
        if(inputValue.magnitude>0)
            RotatePlayerToHead();

        moveDiraction += transform.TransformDirection(new Vector3(inputValue.x, 0f, inputValue.y)) * Time.deltaTime * movementSpeed;
        
    }
    
    
    [SerializeField]
    private float gravity = 10f;
    private void ApplyGravity(){
        moveDiraction += Vector3.down * gravity * Time.deltaTime;
    }
    
    public void RotatePlayerToHead(){
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y+firstPersonCamera.transform.localEulerAngles.y, 0f);
        firstPersonCamera.transform.localEulerAngles = new Vector3(firstPersonCamera.transform.localEulerAngles.x, 0f, 0f);
    }
    
    
    public void Jump(InputAction.CallbackContext context){
        if (!context.performed || !characterController.isGrounded) return;

    }

    
    private void TestSphereCast(){
        RaycastHit hit;
        if(Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, 2f, 0)){
            Debug.Log(hit.normal);
        }
    }
    
    
}
