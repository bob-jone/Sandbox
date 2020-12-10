using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.InputSystem;

public class PlayerMovementController : NetworkBehaviour
{
    #region Serialized.
    // How much force to apply towards move direction.
    [Tooltip("How much force to apply towards move direction.")]
    [SerializeField]
    private float _directionalForce = 1f;
    #endregion

    #region Private.
    // Rigidbody on this object.
    private Rigidbody _rigidbody;
    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;

    private Vector2 inputVector;

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
        Debug.Log($"Move input: {inputVector}");
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
            );

        if (direction == Vector3.zero)
            return;

        //Move locally.
        ProcessInput(direction);
        _rpo.ReduceAggressiveness();

        //Only send inputs to server if not a client host.
        if (base.isClientOnly)
            CmdSendInput(direction);
    }

    // Applies an input direction to the rigidbody.
    private void ProcessInput(Vector3 input)
    {
        // Debug.Log("in ProcessInput method. Input is " + input);


        //Add force first.
        input *= _directionalForce;
        //Add gravity to help keep the object dowm.
        // input += Physics.gravity * 3f;

        //Apply to rigidbody.
        // _rigidbody.AddForce(input, ForceMode.Force);

        _rigidbody.MovePosition(transform.position + Time.deltaTime *
                transform.TransformDirection(input));
    }

    // Tells the server which inputs to move.
    [Command]
    private void CmdSendInput(Vector3 input)
    {
        ProcessInput(input);
    }
}
