/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modifcation and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You shoould have recieved a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

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
