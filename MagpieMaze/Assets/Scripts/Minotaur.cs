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
using UnityEngine.Animations;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Animator))]
public class Minotaur : MonoBehaviour
{
    [Tooltip("How close The Minotaur needs to be to its destination before it looks for a new one")]
    public float wanderDestinationCuttoff = 1f;

    [Tooltip("How close The Minotaur needs to be to the player before trying to charge them again")]
    public float attackDestinationCuttoff = 0.25f;

    //The Minotaur's NavMeshAgent component
    private NavMeshAgent agent;

    //The Minotaur's Animator component
    private Animator anims;

    //A reference to the player, for ease
    private GameObject player;

    //What happens when the Minotaur collides with the player
    private Action<GameObject> OnPlayerContact;

    // Start is called before the first frame update
    void Start()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        anims = this.gameObject.GetComponent<Animator>();
        player = GameObject.FindWithTag("Player");

        OnPlayerContact = KillPlayer;

        StartCoroutine(TrackPlayer());
    }

    // FixedUpdate is called every physics tick, regardless of framerate
    void FixedUpdate()
    {
        anims.SetFloat("m_velocity", agent.velocity.magnitude);
    }

    /*Track to the player's position, but only update the destination continuously if the player
    can be seen. Ideally, such distances will be short, and thus expensive path calculations
    can be avoided.*/
    IEnumerator SearchPlayer()
    {
        while(true)
        {
            Debug.Log("Minotaur.TrackPlayer: calculating new path...");
            do
            {
                //Grab the player's position and make a path
                agent.SetDestination(player.transform.position);
                yield return new WaitWhile(() => agent.pathPending);
            }
            //Keep trying until the path gets made
            while(agent.pathStatus != NavMeshPathStatus.PathComplete);
            
            
            Debug.Log("Minotaur.TrackPlayer: travelling path");
            //then go to it
            yield return new WaitWhile( () => agent.remainingDistance > wanderDestinationCuttoff );

        }
    }

    IEnumerator TrackPlayer()
    {
        RaycastHit raycastHit;

        while(true)
        {
            //Step 1: Grab the player's position
            Debug.Log("Minotaur.TrackPlayer: calculating new path...");
            do
            {
                yield return new WaitUntil( () => agent.enabled );
                agent.SetDestination(player.transform.position);
                
                yield return new WaitWhile( () => agent.pathPending );
            }
            while(agent.pathStatus != NavMeshPathStatus.PathComplete);

            Debug.Log("Minotaur.TrackPlayer: travelling path");

            //Step 2: Go to that spot (not necessarily the player).
            //This happens automatically

            //Step 3: while still travelling...
            while (!agent.enabled || agent.remainingDistance > wanderDestinationCuttoff)
            {
                //Check if the player can be seen by raycasting in their direction
                Physics.Raycast(
                    this.gameObject.transform.position,
                    player.transform.position - this.gameObject.transform.position,
                    out raycastHit
                );

                //If the first thing hit by that raycast is the player...
                if (raycastHit.transform.gameObject == player)
                {
                    Debug.Log("Minotaur.TrackPlayer: Player Detected");
                    yield return ChargePlayer();
                }
                //if it isn't...
                else
                {
                    //keep going, I guess
                    yield return null;
                }
            }
        }
    }

    //walk directly to the player every frame
    IEnumerator ChargePlayer()
    {
        RaycastHit raycastHit;

        do
        {
            yield return null;

            //wait until navAgent is ready to get a path
            yield return new WaitUntil( () => agent.enabled );

            //Set the player as the destination
            //this should be fine as long as the player is nearby
            agent.SetDestination(player.transform.position);

            //Got to that spot
            yield return new WaitUntil( () => !agent.enabled || agent.remainingDistance <= wanderDestinationCuttoff );

            //Check if the player can be seen by raycasting in their direction
            Physics.Raycast(
                this.gameObject.transform.position,
                player.transform.position - this.gameObject.transform.position,
                out raycastHit
            );
        }
        //repeat this process as long as the player is visible
        while(raycastHit.transform.gameObject == player);
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
        player.SendMessage("Kill", this.gameObject);
    }
}
