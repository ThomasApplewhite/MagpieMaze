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

public class MazeCell : MonoBehaviour
{
    public GameObject NorthWall;
    public GameObject SouthWall;
    public GameObject EastWall;
    public GameObject WestWall;

    //This cell's coordinates in its maze
    public Vector2Int Coordinate { get; protected set; }

    //This cell's approximate coordinate in world space
    //The player can be placed at this position to be in this cell
    public Vector3 anchorCoord {
        get 
        {
            //This position is a little bit off of the ground, so be careful
            //It will spawn the player in the air
            var compositePos = 
                NorthWall.transform.position + 
                SouthWall.transform.position + 
                EastWall.transform.position + 
                WestWall.transform.position;
            return compositePos / 4f;
        }
    }
    
    //These two methods connect or disconnect two cells at a shared side
    private static bool SyncWallNorthSouth(MazeCell North, MazeCell South, bool wallState)
    {
        if(North.Coordinate.y - South.Coordinate.y == 1)
        {
            North.SouthWall?.SetActive(wallState);
            South.NorthWall?.SetActive(wallState);
            return true;
        }
        else
        {
            Debug.LogWarning("MazeCell.ConnectNorthSouth: Cells are not correctly aligned");
        }
        return false;
    }

    private static bool SyncWallEastWest(MazeCell East, MazeCell West, bool wallState)
    {
        if(East.Coordinate.x - West.Coordinate.x == 1)
        {
            East.WestWall?.SetActive(wallState);
            West.EastWall?.SetActive(wallState);
            return true;
        }
        else
        {
            Debug.LogWarning("MazeCell.ConnectEastWest: Cells are not correctly aligned");
        }
        return false;
    }

    public virtual void Initialize(Vector2Int coord)
    {
        this.Coordinate = coord;
    }

    //Makes this cell and otherCell connect by destroying the walls between them
    //If at least one of these cells is "in", the other will be "in" also
    public bool Connect(MazeCell otherCell)
    {
        //this.Coordinate is second, so all of these checks will judge relative to this
        Vector2 compositeCoord = otherCell.Coordinate - this.Coordinate;

        if(compositeCoord.x == 1)
        {
            MazeCell.SyncWallEastWest(otherCell, this, false);
        }
        else if(compositeCoord.x == -1)
        {
            MazeCell.SyncWallEastWest(this, otherCell, false);
        }
        else if(compositeCoord.y == 1)
        {
            MazeCell.SyncWallNorthSouth(otherCell, this, false);
        }
        else if(compositeCoord.y == -1)
        {
            MazeCell.SyncWallNorthSouth(this, otherCell, false);
        }
        else
        {
            Debug.LogWarning($"MazeCell.Connect: The two cells at this.{this.Coordinate}" +
                $" and other.{otherCell.Coordinate} are not adjacent");
            return false;
        }

        return true;
    }

    //Makes this cell and otherCell disconnect by restoring the walls between them
    //If a cell is disconnected from all of its neighbors, it becomes "out" of the maze
    public bool Disconnect(MazeCell otherCell)
    {
        //this.Coordinate is second, so all of these checks will judge relative to this
        Vector2 compositeCoord = otherCell.Coordinate - this.Coordinate;

        if(compositeCoord.x == 1)
        {
            MazeCell.SyncWallEastWest(otherCell, this, true);
        }
        else if(compositeCoord.x == -1)
        {
            MazeCell.SyncWallEastWest(this, otherCell, true);
        }
        else if(compositeCoord.y == 1)
        {
            MazeCell.SyncWallNorthSouth(otherCell, this, true);
        }
        else if(compositeCoord.y == -1)
        {
            MazeCell.SyncWallNorthSouth(this, otherCell, true);
        }
        else
        {
            Debug.LogWarning($"MazeCell.Disconnect: The two cells at this.{this.Coordinate}" +
                $" and other.{otherCell.Coordinate} are not adjacent");
            return false;
        }

        return true;
    }     
}
