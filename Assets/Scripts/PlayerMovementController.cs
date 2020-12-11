using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerMovementController : NetworkBehaviour
{
    #region Serialized.
    // How much force to apply towards move direction.
    [Tooltip("How much force to apply towards move direction.")]
    [SerializeField]
    private float _playerSpeed = 5f;

    // Smooth rotations
    [SerializeField]
    private float _turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;

    [SerializeField]
    private float rotationSpeed = 20f;

    [SerializeField]
    private CinemachineFreeLook freeLookCamera = null;
    #endregion

    #region Private.
    // Rigidbody on this object.
    private Rigidbody _rigidbody;
    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;

    private Vector2 inputVector;

    private float desiredRotationAngle = 0;

    // Reference to PlayerControls (and if it doesn't exist already make it exist)
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
    #endregion

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        _rigidbody = GetComponent<Rigidbody>();
        _rpo = GetComponent<ReactivePhysicsObject>();

        // += means subscribe to event so when the player move is performed we want to get the Vector2 value
        Controls.Player.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        Controls.Player.Move.canceled += ctx => OnMove(ctx.ReadValue<Vector2>());
    }

    private void OnMove(Vector2 context)
    {
        inputVector = context;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _rigidbody = GetComponent<Rigidbody>();

    }

    private void OnEnable()
    {
        Controls.Enable();
    }
    [ClientCallback]
    private void OnDisable()
    {
        Controls.Disable();
    }

    private void FixedUpdate()
    {
        if (base.hasAuthority)
            CheckMove();
    }

    // Checks if the client wants to move.
    private void CheckMove()
    {
        float xAxis = inputVector.x;
        float zAxis = inputVector.y;

        Vector3 direction = new Vector3(
            xAxis,
            0f,
            zAxis
            ).normalized;

        if (direction == Vector3.zero)
            return;

        //Move locally.
        ProcessInput(direction);
        _rpo.ReduceAggressiveness();

        //Only send inputs to server if not a client host.
        if (base.isClientOnly)
            CmdSendInput(direction);
    }

    public void HandleMovementDirection(Vector3 direction)
    {
        desiredRotationAngle = Vector3.Angle(transform.forward, direction);
        var crossProduct = Vector3.Cross(transform.forward, direction).y;
        if (crossProduct < 0)
        {
            desiredRotationAngle *= -1;
        }
    }

    private void RotateAgent()
    {
        if (desiredRotationAngle > 10 || desiredRotationAngle < -10)
        {
            transform.Rotate(Vector3.up * desiredRotationAngle * rotationSpeed * Time.deltaTime);
        }
    }

    // Applies an input direction to the rigidbody.
    private void ProcessInput(Vector3 input)
    {
        var cameraForewardDirection = Camera.main.transform.forward;
        Debug.DrawRay(Camera.main.transform.position, cameraForewardDirection * 10, Color.red);

        var directionToMoveIn = Vector3.Scale(cameraForewardDirection, (Vector3.right + Vector3.forward));
        Debug.DrawRay(Camera.main.transform.position, directionToMoveIn * 10, Color.blue);
        directionToMoveIn = directionToMoveIn.normalized;

        HandleMovementDirection(directionToMoveIn);

        RotateAgent();

        // Rotating rigidbody
        /*float targetAngle = Mathf.Atan2(input.x,input.z) * Mathf.Rad2Deg + Camera.main.transform.rotation.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);

        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        _rigidbody.transform.rotation = Quaternion.Euler(0f, angle, 0f);*/

        _rigidbody.MovePosition(transform.position + Time.deltaTime *
               transform.TransformDirection(input) * _playerSpeed);
    }

    // Tells the server which inputs to move.
    [Command]
    private void CmdSendInput(Vector3 input)
    {
        ProcessInput(input);
    }
}
