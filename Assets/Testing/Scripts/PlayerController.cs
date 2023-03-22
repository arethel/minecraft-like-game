using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    

    private PlayerInput playerInput;
    
    [SerializeField]
    private GameObject firstPersonCamera;


    private PlayerControls playerControls;
    
    private CharacterController characterController;


    private Animator animator;

    private void Awake() {
        playerInput = GetComponent<PlayerInput>();
        
        playerControls = new PlayerControls();
        
        playerControls.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        
        playerControls.Player.Jump.performed += Jump;

        characterController = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();

        
    }

    public override void OnNetworkSpawn()
    {
        AssignCamera();
    }
    private void AssignCamera(){
        if(IsOwner){
            GameObject followCam = GameObject.FindGameObjectWithTag("FollowCam");
            followCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = firstPersonCamera.transform;
        }
    }
    

    private Vector3 moveDiraction = Vector3.zero;


    private void FixedUpdate() {
        //moveDiraction = new Vector3(0f, moveDiraction.y, 0f);
        
        //TestSphereCast();
        if(IsOwner)
            Move(playerControls.Player.Move.ReadValue<Vector2>());
        ApplyGravity();
        
        
        characterController.Move(moveDiraction + moveController);
        
        if(IsOwner)
            ApplyAnimation();

    }
    
    private void LateUpdate() {
        if(IsOwner)
            CameraRotation();
    }

    bool landingTransition = false;
    void ApplyAnimation(){
        Vector3 moveVector = moveDiraction + moveController;
        
        if(new Vector3(moveVector.x, 0, moveVector.z).magnitude>0.0f){
            animator.SetBool("IsMoving", true);
        }
        else{
            animator.SetBool("IsMoving", false);
        }
        
        animator.SetBool("IsGrounded", characterController.isGrounded);
        if(characterController.isGrounded
        &&(animator.GetCurrentAnimatorStateInfo(0).IsName("Jump Up")||animator.GetCurrentAnimatorStateInfo(0).IsName("Falling Idle"))
        &&!landingTransition){
            animator.CrossFade("Landing", 0.1f);
            landingTransition = true;
        }
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

    Vector3 moveController = Vector3.zero;
    public void Move(Vector2 inputValue){
        
        if(inputValue.magnitude>0)
            RotatePlayerToHead();
        
        moveController = transform.TransformDirection(new Vector3(inputValue.x, 0f, inputValue.y)) * Time.deltaTime * movementSpeed;
        
    }
    
    
    [SerializeField]
    private float gravity = 10f;

    bool wasOnGround = false;
    private void ApplyGravity(){
        
        RaycastHit hit;
        if(Physics.SphereCast(transform.position + Vector3.up * 0.5f, 0.5f, Vector3.down, out hit, 0.2f)){
            wasOnGround = true;
            
            if(IsOwner)
                animator.SetBool("IsFalling", false);
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            
            //moveDiraction.y = -gravity * Time.deltaTime;
            
            if(angle<=characterController.slopeLimit){
                JumpFun();
                return;
            }

            jump = false;
            float sin = Mathf.Sin(angle*Mathf.PI/180);
            float cos = Mathf.Cos(angle*Mathf.PI/180);

            Vector3 xzAxis = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;

            float slidingMove = Vector3.Dot(xzAxis,moveController);
            if(slidingMove<0){
                moveDiraction = -slidingMove*xzAxis;
            }
            /*
            
            Vector3 newVector = xzAxis*cos;
            
            moveDiraction = newVector * Time.deltaTime * gravity * slidingK;
            moveDiraction.y = gravity * Time.deltaTime * sin * slidingK;*/
            
            
        }
        else{
            
            if(wasOnGround){
                wasOnGround = false;
                if(moveDiraction.y<0f)
                    moveDiraction.y = 0f;
            }
            
            if(IsOwner)
                if(moveDiraction.y<-0.09){
                    animator.SetBool("IsFalling", true);
                    landingTransition = false;
                }
                
            if(IsOwner)
                animator.SetBool("IsJumping", false);
        }
        
        if(!characterController.isGrounded){
            moveDiraction.x = 0;
            moveDiraction.z = 0;
        }
        
        moveDiraction += Vector3.down * gravity * Time.deltaTime;
    }
    
    void JumpFun(){
        if(jump){
            jump = false;
            moveDiraction.y = 0f;
            Vector3 jumpVelocity = Vector3.up * jumpForce * Time.deltaTime;
            moveDiraction += jumpVelocity;
            animator.CrossFade("Jump Up", 0.1f);
            animator.SetBool("IsJumping", true);
            landingTransition = false;
        }
    }
    
    public void RotatePlayerToHead(){
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y+firstPersonCamera.transform.localEulerAngles.y, 0f);
        firstPersonCamera.transform.localEulerAngles = new Vector3(firstPersonCamera.transform.localEulerAngles.x, 0f, 0f);
    }

    [SerializeField]
    private float jumpForce=1;
    bool jump = false;
    
    public void Jump(InputAction.CallbackContext context){
        if (!context.performed || !characterController.isGrounded || !IsOwner) return;
        jump=true;
        
        

    }


    
    
}
