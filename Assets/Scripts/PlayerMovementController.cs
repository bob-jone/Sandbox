using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovementController : NetworkBehaviour
{

    [SerializeField]
    private float _moveSpeed = 5f;
    [SerializeField]
    private float _rotationSpeed = 5f;

    // Rigidbody on this object.
    private Rigidbody _rigidbody;
    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;

    private Vector3 movementVector;

    private Vector2 cameraLookVector;

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

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        _rigidbody = GetComponent<Rigidbody>();
        _rpo = GetComponent<ReactivePhysicsObject>();

        // += means subscribe to event so when the player move is performed we want to get the Vector2 value
        Controls.Player.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());
        Controls.Player.Move.canceled += ctx => OnMove(ctx.ReadValue<Vector2>());

        Controls.Player.Look.performed += ctx => OnLook(ctx.ReadValue<Vector2>());
        Controls.Player.Look.canceled += ctx => OnLook(ctx.ReadValue<Vector2>());
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (base.hasAuthority)
            CheckMove();
    }

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

    private void OnMove(Vector2 context)
    {
        movementVector = context;
        Debug.Log($"Set movementVector to " + context);
    }

    private void OnLook(Vector2 context)
    {
        cameraLookVector = context;
        Debug.Log($"Set the cameraLookVector to " + context);
    }

    private void CheckMove()
    {
        Vector3 direction = new Vector3(
            movementVector.x,
            0f,
            movementVector.y);

        if (direction == Vector3.zero)
            return;

        //Move locally.
        ProcessInput(direction);
        _rpo.ReduceAggressiveness();

        //Only send inputs to server if not a client host.
        if (base.isClientOnly)
            CmdSendInput(direction);
    }

    [ClientRpc]
    private void PlayerRotation()
    {
        // Rotation stuff

        var cameraForewardDirection = Camera.main.transform.forward;
        Debug.DrawRay(Camera.main.transform.position, cameraForewardDirection * 10, Color.red);
        var directionToMoveIn = Vector3.Scale(cameraForewardDirection, (Vector3.right + Vector3.forward));
        Debug.DrawRay(Camera.main.transform.position, directionToMoveIn * 10, Color.blue);

        desiredRotationAngle = Vector3.Angle(transform.forward, directionToMoveIn);
        var crossProduct = Vector3.Cross(transform.forward, directionToMoveIn).y;
        if (crossProduct < 0)
        {
            desiredRotationAngle *= -1;
        }

        if (desiredRotationAngle > 10 || desiredRotationAngle < -10)
        {
            _rigidbody.transform.Rotate(Vector3.up * desiredRotationAngle * _rotationSpeed * Time.deltaTime);
        }
    }

    private void ProcessInput(Vector3 input)
    {
        PlayerRotation();

        // Movement stuff

        Vector3 right = _rigidbody.transform.right;
        Vector3 forward = _rigidbody.transform.forward;
        right.y = 0f;
        forward.y = 0f;

        Vector3 adjustedInput = right.normalized * input.x + forward.normalized * input.z;

        //Apply to rigidbody.
        _rigidbody.AddForce(adjustedInput * _moveSpeed, ForceMode.Force);
    }

    [Command]
    private void CmdSendInput(Vector3 input)
    {
        ProcessInput(input);
    }
}
