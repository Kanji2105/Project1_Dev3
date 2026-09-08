using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class wasd3D : MonoBehaviour
{
    public float MouseSensitivity = 3f;
    public float speed = 100f;
    public float dragCoefficient = 1f;
    Rigidbody myRB;

    //jump for the charcter
    public static wasd3D Instance;
    public float jumpForce = 15f;
    bool isGrounded = false;
    public int extraJumpsValue = 1;
    private int extraJumps;
    public float doubleJumpTimer;

    //jump fixed 
    public float airDragCoefficient = 0.3f;
    [Range(0f, 1f)]
    public float airControlFactor = 0.4f;

    //wall ability//jumpoinwall/climing (test)

    bool isOnWall = false;
    bool isClimbing = false;
    public float wallRunGravity = 2f;
    public float wallJumpForce = 10f;
    public float wallRunSpeed = 150f;
   
    Vector3 wallNormal;

    void Awake()
    {
        myRB = GetComponent<Rigidbody>();
        myRB.maxLinearVelocity = 20f;
        Instance = this;
    }

    void Start()
    {
        extraJumps = extraJumpsValue;
        doubleJumpTimer = 99999;
    }

    void Update()
    {
        //If my mouse goes left/right my body moves left/right
        float xRot = Input.GetAxis("Mouse X") * MouseSensitivity;
        transform.Rotate(0, xRot, 0);

        if (isGrounded)
        {
            extraJumps = extraJumpsValue;
            doubleJumpTimer = 0;
        }
        else
        {
            doubleJumpTimer += Time.deltaTime;
        }

        // Regular jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            myRB.linearVelocity = new Vector3(myRB.linearVelocity.x, jumpForce, myRB.linearVelocity.z);
        }

        // Air jump
        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded && extraJumps > 0 && doubleJumpTimer < 5f)
        {
            // Dampen horizontal speed before the air jump so it doesnt feel slippery
            Vector3 dampedVel = new Vector3(myRB.linearVelocity.x * 0.5f, jumpForce, myRB.linearVelocity.z * 0.5f);
            myRB.linearVelocity = dampedVel;
            extraJumps--;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isOnWall && !isGrounded)
        {
            isClimbing = false; // so it dosen't intervene with the wall jump and wall walk 
            myRB.useGravity = true;
            myRB.linearVelocity = new Vector3(wallNormal.x * wallJumpForce, jumpForce, wallNormal.z * wallJumpForce);
            isOnWall = false;
        }

        if (Input.GetKeyDown(KeyCode.E) && isOnWall)
        {
            isClimbing = true;
            myRB.useGravity = false;     // help to prevent me from falling to fast or drastictly

            //Debug.Log("climbing:" + isClimbing);
        }

        if (isClimbing)
        {
            float climbInput = Input.GetAxis("Vertical");

            myRB.linearVelocity = new Vector3(
               myRB.linearVelocity.x,
               climbInput *1.5f,
               myRB.linearVelocity.z
            );
              //W gives positive vertical movement
              //  S gives negative vertical movement
              //1.5f is just the climbing speed for now
        }
    }

    void FixedUpdate()
    {
        myRB.angularVelocity = Vector3.zero;


        if (isClimbing)
            return;
        // this will help to make sure it will stick to the wall and it wont intervene with the mocement 
        // if im climbing, stop executing the normal movement code for this physic frame that hwat retur help here 
        Vector3 inputDir = transform.TransformDirection(Direction());

        if (inputDir != Vector3.zero)
        {
            float control = isGrounded ? 1f : airControlFactor;
            myRB.AddForce(inputDir * speed * control * Time.fixedDeltaTime);
        }
        else
        {
            float drag = isGrounded ? dragCoefficient : airDragCoefficient;
            Vector3 horizontalVel = new Vector3(myRB.linearVelocity.x, 0, myRB.linearVelocity.z);
            myRB.AddForce(-horizontalVel * drag * Time.fixedDeltaTime);
        }

        if (isOnWall && !isGrounded)
        {
            myRB.AddForce(Vector3.up * wallRunGravity * Time.fixedDeltaTime);
            myRB.AddForce(-transform.forward * wallRunSpeed * Time.fixedDeltaTime);
        }

    }

    Vector3 Direction()
    {
        float h = -Input.GetAxis("Horizontal"); //horizontal axis is A/D, controls left/right strafe
        float v = -Input.GetAxis("Vertical"); //vertical axis is W/S, controls forwards/backwards walk
        return new Vector3(h, 0, v); //return a direction with horizontal strafe for the X-axis and forwards move for the Z-axis
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ground"))
            isGrounded = false;

        if (col.gameObject.CompareTag("Wall"))
        {
            isOnWall = false;
            isClimbing = false;

            myRB.useGravity = true; // just bring back the gravity on
        }  // Debug.Log("isOmWall: " + isOnWall);

        
    }

    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("Wall"))
        {
            isOnWall = true;
            wallNormal = col.contacts[0].normal;
            
        }
    }

    
}