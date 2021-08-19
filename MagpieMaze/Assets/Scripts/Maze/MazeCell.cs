/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

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

    //This cell's connections
    public MazeCell NorthConnection { get; private set; }
    public MazeCell SouthConnection { get; private set; }
    public MazeCell EastConnection { get; private set; }
    public MazeCell WestConnection { get; private set; }

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

    public enum WallDirection
    {
        NORTH,
        SOUTH,
        EAST,
        WEST
    }
    
    //These two methods connect or disconnect two cells at a shared side
    private static bool SyncWallNorthSouth(MazeCell North, MazeCell South, bool wallState)
    {
        if(North.Coordinate.y - South.Coordinate.y == 1)
        {
            North.SouthWall?.SetActive(wallState);
            South.NorthWall?.SetActive(wallState);
            //If the walls are going away, then the cells are getting connected
            if(!wallState)
            {
                North.SouthConnection = South;
                South.NorthConnection = North;
            }
            else
            {
                North.SouthConnection = null;
                South.NorthConnection = null;
            }

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
            //If the walls are going away, then the cells are getting connected
            if (!wallState)
            {
                East.WestConnection = West;
                West.EastConnection = East;
            }
            else
            {
                East.WestConnection = null;
                West.EastConnection = null;
            }
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

    public bool IsConnected(WallDirection dir)
    {
        switch(dir)
        {
            case WallDirection.NORTH:
                return NorthConnection != null;

            case WallDirection.SOUTH:
                return SouthConnection != null;

            case WallDirection.EAST:
                return EastConnection != null;
            
            case WallDirection.WEST:
                return WestConnection != null;
            
            default:
                Debug.LogError("MazeCell.IsConnected: Invalid WallDirection!");
                return false;
        }
    }

    //Makes this cell and otherCell connect by destroying the walls between them
    //If at least one of these cells is "in", the other will be "in" also
    public bool Connect(MazeCell otherCell)
    {
        if(otherCell == null)
        {
            Debug.LogWarning($"MazeCell{Coordinate}.Connect: otherCell is null!");
            return false;
        }

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
        if(otherCell == null)
        {
            Debug.LogWarning($"MazeCell{Coordinate}.Disconnect: otherCell is null!");
            return false;
        }

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

    //Disconnects a cell from all of its connections
    public void DisconnectAll()
    {
        this.Disconnect(this.NorthConnection);
        this.Disconnect(this.SouthConnection);
        this.Disconnect(this.EastConnection);
        this.Disconnect(this.WestConnection);
    }     

    //Connects a cell to all of copiedCell's connections
    public void CopyConnections(MazeCell copiedCell)
    {
        this.Connect(copiedCell.NorthConnection);
        this.Connect(copiedCell.SouthConnection);
        this.Connect(copiedCell.EastConnection);
        this.Connect(copiedCell.WestConnection);
    }
}
