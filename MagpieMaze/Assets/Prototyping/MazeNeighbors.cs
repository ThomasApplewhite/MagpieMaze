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

using System;
using System.Collections;
using System.Collections.Generic;

//This class stores references to a cell's adjacent neighbors.
//That's it. That's all it does. Nothing else.

public class MazeNeighbors
{

    //The Cell we're judging relative to
    public MazeCell Owner { get; private set; }

    /*And all of its neighbors. Because non-replaced cells never move.
    These don't need to be updated after creation. If there is no neighbor,
    the value will be null, as that's a reference type's default.*/
    public MazeCell North { get; private set; }
    public MazeCell South { get; private set; }
    public MazeCell East { get; private set; }
    public MazeCell West { get; private set; }

    //Determines the maze's neighbors by directly yanking them out of a coordinate array
    //Very fast, this is the better way to a MazeNeighbors this up
    public MazeNeighbors(MazeCell center, MazeCell[,] maze)
    {
        var c = center.Coordinate;
        North = maze[c.x, c.y+1];
        South = maze[c.x, c.y-1];
        East = maze[c.x-1, c.y];
        West = maze[c.x+1, c.y];
    }

    /*Determines the maze's neighbors by searching for each of them in an array
    A good bit slower, and can't tell the difference between a neighbor that isn't
    in the array and that doesn't exist at all, but works directly with radius stuff
    
    From a technical perspective, searching the array for 4 things 1 time takes
    as long as searching the array for 1 thing 4 times. IRL, I'm certain it's more
    efficient to get the array of adjacents and filter them out later
    
    Either way, as long as it's fast enough, it doesn't matter*/
    public MazeNeighbors(MazeCell center, MazeCell[] area)
    {
        //Neighbor location predicate
        Predicate<MazeCell> isNeighbor = (cell) => 
        {
            return Math.Abs(center.Coordinate.x - cell.Coordinate.x) == 1 || 
                Math.Abs(center.Coordinate.y - cell.Coordinate.y) == 1;
        };

        /*The sloppy part, actually figuring out which neighbor is which based on
        coordinate values*/
        Action<MazeCell> assignCell = (cell) => 
        {
            var xDiff = center.Coordinate.x - cell.Coordinate.x;
            var yDiff = center.Coordinate.y - cell.Coordinate.y;

            if(xDiff == 0)
            {
                //Same X, Y is 1, then center is north of cell
                if(yDiff == 1)
                {
                    this.South = cell;
                }
                //Same X, Y is -1, then center is south of cell
                else
                {
                    this.North = cell;
                }
            }
            else
            {
                //Same Y, X is 1, then center is east of cell
                if(xDiff == 1)
                {
                    this.West = cell;
                }
                //Same Y, X is -1, then center is west of cell
                else
                {
                    this.East = cell;
                }
            }
        };

        /*Actual array search. Convert the results to a Queue for easy searching
        This is totally fine because MazeCell is a reference type.*/
        Queue<MazeCell> neighbors = new Queue<MazeCell>(Array.FindAll(area, isNeighbor));

        //Pop off cells one at a time and assign them to neighbor slots
        //god functional programming is so convinient
        while(neighbors.Count != 0)
        {
            assignCell(neighbors.Dequeue());
        }
    }
}