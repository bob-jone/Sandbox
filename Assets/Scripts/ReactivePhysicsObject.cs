using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Mirror;

public class ReactivePhysicsObject : NetworkBehaviour
{
    // Data sent to observers to keep object in sync
    private struct SyncData
    {
        public SyncData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
        {
            Position = position;
            Rotation = rotation;
            Velocity = velocity;
            AngularVelocity = angularVelocity;
        }

        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
        public Vector2 Velocity2D { get { return new Vector2(Velocity.x, Velocity.y); } }

        public Vector3 AngularVelocity;
        public float AngularVelocity2D { get { return AngularVelocity.z; } }
    }

    // Interval types to determine when to synchronize data
    [System.Serializable]
    private enum IntervalTypes : int
    {
        Timed = 0,
        FixedUpdate = 1
    }

    [Tooltip("True to synchronize using localSpace rather than worldSpace. If you are to child this object throughout it's lifespan using worldspace is recommended. However, when using worldspace synchronization may not behave properly on VR. LocalSpace is the default.")]
    [SerializeField]
    private bool _useLocalSpace = true;

    // How to operate synchronization timings. Timed will synchronized every specified interval while FixedUpdate will synchronize every FixedUpdate
    [Tooltip("How to operate synchronization timings. Timed will synchronized every specified interval while FixedUpdate will synchronize every FixedUpdate.")]
    [SerializeField]
    private IntervalTypes _intervalType = IntervalTypes.Timed;

    // How often to synchronize this transform
    [Tooltip("How often to synchronize this transform. If being used as a controller it's best to set this to the same rate that your controller sends movemenet.")]
    [Range(0.01f, 0.5f)]
    [FormerlySerializedAs("_syncInterval")]
    [SerializeField]
    private float _synchronizeInterval = 0.1f;

    // True to synchronize using the reliable channel. False to synchronize using the unreliable channel. Your project must use 0 as reliable, and 1 as unreliable for this to function properly. This feature is not supported on TCP transports.
    [Tooltip("True to synchronize using the reliable channel. False to synchronize using the unreliable channel. Your project must use 0 as reliable, and 1 as unreliable for this to function properly.")]
    [SerializeField]
    private bool _reliable = true;

    // True to synchronize data anytime it has changed. False to allow greater differences before synchronizing. Given that rigidbodies often shift continuously it's recommended to leave this false to not flood the network.
    [Tooltip("True to synchronize data anytime it has changed. False to allow greater differences before synchronizing. Given that rigidbodies often shift continuously it's recommended to leave this false to not flood the network.")]
    [SerializeField]
    private bool _preciseSynchronization = false;

    // How strictly to synchronize this object when owner. Lower values will still keep the object in synchronization but it may take marginally longer for the object to correct if out of synchronization. It's recommended to use higher values, such as 0.5f, when using fast intervals. Default value is 0.1f.
    [Tooltip("How strictly to synchronize this object when owner. Lower values will still keep the object in synchronization but it may take marginally longer for the object to correct if out of synchronization. It's recommended to use higher values, such as 0.5f, when using fast intervals. Default value is 0.1f.")]
    [Range(0.01f, 0.75f)]
    [SerializeField]
    private float _strength = 0.5f;

    // SyncData client should move towards.
    private SyncData? _syncData = null;

    // Rigidbody on this object. May be null.
    private Rigidbody _rigidbody;

    // Last SyncData values sent by server.
    private SyncData _lastSentSyncData;

    // Next time server can send SyncData.
    private double _nextSendTime = 0f;

    // True if a reliable packet has been sent for most recent values.
    private bool _reliableSent = false;

    // Last time SyncData was updated.
    private float _lastReceivedTime = 0f;

    // How much time must pass after receiving a packet before snapping to the last received value.
    private float TIME_PASSED_SNAP_VALUE = 3f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        /* Assign current data as a new sync data. Server will update as needed.
         * This is to stop player from moving objects which haven't received
         * an update yet. */
        _syncData = new SyncData(transform.GetPosition(_useLocalSpace), transform.GetRotation(_useLocalSpace), Vector3.zero, Vector3.zero);
    }

    private void FixedUpdate()
    {
        if (base.isServer)
        {
            CheckSendSyncData();
        }
        if (base.isClientOnly)
        {
            MoveTowardsSyncDatas();
        }
    }

    private void Update()
    {
        if (base.isServer)
        {
            CheckSendSyncData();
        }
        if (base.isClientOnly)
        {
            MoveTowardsSyncDatas();
        }
    }

    // Temporarily reduces aggressive to stay in synchronization. Best to use this after your object is moved using it's controller.
    [Client]
    public void ReduceAggressiveness()
    {
        _lastReceivedTime = Time.time;
    }

    // Clears data to synchronize towards allowing this gameObject to teleport. The object should be teleported on the server as well.
    [Client]
    public void AllowTeleport()
    {
        _syncData = null;
    }

    // Moves towards most recent sync data values.
    private void MoveTowardsSyncDatas()
    {
        if (_syncData == null)
            return;
        if (_strength == 0f)
            return;
        // If data matches no reason to continue.
        if (SyncDataMatchesObject(_syncData.Value, _preciseSynchronization))
        {
            /* Also reset data received time so smoothing
             * will reoccur if object is bumped into, rather
             * than snapping to last position. */
            return;
        }

        float timePassed = Time.time - _lastReceivedTime;

        //If to snap.
        if (timePassed > TIME_PASSED_SNAP_VALUE)
        {
            transform.SetPosition(_useLocalSpace, _syncData.Value.Position);
            transform.SetRotation(_useLocalSpace, _syncData.Value.Rotation);

            _rigidbody.velocity = _syncData.Value.Velocity;
            _rigidbody.angularVelocity = _syncData.Value.AngularVelocity;
            Physics.SyncTransforms();
        }
        //Do not snap yet.
        else
        {
            //If owner use configured strength, otherwise always use 1f.
            float strength = (base.hasAuthority) ? _strength : 1f;
            float accumulated = timePassed * strength;

            //Smoothing multiplier based on sync interval and frame rate.
            float deltaMultiplier = (Time.deltaTime / ReturnSyncInterval());
            float distance;

            //Modify transform properties in regular update for smoother visual transitions.
            if (!Time.inFixedTimeStep)
            {
                //Position.
                distance = Vector3.Distance(transform.GetPosition(_useLocalSpace), _syncData.Value.Position);
                distance *= distance;
                transform.SetPosition(_useLocalSpace,
                    Vector3.MoveTowards(transform.GetPosition(_useLocalSpace), _syncData.Value.Position, accumulated * distance));

                //Rotation
                distance = Quaternion.Angle(transform.rotation, _syncData.Value.Rotation);
                transform.SetRotation(_useLocalSpace,
                    Quaternion.RotateTowards(transform.GetRotation(_useLocalSpace), _syncData.Value.Rotation, deltaMultiplier * distance * strength));

                /* Only sync transforms if have authority. This is because
                 * if the client has authority we assume they have the ability
                 * to manipulate the objects transform. */
                if (base.hasAuthority)
                {
                    Physics.SyncTransforms();
                }
            }
            // Move forces in fixed update
            else
            {
                // Angular.
                distance = Vector3.Distance(_rigidbody.angularVelocity, _syncData.Value.AngularVelocity);
                _rigidbody.angularVelocity = Vector3.MoveTowards(_rigidbody.angularVelocity, _syncData.Value.AngularVelocity, deltaMultiplier * distance * strength);
                // Velocity
                distance = Vector3.Distance(_rigidbody.velocity, _syncData.Value.Velocity);
                _rigidbody.velocity = Vector3.MoveTowards(_rigidbody.velocity, _syncData.Value.Velocity, deltaMultiplier * distance * strength);
            }
        }
    }

    // Returns used synchronization interval.
    private float ReturnSyncInterval()
    {
        if (_intervalType == IntervalTypes.FixedUpdate)
            return Time.fixedDeltaTime;
        else
            return _synchronizeInterval;
    }

    // Returns if properties can be sent to clients.
    private bool CanSendProperties(SyncData data, ref bool useReliable)
    {
        bool dataMatches = SyncDataMatchesObject(data, _preciseSynchronization);

        //Send if data doesn't match.
        if (!dataMatches)
        {
            /* Unset ReliableSent so that it will fire
             * once object has settled, assuming not using a reliable
             * transport. */
            _reliableSent = false;
            return true;
        }
        //If data matches.
        else
        {
            //If using unreliable, but reliable isn't sent yet.
            if (!_reliable && !_reliableSent)
            {
                useReliable = true;
                _reliableSent = true;
                return true;
            }
            //Either using reliable or reliable already sent.
            else
            {
                return false;
            }
        }
    }

    // Returns if the specified SyncData matches values on this object.
    private bool SyncDataMatchesObject(SyncData data, bool precise)
    {
        bool transformMatches = (PositionMatches(data, precise) && RotationMatches(data, precise));

        bool velocityMatches = false;
        /* If transform matches then we must check
         * also if physics match. If transform does not match there's
         * no reason to check physics as an update is required regardless. */
        if (transformMatches)
            velocityMatches = VelocityMatches(data, precise);

        return (transformMatches && velocityMatches);
    }

    // Returns if this transform position matches data.
    private bool PositionMatches(SyncData data, bool precise)
    {
        if (precise)
        {
            return (transform.GetPosition(_useLocalSpace) == data.Position);
        }
        else
        {
            float dist = Vector3.SqrMagnitude(transform.GetPosition(_useLocalSpace) - data.Position);
            return (dist < 0.0001f);
        }
    }

    // Returns if this transform rotation matches data.
    private bool RotationMatches(SyncData data, bool precise)
    {
        if (precise)
            return transform.GetRotation(_useLocalSpace).Near(data.Rotation);
        else
            return transform.GetRotation(_useLocalSpace).Near(data.Rotation, 1f);
    }

    private bool VelocityMatches(SyncData data, bool precise)
    {
        if (precise)
        {
            return ((_rigidbody.velocity == data.Velocity) && (_rigidbody.angularVelocity == data.AngularVelocity));
        }
        else
        {
            float dist;
            dist = Vector3.SqrMagnitude(_rigidbody.velocity - data.Velocity);
            //If velocity is outside tolerance then return false early.
            if (dist >= 0.0025f)
                return false;
            //Angular.
            dist = Vector3.SqrMagnitude(_rigidbody.angularVelocity - data.AngularVelocity);
            return (dist < 0.0025f);
        }
    }

    // Checks if SyncData needs to be sent over the network.
    private void CheckSendSyncData()
    {
        // Timed interval.
        if (_intervalType == IntervalTypes.Timed)
        {
            if (Time.inFixedTimeStep)
                return;

            if (Time.time < _nextSendTime)
                return;
        }
        // Fixed interval.
        else
        {
            if (!Time.inFixedTimeStep)
                return;
        }

        bool useReliable = _reliable;
        //Values haven't changed.
        if (!CanSendProperties(_lastSentSyncData, ref useReliable))
            return;

        /* If here a new sync data needs to be sent. */

        //Set sync data being set, and next time data can send.
        Vector3 velocity;
        velocity = _rigidbody.velocity;

        Vector3 angularVelocity;
        angularVelocity = _rigidbody.angularVelocity;

        _lastSentSyncData = new SyncData(transform.GetPosition(_useLocalSpace), transform.GetRotation(_useLocalSpace), velocity, angularVelocity);
        //Set time regardless if using interval or not. Quicker than running checks.
        _nextSendTime = NetworkTime.time + _synchronizeInterval;

        //Send new SyncData to clients.
        if (useReliable)
            RpcUpdateSyncDataReliable(_lastSentSyncData);
        else
            RpcUpdateSyncDataUnreliable(_lastSentSyncData);
    }

    // Updates SyncData on clients.
    [ClientRpc(channel = 0)]
    private void RpcUpdateSyncDataReliable(SyncData data)
    {
        ServerSyncDataReceived(data);
    }

    // Updates SyncData on clients.
    [ClientRpc(channel = 1)]
    private void RpcUpdateSyncDataUnreliable(SyncData data)
    {
        ServerSyncDataReceived(data);
    }

    // Called when client 
    private void ServerSyncDataReceived(SyncData data)
    {
        //If received on client host, no need to update.
        if (base.isServer)
            return;

        _lastReceivedTime = Time.time;
        _syncData = data;
    }
}
