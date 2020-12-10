using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class PlayerSpawner : NetworkBehaviour
{
    [System.Serializable]
    private class CharacterPrefab
    {
        
        [Tooltip("Prefab to spawn for the character type.")]
        [SerializeField]
        private GameObject _prefab;

        // Prefab to spawn for the character type
        public GameObject Prefab { get { return _prefab; } }

        // Character type the prefab is for.
        [Tooltip("Character type the prefab is for.")]
        [SerializeField]
        private CharacterTypes _characterType;

        // Character type the prefab is for.
        public CharacterTypes CharacterType { get { return _characterType; } }
    }

    // Prefabs for each character type
    [Tooltip("Prefabs for each character type.")]
    [SerializeField]
    private List<CharacterPrefab> _characterPrefabs = new List<CharacterPrefab>();

    //Called when the local player object has been set up.
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        //Show the spawning canvas once connected as an local player.
        OfflineGameplayDependencies.SpawningCanvas.Show();
    }

    // Tries to spawn the requested character type.
    public void TrySpawn(CharacterTypes characterType)
    {
        CmdTrySpawn((int)characterType);
    }

    // Tries to spawn the specified character type.
    [Command]
    private void CmdTrySpawn(int characterType)
    {
        // Convert for readability.
        CharacterTypes characterEnum = (CharacterTypes)characterType;

        int index = _characterPrefabs.FindIndex(x => x.CharacterType == characterEnum);
        // If index isn't found then you likely forgot to setup the character type prefabs.
        if (index == -1)
            return;

        // No prefab set for index, you likely forgot to specify a prefab for the character type.
        if (_characterPrefabs[index].Prefab == null)
            return;

        // Choose a random position.
        Vector3 pos = new Vector3(UnityEngine.Random.Range(-2f, 2f), 1f, UnityEngine.Random.Range(-2f, 2f));
        GameObject result = Instantiate(_characterPrefabs[index].Prefab, pos, Quaternion.identity);

        // Spawn over server giving authority to the client calling this command.
        NetworkServer.Spawn(result, base.netIdentity.connectionToClient);

        // Tell player their character was spawned. Could pass in an object here.
        TargetCharacterSpawned();
    }

    // Notifies a player their character spawned, providing the gameobject of the spawned character.
    [TargetRpc]
    private void TargetCharacterSpawned()
    {
        OfflineGameplayDependencies.SpawningCanvas.Hide();
    }
}
