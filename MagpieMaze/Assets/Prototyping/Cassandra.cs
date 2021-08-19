/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modifcation and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You shoould have recieved a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Collider))]
public class Cassandra : MonoBehaviour
{
    [Tooltip("How close to her selected wander point Cassandra needs to be before picking a new wander point")]
    public float wanderDestinationCuttoff = 1f;

    //The maze that Cassandra is wandering
    private Maze maze;

    //Cassandra's NavMeshAgent
    private NavMeshAgent agent;

    // Start is called before the first frame update
    void Awake()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();

        StartCoroutine(PathReset());
    }

    //Tells Cassandra to find a new place to go to every time she finishes a wander
    IEnumerator PathReset()
    {
        while(true)
        {
            yield return new WaitWhile( () => agent.pathPending );
            yield return new WaitUntil( () => 
                agent.enabled
                ? agent.remainingDistance <= wanderDestinationCuttoff 
                : false
            );

            Debug.Log($"{this.gameObject.name}.Cassandra.PathReset: Path Complete. Restarting wander...");

            BeginWander();
        }
    }

    public void BeginWander()
    {
        BeginWander(maze);
    }

    //Picks a random point in the maze and tells Cassandra to go to it
    public void BeginWander(Maze wanderMaze)
    {
        //If Cassandra knows no maze...
        if(maze == null)
        {
            //And wasn't provided one...
            if(wanderMaze == null)
            {
                //That's a problem
                Debug.LogError($"{this.gameObject.name}.Cassandra.BeginWander: No maze to wander!");
            }
            //And was provided one...
            else
            {
                //remember it for later
                maze = wanderMaze;
            }
        }

        //var destination = wanderMaze.GetRandomCell().anchorCoord;
        //Debug.Log($"{this.gameObject.name}.Cassandra.BeginWander: Wandering to {destination}");
        agent.SetDestination(wanderMaze.GetRandomCell().anchorCoord);
    }
}
