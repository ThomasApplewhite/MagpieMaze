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
    public Vector2Int Coordinate { get; private set; }

    public static void ConnectNorthSouth(MazeCell North, MazeCell South)
    {
        Destroy(North.SouthWall);
        Destroy(South.NorthWall);
    }

    public static void ConnectEastWest(MazeCell East, MazeCell West)
    {
        Destroy(East.WestWall);
        Destroy(West.EastWall);
    }

    public virtual void Initialize(Vector2Int coord)
    {
        this.Coordinate = coord;
    }

    //Makes this cell and otherCell connect by destroying the walls between them
    //If at least one of these cells is "in", the other will be "in" also
    public void Connect(MazeCell otherCell)
    {
        //this.Coordinate is second, so all of these checks will judge relative to this
        Vector2 compositeCoord = otherCell.Coordinate - this.Coordinate;

        if(compositeCoord.x == 1)
        {
            MazeCell.ConnectEastWest(otherCell, this);
        }
        else if(compositeCoord.x == -1)
        {
            MazeCell.ConnectEastWest(this, otherCell);
        }
        else if(compositeCoord.y == 1)
        {
            MazeCell.ConnectNorthSouth(otherCell, this);
        }
        else if(compositeCoord.y == -1)
        {
            MazeCell.ConnectNorthSouth(this, otherCell);
        }
        else
        {
            Debug.LogError($"MazeCell.Connet: The two cells at this.{this.Coordinate}" +
                $" and other.{otherCell.Coordinate} are not adjacent");
        }
    }     
}
