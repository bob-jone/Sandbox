using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BeardedManStudios.Forge.Networking.Generated;

public class Player : PlayerBehavior
{

    /// <summary>
	/// The speed that the player should move by when there are axis inputs
	/// </summary>
     

	public float speed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If this is not owned by the current network client then it needs to
        // assign it to the position and rotation specified
        if (!networkObject.IsOwner)
        {
            // Assign the position of the player to the position sent on the network
            transform.position = networkObject.position;

            // Assign the rotation of the player to the rotation sent on the network
            transform.rotation = networkObject.rotation;

            // Stop the function here and don't run any more code in this function
            return;
        }

        // Get the movement based on the axis input values
        Vector3 translation = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        // Scale the speed to normalize for processors
        translation *= speed * Time.deltaTime;

        // Move the player by the given translation
        transform.position += translation;

        // Just a random rotation on all axis
        transform.Rotate(new Vector3(speed, speed, speed) * 0.25f);

        // Since we are the owner, tell the network the updated position
        networkObject.position = transform.position;

        // Since we are the owner, tell the network the updated rotation
        networkObject.rotation = transform.rotation;

        // Note: Forge Networking takes care of only sending the delta, so there
        // is no need for you to do that manually
    }
}
