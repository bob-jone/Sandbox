using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerCameraController : NetworkBehaviour
{
    [SerializeField] 
    private CinemachineFreeLook freeLookCamera = null;

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        enabled = true;
        freeLookCamera.gameObject.SetActive(true);
    }

    // The client callback tag means the server won't mess around with it
    [ClientCallback]

    private void OnEnable()
    {

    }
    [ClientCallback]
    private void OnDisable()
    {
        
    }
}
