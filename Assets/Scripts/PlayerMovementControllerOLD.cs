using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovementControllerOLD : NetworkBehaviour
{

    // How much force to apply towards move direction
    [Tooltip("How much force to apply towards move direction.")]
    [SerializeField]
    private float walkSpeed = 5f;

    // Keep track of current player speed
    public float currentSpeed;

    // How much force to apply towards move direction
    [Tooltip("How much force to apply to player jump.")]
    [SerializeField]
    private float jumpForce = 5f;

    // Jump force mode setting
    [Tooltip("Jump force mode setting.")]
    [SerializeField]
    public ForceMode appliedForceMode;

    // Bool used to not let players jump infinitely
    public bool playerIsJumping;

    // Maximum allowed jumps
    private const int MaxAllowJump = 2;
    // Current number of jumps processed
    private int m_currentNumberOfJumpsMade;

    // Ground layer mask we get later to raycast to for jumping
    private int m_groundLayerMask;

    public string groundLayerMask;


    // Store last input so we don't have to send the input every frame
    private Vector2 previousInput;

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

    // Rigidbody on this object.
    private Rigidbody _rigidbody;

    // ReactivePhysicsObject on this object
    private ReactivePhysicsObject _rpo;

    public override void OnStartAuthority()
    {
        enabled = true;

        base.OnStartAuthority();
        _rigidbody = GetComponent<Rigidbody>();
        _rpo = GetComponent<ReactivePhysicsObject>();

        m_groundLayerMask = LayerMask.GetMask();

        Controls.Player.Move.performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        Controls.Player.Move.canceled += ctx => ResetMovement();
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

    [Client]
    private void SetMovement(Vector2 movement)
    {
        previousInput = movement;
    }

    [Client]
    private void ResetMovement()
    {
        previousInput = Vector2.zero;
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

    // Checks if the client wants to move.
    private void CheckMove()
    {
        Vector3 direction = new Vector3(
            previousInput.x,
            0f,
            previousInput.y
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
        //Add force first.
        input *= walkSpeed;

        //Add gravity to help keep the object dowm.
        // input += Physics.gravity * 3f;

        Vector3 zMov = this.transform.right;
        Vector3 xMov = this.transform.forward;
        zMov.y = 0f;
        xMov.y = 0f;

        Vector3 movement = zMov.normalized * previousInput.x + xMov * previousInput.y;

        //Apply to rigidbody.
        _rigidbody.AddForce(movement, ForceMode.Force);
    }

    // Tells the server which inputs to use
    [Command]
    private void CmdSendInput(Vector3 input)
    {
        ProcessInput(input);
    }
}
