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
    [SerializeField] private Vector2 cameraVelocity = new Vector2(10f, 0.25f);
    // Rigidbody on this object.
    private Rigidbody _rigidbody;
    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;
    // We need a reference to the cinemachine virtual camera because we need to change the values on it later
    [SerializeField] private CinemachineFreeLook virtualCamera = null;



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
    //private CinemachineOrbitalTransposer orbitalTransposer;

    // Overriding OnStartAuthority which comes from NetworkBehavior
    // This gets called on the object that has authority over this game object
    // Disable the camera in all the player prefabs -- then if the player has authority just enable their camera
    public override void OnStartAuthority()
    {

        base.OnStartAuthority();
        _rigidbody = GetComponent<Rigidbody>();
        _rpo = GetComponent<ReactivePhysicsObject>();

        // Get our transposer from the virtual camera
        //orbitalTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

        // Set the camera to active
        virtualCamera.gameObject.SetActive(true);

        enabled = true;

        // += means subscribe to event so when the player look is performed we want to get the Vector2 value
        controls.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());
        controls.Player.Look.canceled += ctx => Look(ctx.ReadValue<Vector2>());

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
        

        xLookAxis = lookAxis.x;
    }

    private void RotatePlayer()
    {
        float deltaTime = Time.deltaTime;

        _rigidbody.transform.Rotate(0f,xLookAxis * cameraVelocity.x * deltaTime, 0f);
    }

}
