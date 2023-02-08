using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController controller;
    public Transform cameraReference;
    public Transform groundReference;
    public Transform characterReference;
    public Transform grappleReference;
    public LayerMask groundMask;
    public LayerMask wallMask;
    public Animator anim;
    public CinemachineFreeLook TPCam;
    public CinemachineVirtualCamera FPCam;
    public CinemachineVirtualCamera FPCamSprint;
    public CinemachineVirtualCamera FPCamGrapple;


    public float speed;
    public float sprintSpeed;
    public float jumpHeight;
    public float doubleJumpVar;
    public float wallRunForce;
    public float gravity;  
    
    
    
    private float playerVertVelocity;
    private float grappleRopeSize;
    private Vector3 grapplePoint;
    private Vector3 playerMomentum;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private float groundDistance = 0.2f;
    private bool canDoubleJump;
    private bool toggle;
    private bool iswallLeft, iswallRight;
    private bool isWallRunning;
    private bool activeGrapple;
    private bool freeze;
    
    private State state;

    private enum State
    {
        Normal,
        GrappleThrown,
        GrapplingState,
        
    }

    private void Start()
    {
        state = State.Normal;
        grappleReference.gameObject.SetActive(false);   
    
    }

    // Update is called once per frame
    private void Update()
    {
        switch (state) {
            default:
            case State.Normal:
                isGrounded = Physics.CheckSphere(groundReference.position, groundDistance, groundMask);
                handleWallRun();
                handleFirstPersonMovement();
                handleCamToggle();
                handleGrappleStart();
                break;
            case State.GrappleThrown:
                grappleThrow();
                handleFirstPersonMovement();
                break;
            case State.GrapplingState:
                grappleToPosition();
                handleWallRun();
                //handleFirstPersonGrappleMovement();
                break; 


        }
    }  
    
    void handleFirstPersonMovement()
    {   
    

        float moveAD = Input.GetAxis("Horizontal");
        float moveWS = Input.GetAxis("Vertical");

        playerVelocity = (transform.right * moveAD * speed) + (transform.forward * moveWS * speed);

       if(Input.GetButtonDown("Jump"))
        {
            if(isGrounded) playerVertVelocity = Mathf.Sqrt(jumpHeight * -2.0f * gravity);

            else if(canDoubleJump) 
            {
                playerVertVelocity = Mathf.Sqrt((jumpHeight * doubleJumpVar)  * -2.0f * gravity);
                Debug.Log("Double Jump!");
                canDoubleJump = false;
            } 
        }

        if(Input.GetButtonDown("Sprint"))
        {   
            speed = sprintSpeed;
            FPCamSprint.Priority = 1;
            FPCam.Priority = 0;
        }
        if(Input.GetButtonUp("Sprint"))
        {
            speed = 7.0f;
            FPCamSprint.Priority = 0;
            FPCam.Priority = 1;
        }


        if((isGrounded) && playerVertVelocity < 0 )
        {
            playerVertVelocity = -2.0f;
            canDoubleJump = true;

        }        

        playerVertVelocity += gravity * Time.deltaTime;

        playerVelocity.y = playerVertVelocity;
        
        playerVelocity += playerMomentum;


        controller.Move(playerVelocity * Time.deltaTime);

        if(playerMomentum.magnitude >= 0.0f )
        {
            float momentumGroundDrag = 2.0f;
            float momentumAirDrag = 0.5f;

            if(isGrounded)  playerMomentum -= playerMomentum * momentumGroundDrag *Time.deltaTime;
            else playerMomentum -= playerMomentum * momentumAirDrag *Time.deltaTime;

            if(playerMomentum.magnitude < .0f)
            {
                playerMomentum = Vector3.zero; 
            }
        }
    }
    

    void handleFirstPersonGrappleMovement()
    {   
        if(freeze) return;

        /*WORK ON THIS TO SIMULATE SWING BETTER*/

        float moveAD = Input.GetAxis("Horizontal");

        Vector3 playerVelocityGrapple = (transform.right * moveAD * speed * 3);       

        playerVertVelocity += (gravity * 0.05f) * Time.deltaTime;

        playerVelocityGrapple.y = playerVertVelocity;
        
        playerVelocityGrapple += playerMomentum;


        controller.Move(playerVelocityGrapple * Time.deltaTime);

        if(playerMomentum.magnitude >= 0.0f)
        {
            float momentumAirDrag = 0.5f;
            playerMomentum -= playerMomentum * momentumAirDrag *Time.deltaTime;

            if(playerMomentum.magnitude < .0f)
            {
                playerMomentum = Vector3.zero; 
            }
        }
    }


    
    
    void handleWallRun()
    {

        checkWall();
        if(Input.GetKey(KeyCode.D) && iswallRight)
        {
            wallRunStart();
        }

        if(Input.GetKey(KeyCode.A) && iswallLeft)
        {
            wallRunStart();
        }
    }

    
    
    void wallRunStart()
    {
        setGravity(-0.5f);
       
        isWallRunning = true;
        canDoubleJump = true;

       

        controller.Move(characterReference.forward * wallRunForce * Time.deltaTime);

        if(iswallRight)
        {
            controller.Move(characterReference.right * wallRunForce/5 * Time.deltaTime);  
        }
        else if (iswallLeft)
        { 
            controller.Move(-characterReference.right * wallRunForce/5 * Time.deltaTime);
        }
    }

    
    
    void wallRunStop()
    {
        defaultGravity();
        isWallRunning = false;
    }

    void checkWall()
    {
        iswallRight = Physics.Raycast(transform.position, characterReference.right, 1f, wallMask);
        iswallLeft = Physics.Raycast(transform.position, -characterReference.right, 1f, wallMask);
    
        if(!iswallLeft && !iswallRight)
        {
            wallRunStop();
        }
    }

    
    
    private void handleGrappleStart()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            if(Physics.Raycast(FPCam.transform.position, FPCam.transform.forward, out RaycastHit raycastHit))
            {
                grapplePoint = raycastHit.point;
                grappleRopeSize = 0.0f;
                grappleReference.gameObject.SetActive(true);
                grappleReference.localScale = Vector3.zero; 
                state = State.GrappleThrown;
            }
        }
    }

    private void grappleThrow()
    {
        grappleReference.LookAt(grapplePoint);

        float grappleThrowSpeed = 70f;
        grappleRopeSize += grappleThrowSpeed * Time.deltaTime;
        grappleReference.localScale = new Vector3(1, 1, grappleRopeSize);

        if(grappleRopeSize >= Vector3.Distance(transform.position, grapplePoint))
        {
            state = State.GrapplingState;
        }
    }    


    public void grappleToPosition()
    {
        grappleReference.LookAt(grapplePoint);
        activeGrapple  = true;
        Vector3 grappleDirection = (grapplePoint - transform.position).normalized;
        float moveAD = Input.GetAxis("Horizontal");
        Vector3 playerVelocityGrapple = (transform.right * moveAD);

        float grappleMinSpeed = 7.0f;
        float grappleMaxSpeed = 14.0f;
        float grappleSpeed = 9.0f; //Mathf.Clamp(Vector3.Distance(transform.position, grapplePoint), grappleMinSpeed, grappleMaxSpeed);
        float grappleSpeedVar = 2.0f;

        FPCamGrapple.Priority = 1;
        FPCam.Priority = 0;
        
        
       
        
        controller.Move((grappleDirection+playerVelocityGrapple) * grappleSpeed  * grappleSpeedVar * Time.deltaTime);
        
        if(Vector3.Distance(transform.position, grapplePoint) < 1.0f )
        {
            activeGrapple = false;
            stopGrapple();
       }

        if(Input.GetButtonDown("Jump"))
        {
            float extraMomentumSpeed = 2.0f;
            float grappleJumpHeight = 5.0f;
            playerMomentum = FPCam.transform.forward * grappleSpeed * extraMomentumSpeed;
            playerMomentum += Vector3.up * grappleJumpHeight;
            stopGrapple();
        } 

        if(Input.GetKeyDown(KeyCode.F))
        {
            stopGrapple();
        }

    }

    void stopGrapple()
    {
        state = State.Normal;   
        defaultGravity();
        grappleReference.gameObject.SetActive(false); 
        FPCamGrapple.Priority = 0;
        FPCam.Priority = 1;
    }

    
    void setGravity(float g)
    {
        gravity = g;
    }

    void defaultGravity()
    {
        gravity = -9.81f;
    }  

    void handleCamToggle()
    {
        if(Input.GetButtonDown("CamToggle"))
        {
            toggle = !toggle;
            if(toggle)
            {   
                freeze = true;
                TPCam.Priority = 1;
                FPCam.Priority = 0;
            }
            else
            {
                freeze = false;
                TPCam.Priority = 0;
                FPCam.Priority = 1;

            }
        }
    }

    //public float smoothenVar;
    //private float turnSmoothVelocity;
    /*void thirdPersonMovement()
    {
        
        float ADmovement = Input.GetAxis("Horizontal");
        float WSmovement = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(ADmovement,0f,WSmovement).normalized;


        if(direction.magnitude > 0.1f)
        {
            float angleDirection = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraReference.eulerAngles.y;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angleDirection, ref turnSmoothVelocity,smoothenVar);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            Vector3 trueDirection = Quaternion.Euler(0f, angleDirection, 0f) * Vector3.forward;

    
            controller.Move(trueDirection*speed*Time.deltaTime);
        }
    }*/

    /*void handleMovement()
    {
         if(TPCam.Priority == 1)
        {
            thirdPersonMovement();
        }
        else if(FPCam.Priority == 1)
        {
            firstPersonMovement();
        } 
    }*/ 

}


 