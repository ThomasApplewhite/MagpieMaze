using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPlayerToCoordinate : MonoBehaviour
{
    public Vector3 teleportDestination = new Vector3(0f, 0f, 0f);

    void OnTriggerEnter(Collider collided)
    {
        collided.gameObject.transform.position = teleportDestination;
    }
}
