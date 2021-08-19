/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class Minotaur : MonoBehaviour
{
    //The Minotaur's NavMeshAgent component
    private NavMeshAgent agent;

    //A reference to the player, for ease
    private GameObject player;

    //What happens when the Minotaur collides with the player
    private Action<GameObject> OnPlayerContact;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");

        OnPlayerContact = KillPlayer;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(agent.enabled) agent.SetDestination(player.transform.position);
    }

    void OnCollisionEnter(Collision collided)
    {
        if(collided.gameObject == player)
        {
            OnPlayerContact.Invoke(collided.gameObject);
        }
    }

    void KillPlayer(GameObject player)
    {
        player.SendMessage("Kill");
    }
}
