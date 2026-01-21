using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    public float wallRunTimer;

    [Header("Wall Jump")]
    public float wallJumpUpForce;
    public float wallJumpSideForce;

    [Header("Camera")]
    public Camera cam; // Zmieniono na Camera
    public float tiltAngle = 15f;
    public float tiltSpeed = 10f;
    private float currentTilt;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        
        // Automatyczne przypisanie kamery jeśli puste
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
        HandleCameraTilt();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            if (!pm.wallrunning)
                StartWallRun();

            if (Input.GetKeyDown(pm.jumpKey))
                WallJump();
        }
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void HandleCameraTilt()
    {
        float targetTilt = 0;

        if (pm.wallrunning)
        {
            if (wallLeft) targetTilt = -tiltAngle;
            else if (wallRight) targetTilt = tiltAngle;
        }

        // Płynne przejście
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        // Aplikowanie rotacji tylko na osi Z, zachowując X i Y z myszki
        cam.transform.localRotation = Quaternion.Euler(cam.transform.localEulerAngles.x, cam.transform.localEulerAngles.y, currentTilt);
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
    }

    private void WallJump()
    {
        StopWallRun();
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
    }
}