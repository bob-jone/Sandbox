using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class PlayerInstance : NetworkBehaviour
{
    // Dispatched when this player instance is set as the local player
    public static event Action<PlayerInstance> OnPlayerInstance;

    // PlayerSpawner on this object
    public PlayerSpawner PlayerSpawner { get; private set; }


    // Called when the local player object has been set up.
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        PlayerSpawner = GetComponent<PlayerSpawner>();

        OnPlayerInstance?.Invoke(this);
    }
}
