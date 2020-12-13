using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerCameraController : NetworkBehaviour
{

    // Rigidbody on this object.
    private Rigidbody _rigidbody;

    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;

    private float xLookAxis;

    [SerializeField]
    private float _rotationSpeed = 5f;

    [SerializeField]
    private float maxAngularVelocity = 10f;

    [SerializeField] 
    private CinemachineFreeLook freeLookCamera = null;

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

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        _rigidbody = GetComponent<Rigidbody>();
        _rpo = GetComponent<ReactivePhysicsObject>();

        enabled = true;
        freeLookCamera.gameObject.SetActive(true);

        Controls.Player.Look.performed += ctx => Look(ctx.ReadValue<Vector2>());
        Controls.Player.Look.canceled += ctx => Look(ctx.ReadValue<Vector2>());
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (base.hasAuthority)
        {
            RotatePlayer();
        }
    }

    // The client callback tag means the server won't mess around with it
    [ClientCallback]

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    [ClientCallback]
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void Look(Vector2 lookAxis)
    {
        Debug.Log("OLD " + lookAxis.x);

        xLookAxis = lookAxis.x;

        Debug.Log("NEW " + lookAxis.x);
    }

    private void RotatePlayer()
    {
        Debug.Log("Attempting to rotate player!");

        /*Quaternion deltaRotation = Quaternion.Euler(0f, xLookAxis * _rotationSpeed * Time.deltaTime, 0f);

        _rigidbody.maxAngularVelocity = maxAngularVelocity;

        _rigidbody.AddTorque(transform.up * xLookAxis);*/

        transform.Rotate(0f, xLookAxis * _rotationSpeed * Time.deltaTime, 0f);
    }
}
