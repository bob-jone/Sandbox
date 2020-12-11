using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovementController : NetworkBehaviour
{

    [SerializeField]
    private float _moveSpeed = 5f;

    // Rigidbody on this object.
    private Rigidbody _rigidbody;
    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;

    private Vector3 movementVector;


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
        Debug.Log($"Set movement vector to " + context);
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

    private void ProcessInput(Vector3 input)
    {
        //Add force first.
        // input *= _moveSpeed;

        //Apply to rigidbody.
        // _rigidbody.AddForce(input, ForceMode.Force);
        _rigidbody.MovePosition(transform.position + Time.deltaTime * _moveSpeed *
                transform.TransformDirection(input.x, 0f, input.z));
    }

    [Command]
    private void CmdSendInput(Vector3 input)
    {
        ProcessInput(input);
    }
}
