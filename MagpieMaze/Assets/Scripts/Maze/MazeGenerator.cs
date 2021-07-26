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
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    [Header("Generation Parameters")]
    [Tooltip("How long the maze should be")]
    public int MazeLength = 10;
    
    [Tooltip("How wide the maze should be")]
    public int MazeWidth = 10;

    [Tooltip("How big each cell should be")]
    public float MazeScale = 3f;

    [Tooltip("How many cells should be generated at a time (bigger number = faster but needs better computer)")]
    public int MazeGenStepSize = 10;

    [Header("Spawn Parameters")]
    [Tooltip("The location of the Player's spawn point")]
    public Vector3 playerSpawn = new Vector3(3f, 1f, 3f);

    //[Tooltip("The Player's spawn room")]
    //public GameObject PlayerSpawnRoom;

    [Tooltip("The location of the Minotaur's spawn point")]
    public Vector3 minotaurSpawn = new Vector3(66f, 1f, 66f);

    [Header("Prefab Parameters")]
    [Tooltip("The prefab of a Maze Cell")]
    public GameObject MazeCell;

    [Tooltip("The prefab of the floor")]
    public GameObject MazeFloor;

    [Tooltip("The prefab of the Minotaur")]
    public GameObject Minotaur;

    [Header("Dev Parameters")]
    [Tooltip("The prefab of a Replacer to just replace some things")]
    public GameObject ReplacerCell;

    [Tooltip("The size of the XbyX square that the above replacer cell takes up")]
    public int replacerSize = 3;
    
    [Tooltip("The Prefab of an Nclidian Duo")]
    public GameObject Nclidian;

    //The NavMesh of the floor
    private NavMeshSurface nav;

    //The Array of MazeCells that make up the maze
    private MazeCell[,] Maze;

    //The random number generator, for random number things
    private System.Random Rnd;

    // Start is called before the first frame update
    void Start()
    {
        Maze = new MazeCell[MazeWidth, MazeLength];
        Rnd = new System.Random();

        StartCoroutine(ComissionMaze());
    }

    /*
    ---Notes about Maze Generation---
    The origin cell is (0, 0), which is at 0, 0, 0 relative to the Maze parent object
    X Coordinates increment Eastward
    Y Coordinates increment Northward
    */

    ///GENERATION METHODS--------------------------------------------------------------------------
    IEnumerator ComissionMaze()
    {
        BuildFloor();

        yield return GenerateMaze(MazeGenStepSize);
        yield return ConnectCells(MazeGenStepSize);

        nav.BuildNavMesh();

        SpawnMinotaur();

        SpawnPortals();
    }

    //The current scaling and positioning calculation use constants
    //that were only tested with a maze scale of 5, so be careful
    void BuildFloor()
    {
        //Cell size constant modifier
        float sizeScale = 8f;
        //Cell position constant modifier
        float posScale = 2f;

        //Create the floor
        var floor = Instantiate(MazeFloor, this.gameObject.transform);

        //Scale and position the floor
        floor.transform.localScale = new Vector3(
            MazeLength * sizeScale,
            0.33f,
            MazeWidth * sizeScale
        );

        floor.transform.localPosition = new Vector3(
            (floor.transform.localScale.x / posScale) - MazeScale,
            1f,
            (floor.transform.localScale.z / posScale) - MazeScale
        );

        //First-Pass Nav Mesh
        nav = floor.GetComponent<NavMeshSurface>();
        nav.BuildNavMesh();
    }

    IEnumerator GenerateMaze(int step)
    {
        //For each (i, j) coordinate...
        int stepCount = 0;
        for(int i = 0; i < MazeWidth; ++i)
        {
            for(int j = 0; j < MazeLength; ++j)
            {
                //Make a new cell and add it to the maze array
                var newcell = MakeCell(i, j);
                Maze[i, j] = newcell;
                
                //every stepCount cells, pause to let the engine catch up
                ++stepCount;
                if(stepCount > step)
                {
                    //pause to let the engine render a frame
                    yield return null;
                    stepCount = 0;
                }
            }
        }
    }

    IEnumerator ConnectCells(int step)
    {
        //this list represents all of the cells adjacent to cells in the maze.
        List<MazeCell> frontierCells = new List<MazeCell>();
        List<MazeCell> inCells = new List<MazeCell>();

        //Step 0: Add a random to cell to the maze
        //This could be any cell, but for now I'm just gonna make it be (0,0)
        inCells.Add(Maze[0, 0]);
        MakeAllNeighborsFrontier(Maze[0,0], inCells, frontierCells);

        //Step 1: While there are frontier cells...
        int stepCount = 0;
        while(frontierCells.Count != 0)
        {
            //Step 1.1: Pick a random cell
            MazeCell toConnect = frontierCells[Rnd.Next(frontierCells.Count)];

            //Step 1.2: Connect it to an "In" Neighbor
            ConnectToRandomNeighbor(toConnect, inCells);

            //Step 1.3: Move this cell from "frontier" to "in"
            frontierCells.Remove(toConnect);
            inCells.Add(toConnect);

            //Step 1.4: Make all of this cell's neighbors frontier cells
            MakeAllNeighborsFrontier(toConnect, inCells, frontierCells);
            ++stepCount;

            if(stepCount > step)
            {
                //give the engine a chance to render the frame every now and again
                stepCount = 0;
                yield return null;
            }
        }

        //And the maze should be done now
    }

    void ConnectToRandomNeighbor(MazeCell center, List<MazeCell> inCells)
    {
        //Create an array of cells
        List<MazeCell> validCells = new List<MazeCell>();
        var c = center.Coordinate;

        Action<int, int> validate = new Action<int, int>( (x, y) => 
        {
            //Check if the cell at (x, y) exists and is in the maze
            //If so, mark it as a valid cell
            if(IsValidCell(x, y) && inCells.Contains(Maze[x, y])) validCells.Add(Maze[x, y]);
        });

        //add all valid cells adjacent to the center cell
        validate(c.x+1, c.y);
        validate(c.x-1, c.y);
        validate(c.x, c.y+1);
        validate(c.x, c.y-1);

        //pick a random valid cell and connect to it
        var otherCell = validCells[Rnd.Next(validCells.Count)];
        center.Connect(otherCell);    
    }

        void MakeAllNeighborsFrontier(MazeCell center, List<MazeCell> inCells, List<MazeCell> frontierCells)
    {
        Vector2Int c = center.Coordinate;

        //Determines if the maze cell should be added to the "frontier" of the maze, which are cells
        //that are adjacent to cells in the maze but are not themselves in the main
        Action<int, int> frontify = new Action<int, int>( (x, y) => 
        {
            /*//If we're checking cells on the x axis...
            if(dynamicX)
            {
                //and such a cell exists and it isn't already in the maze or the frontier
                if(IsValidCell(dyn, stat) && !inCells.Contains(Maze[dyn, stat]) 
                    && !frontierCells.Contains(Maze[dyn, stat]))
                {
                    //add it to the frontier
                    frontierCells.Add(Maze[dyn, stat]);
                }
            }
            //Otherwise check as if the cell is on the x axis
            else
            {
                if(IsValidCell(stat, dyn) && !inCells.Contains(Maze[stat, dyn]) 
                    && !frontierCells.Contains(Maze[stat, dyn]))
                {
                    frontierCells.Add(Maze[stat, dyn]);
                }
            }*/

            if(IsValidCell(x, y) && !inCells.Contains(Maze[x, y]) && !frontierCells.Contains(Maze[x, y]))
            {
                frontierCells.Add(Maze[x, y]);
            }
            
        });
    
        //Frontify all of the center's neighbors
        frontify(c.x+1, c.y);
        frontify(c.x-1, c.y);
        frontify(c.x, c.y+1);
        frontify(c.x, c.y-1);

        /*frontify(c.x+1, c.y, true);
        frontify(c.x-1, c.y, true);
        frontify(c.y+1, c.x, false);
        frontify(c.y-1, c.x, false);*/
    }

        MazeCell MakeCell(int i, int j)
    {
        //create cell
        var cell = Instantiate(MazeCell, this.gameObject.transform);
        var cellData = cell.GetComponent<MazeCell>();

        //Move it to its spot
        cell.transform.localPosition = new Vector3
        (
            (float)i * MazeScale * 1.5f,
            0f,
            (float)j * MazeScale * 1.5f
        );

        //scale it
        cell.transform.localScale = cell.transform.localScale * MazeScale;

        //initialize it
        cellData.Initialize(new Vector2Int(i, j));
        return cellData;
    }

    //POINT-OF-INTEREST METHODS--------------------------------------------------------------------
    void SpawnMinotaur()
    {
        Instantiate(Minotaur, minotaurSpawn, Quaternion.identity);
    }

    void SpawnMonolith()
    {
        //Pick a random cell
        MazeCell center = GetRandomCellWithPadding(replacerSize + 1);

        //Get the cells around it
        MazeCell[] radius = GetCellsInRadius(center, replacerSize - 1);

        //Create the replacer
        var replacer = Instantiate(
            ReplacerCell, 
            center.gameObject.transform.position,
            Quaternion.identity
        ).GetComponent<MazeCellReplacer>();

        //Initialize it with the chosen cells
        replacer.Initialize(center, radius);
    }
    
    void SpawnSpawnRoom()
    {
        //the size of the spawn room as a constant fuck off I'll make it more flexible later
        int spawnRoomSize = 2;

        //Pick a random cell
        MazeCell center = GetRandomCellWithPadding(spawnRoomSize + 1);

        //Get the cells around it
        MazeCell[] radius = GetCellsInRadius(center, spawnRoomSize - 1);

        //Initialize the spawn room
        GameObject.FindWithTag("Respawn").GetComponent<MazeCellReplacer>().Initialize(center, radius);
    }

    void SpawnPortals()
    {
        //Get two random cells
        var a = GetRandomCellWithPadding(2);
        var b = GetRandomCellWithPadding(2);

        //And make the portals idk it's not rocket science
        Instantiate(Nclidian).GetComponent<NclidianController>().PlacePortals(
            new MazeNeighbors(a, Maze),
            new MazeNeighbors(b, Maze)
        );
    }

    ///HELPER METHODS------------------------------------------------------------------------------
    //Simple bounds check on the array. Can C# do this automatically?
    bool IsValidCell(int i, int j)
    {
        return (i >= 0 && MazeWidth > i) && (j >= 0 && MazeLength > j);
    }

    //Returns a random cell in the maze
    MazeCell GetRandomCell()
    {
        return Maze[Rnd.Next(MazeWidth), Rnd.Next(MazeLength)];
    }

    //Returns a random cell in the maze that has at least pad many cells between it and the
    //outside of the maze
    MazeCell GetRandomCellWithPadding(int pad)
    {
        //1 is added to the max of the pad because System.Random.Next()'s upper bound is exclusive
        return Maze[Rnd.Next(pad, MazeWidth - pad + 1), Rnd.Next(pad, MazeLength - pad + 1)];
    }

    //Returns an array of all the cells in a radius around a specified cell
    MazeCell[] GetCellsInRadius(MazeCell center, int radius)
    {
        List<MazeCell> radiusCells = new List<MazeCell>();
        var c = center.Coordinate;

        //Step 1: get every cell on the same X or Y level as the center
        for(int i = 1; i <= radius; ++i)
        {
            if(IsValidCell(c.x+i, c.y)) radiusCells.Add(Maze[c.x+i, c.y]);
            if(IsValidCell(c.x-i, c.y)) radiusCells.Add(Maze[c.x-i, c.y]);
            if(IsValidCell(c.x, c.y+i)) radiusCells.Add(Maze[c.x, c.y+i]);
            if(IsValidCell(c.x, c.y-i)) radiusCells.Add(Maze[c.x, c.y-i]);
        }

        //Step 2: get every cell that isn't on the same X or Y level
        for(int i = 1; i <= radius; ++i)
        {
            for(int j = 1; j <= radius; ++j)
            {
                if(IsValidCell(c.x+i, c.y+j)) radiusCells.Add(Maze[c.x+i, c.y+j]);
                if(IsValidCell(c.x-i, c.y+j)) radiusCells.Add(Maze[c.x-i, c.y+j]);
                if(IsValidCell(c.x+i, c.y-j)) radiusCells.Add(Maze[c.x+i, c.y-j]);
                if(IsValidCell(c.x-i, c.y-j)) radiusCells.Add(Maze[c.x-i, c.y-j]);
            }
        }

        //Step 3: return the cells as an array
        //Keeping the radius as an array makes it easy to pass to a params argument
        return radiusCells.ToArray();
    }
}

