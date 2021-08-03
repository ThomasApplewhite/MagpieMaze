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

public class SpawnRoom : MonoBehaviour
{
    [Header("Instance GameObjects")]
    [Tooltip("The player's GameObject")]
    public GameObject player;

    [Tooltip("The MazeCell of the White Room where the player sits while the maze is made")]
    public MazeCell whiteRoom;

    [Tooltip("The Maze of the starting Hallway")]
    public Maze hallway;

    [Tooltip("The actual Maze itself")]
    public Maze mazeProper;

    [Header("Prefab GameObjects")]
    [Tooltip("The NclidianController prefab")]
    public GameObject Nclidian;

    void Start()
    {
        StartCoroutine(BeingSpawnProcess());
    }

    //When the game starts (and the spawn process begins...)
    IEnumerator BeingSpawnProcess()
    {
        //Move the player to the white room
        player.transform.position = whiteRoom.anchorCoord;

        //wait for the hallway and the maze to be ready
        yield return new WaitUntil( () => 
            mazeProper.initialized == Maze.MazeStatus.GENERATED && 
            hallway.initialized == Maze.MazeStatus.GENERATED
        );

        //Replace the farthest cell in the hallway and a random cell in the real maze with a portal
        MazeCell a = mazeProper.GetRandomCellWithPadding(3);    //random cell
        MazeCell b = hallway.length > hallway.width ?   //farthest cell
            hallway.GetCell(0, hallway.length - 1) : 
            hallway.GetCell(hallway.width - 1, 0);

        //Make the actual portals and do the actual replacement
        Instantiate(Nclidian).GetComponent<NclidianController>().PlacePortals(
            new MazeNeighbors(a, mazeProper),
            new MazeNeighbors(b, hallway),
            NclidianController.ReplacementState.RANDOM,
            NclidianController.ReplacementState.DIRECT
        );

        Debug.Log($"Teleporting player to {hallway.GetCell(0, 0).anchorCoord}");
        //Move the player to the 0 0 cell of the hallway maze
        player.transform.SetPositionAndRotation(hallway.GetCell(0, 0).anchorCoord, Quaternion.identity);

        //that's it, for now
    }
}
