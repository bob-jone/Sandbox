using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Camera")]
    [SerializeField] private Vector2 maxFollowOffset = new Vector2(-1f, 6f);
    // X and Y velocity of the camera
    [SerializeField] private Vector2 cameraVelocity = new Vector2(4f, 0.25f);
    // Rigidbody on this object.
    private Rigidbody _rigidbody;
    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;
    // We need a reference to the cinemachine virtual camera because we need to change the values on it later
    [SerializeField] private CinemachineVirtualCamera virtualCamera = null;

    // max angular velocity of the rigidbody (how fast it can spin)
    [SerializeField]
    private float maxAngularVelocity;

    private float xLookAxis;


    // Get player controls from the new unity controls system
    private PlayerControls controls;
    private PlayerControls Controls
    {
        get
        {
            if (controls != null) { return controls; }
            {
                return controls = new PlayerControls();
            }
        }
    }

    // This is a special cinemachine component that we get from the virtual camera
    private CinemachineTransposer transposer;

    // Overriding OnStartAuthority which comes from NetworkBehavior
    // This gets called on the object that has authority over this game object
    // Disable the camera in all the player prefabs -- then if the player has authority just enable their camera
    public override void OnStartAuthority()
    {

        base.OnStartAuthority();
        _rigidbody = GetComponent<Rigidbody>();
        _rpo = GetComponent<ReactivePhysicsObject>();

        // Get our transposer from the virtual camera
        transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();

        // Set the camera to active
        virtualCamera.gameObject.SetActive(true);

        enabled = true;

        // += means subscribe to event so when the player look is performed we want to get the Vector2 value
        controls.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (base.hasAuthority)
            RotatePlayer();
    }


    // The client callback tag means the server won't mess around with it
    [ClientCallback]

    private void OnEnable()
    {
        Controls.Enable();
    }
    [ClientCallback]
    private void OnDisable()
    {
        Controls.Disable();
    }

    // Actual logic for moving the camera
    private void Look(Vector2 lookAxis)
    {
        // We use deltaTime twice here so just caching it for ease of use
        float fixedDeltaTime = Time.fixedDeltaTime;


        // As we move the mouse up and down the camera offset changes which is what makes the camera move up and down
        float followOffset = Mathf.Clamp(
            transposer.m_FollowOffset.y - (lookAxis.y * cameraVelocity.y * Time.deltaTime),
            maxFollowOffset.x,
            maxFollowOffset.y);

        // Send the data to the transposer
        transposer.m_FollowOffset.y = followOffset;

        
        // Rotate the player on the Y axis only
        //_rigidbody.AddRelativeTorque(0f, lookAxis.x * cameraVelocity.x * Time.fixedDeltaTime, 0f);


        xLookAxis = lookAxis.x;
        Debug.Log($"lookAxis is  {lookAxis} and xLookAxis is {xLookAxis}");
    }

    private void RotatePlayer()
    {
        Quaternion deltaRotation = Quaternion.Euler(0f, xLookAxis * cameraVelocity.x * Time.fixedDeltaTime, 0f);

        _rigidbody.maxAngularVelocity = maxAngularVelocity;
        // _rigidbody.AddTorque(0f, xLookAxis * cameraVelocity.x * Time.fixedDeltaTime, 0f);

        _rigidbody.AddTorque(transform.up * xLookAxis);
    }

}
