/*Copyright (c) 2021 Magpie Paulsen

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>
Code originally written by Thomas Applewhite*/

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
    public MazeGenerator hallway;

    [Tooltip("The actual Maze itself")]
    public MazeGenerator mazeProper;

    [Header("Prefab GameObjects")]
    [Tooltip("The NclidianController prefab")]
    public GameObject Nclidian;

    //When the game starts (and the spawn process begins...)
    public void BeingSpawn()
    {
        //Move the player to the white room
        player.transform.position = whiteRoom.anchorCoord;

        //that's it, for now
    }

    //When the actual maze is ready...
    public void EndSpawn()
    {
        //Replace the farthest cell in the hallway and a random cell in the real maze with a portal
        MazeCell a = mazeProper.GetRandomCellWithPadding(3);    //random cell
        MazeCell b = hallway.MazeLength > hallway.MazeWidth ?   //farthest cell
            hallway.GetCell(hallway.MazeLength, 0) : 
            hallway.GetCell(0, hallway.MazeWidth);

        //Make the actual portals and do the actual replacement
        Instantiate(Nclidian).GetComponent<NclidianController>().PlacePortals(
            new MazeNeighbors(a, mazeProper.Maze),
            new MazeNeighbors(b, hallway.Maze)
        );

        //Move the player to the 0 0 cell of the hallway maze
        player.transform.position = hallway.GetCell(0, 0).anchorCoord;
    }
}
