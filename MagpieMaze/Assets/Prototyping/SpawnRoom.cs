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

public class SpawnRoom : MazeCellReplacer
{
    //The player, for easy reference
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        //If the player is around, save them for easy reference
        player = GameObject.FindWithTag("Player");
    }

    public override void Initialize(MazeCell centerReplacee, params MazeCell[] replacees)
    {
        //Initialize as normal
        base.Initialize(centerReplacee, replacees);

        //But deparent the player
        player.transform.parent = null;

        //and deactivate all of the walls
        NorthWall.SetActive(false);
        SouthWall.SetActive(false);
        EastWall.SetActive(false);
        WestWall.SetActive(false);
    }

    // Called when no cameras can see this script's gameObject anymore
    void OnBecameInvisible()
    {
        Debug.Log("Spawn Room can no longer be seen");
        //this.Denitialize();
    }
}
