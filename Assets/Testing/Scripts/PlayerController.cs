using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using QFSW.QC;

public class PlayerController : NetworkBehaviour
{
    

    
    
    [SerializeField]
    private GameObject firstPersonCamera;



    private CharacterController characterController;


    private Animator animator;

    private void Awake() {

        characterController = GetComponent<CharacterController>();

        animator = GetComponentInChildren<Animator>();
    }
    
    private void AssignPlayerControls(){
        InputController.Singleton.playerControls.Player.Jump.performed += Jump;
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner){
            AssignPlayerControls();
            AssignCamera();
            InputController.Singleton.SetModeOfInput(InputController.Modes.PLayer);
        }
            
    }
    
    private void AssignCamera(){
        GameObject followCam = GameObject.FindGameObjectWithTag("FollowCam");
        followCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = firstPersonCamera.transform;
        
    }
    

    private Vector3 moveDiraction = Vector3.zero;


    private void FixedUpdate() {
        if(IsOwner)
            Move(InputController.Singleton.playerControls.Player.Move.ReadValue<Vector2>());
            
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

        Vector2 lookImput = InputController.Singleton.playerControls.Player.Look.ReadValue<Vector2>();//input group

        firstPersonCamera.transform.Rotate(Vector3.up * lookImput.x * sensitivity);

        yView += -lookImput.y * sensitivity;
        Vector3 angels = firstPersonCamera.transform.eulerAngles;
        
        firstPersonCamera.transform.eulerAngles = new Vector3(Mathf.Clamp(yView, -80f, 80f), angels.y, 0f);


    }

    [SerializeField]
    private float movementSpeed = 4f;

    Vector3 moveController = Vector3.zero;
    public void Move(Vector2 inputValue){
        
        if(inputValue.magnitude>0){
            RotatePlayerToHead(inputValue);
            moveController = firstPersonCamera.transform.TransformDirection(new Vector3(inputValue.x, 0f, inputValue.y)) * Time.deltaTime * movementSpeed;
        }
        else{
            moveController = Vector3.zero;
        }
        
        
    }


    [SerializeField]
    private float rotateSpeed = 1;
    [SerializeField]
    private float minimizerExp = 4;
    public void RotatePlayerToHead(Vector2 input){
        float difAngle = firstPersonCamera.transform.eulerAngles.y - transform.eulerAngles.y;

        float changeOfRot = GetDiractionAngle(difAngle) * rotateSpeed * Mathf.Exp(-minimizerExp / Mathf.Abs(difAngle));
        /*if (Mathf.Abs(difAngle) < 5f)
        {
            changeOfRot = 0;
        }*/

        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y + changeOfRot, 0f);
        firstPersonCamera.transform.localEulerAngles = new Vector3(firstPersonCamera.transform.localEulerAngles.x, firstPersonCamera.transform.localEulerAngles.y - changeOfRot, 0f);
    }
    
    float GetDiractionAngle(float difAngle)
    {
        if(Mathf.Abs(difAngle)>180){
            if(difAngle>0){
                return -1;
            }
            else{
                return 1;
            }
        }
        return Mathf.Sign(difAngle);
    }
    
    float GetAngle(Vector2 direction){
        
        if(direction.x>0){
            return 90-Mathf.Atan(direction.y/direction.x)*180/Mathf.PI;
        }
        else if(direction.x<0){
            return 270+Mathf.Atan(-direction.y/direction.x)*180/Mathf.PI;
        }
        
        if(direction.y<0){
            return 180;
        }
        
        return 0;
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
    
    

    [SerializeField]
    private float jumpForce=1;
    bool jump = false;
    
    public void Jump(InputAction.CallbackContext context){
        if (!context.performed || !characterController.isGrounded || !IsOwner) return;
        jump=true;
    }


    
    
}
