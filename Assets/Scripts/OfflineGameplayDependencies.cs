using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineGameplayDependencies : MonoBehaviour
{

    // Todo - destroy this on server because it is only meant for client

    [Tooltip("Canvas for spawning.")]
    [SerializeField]
    private SpawningCanvas _spawningCanvas;

    // Canvas for spawning

    public static SpawningCanvas SpawningCanvas { get { return _instance._spawningCanvas; } }



    // Singleton instance of this script.
    private static OfflineGameplayDependencies _instance;

    private void Awake()
    {
        _instance = this;
    }
}
