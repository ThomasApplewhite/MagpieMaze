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
using UnityEngine.Events;

//alias to avoid name clash with object (the inherent C# type)
using Uobj = UnityEngine.Object;

[System.Serializable]
public class MazeGenerator
{
    [Header("Generation Parameters")]
    [Tooltip("How long the maze should be")]
    public int mazeLength = 10;
    
    [Tooltip("How wide the maze should be")]
    public int mazeWidth = 10;

    [Tooltip("How big each cell should be")]
    public float mazeScale = 5f;

    [Tooltip("How many cells should be generated at a time (bigger number = faster but needs better computer)")]
    public int mazeGenStepSize = 10;

    [Tooltip("Whether or not this maze should have a Minotaur")]
    public bool generateMinotaur = true;

    [Tooltip("Whether or not this maze should have a Cassandra")]
    public bool generateCassandra = false;

    [Header("Spawn Parameters")]
    [Tooltip("The location of the Minotaur's spawn point")]
    public Vector2Int minotaurSpawn = new Vector2Int(6, 6);

    [Tooltip("The location of Cassandra's spawn point")]
    public Vector2Int cassandraSpawn = new Vector2Int(3, 3);

    [Header("Prefab Parameters")]
    [Tooltip("The prefab of a Maze Cell")]
    public GameObject mazeCell;

    [Tooltip("The prefab of the floor")]
    public GameObject mazeFloor;

    [Tooltip("The prefab of the Minotaur")]
    public GameObject minotaur;

    [Tooltip("The prefab of Cassandra")]
    public GameObject cassandra;

    [Tooltip("The Prefab of an Nclidian Duo")]
    public GameObject nclidian;

    [Tooltip("The number of portal pairs that should be created in the maze")]
    public int portalPairCount;

    [Header("Generation Timing Events")]
    public MazeGenEvent onGenerationBegin;
    public UnityEvent onGenerationEnd;

    //The Array of MazeCells that make up the maze
    public MazeCell[,] maze { get; private set; }

    //The maze being generated
    private Maze activeMaze;

    //The NavMesh of the floor
    private NavMeshSurface nav;

    //The random number generator, for random number things
    private System.Random rnd;
    

    // Start is called before the first frame update
    public IEnumerator Generate(Maze mazeToGenerate)
    {
        activeMaze = mazeToGenerate;
        maze = new MazeCell[mazeWidth, mazeLength];
        rnd = new System.Random();

        //Pass generation parameters to event listeners
        //Currently, this is just the MazeScale parameter
        onGenerationBegin.Invoke(mazeScale);

        yield return ComissionMaze();

        //Save the maze. This is needed for generation steps that need the maze to be done
        mazeToGenerate.mazeArray = maze;

        yield return ComissionAddOns();

        //Notifies other parts of the code that generation is done
        onGenerationEnd.Invoke();
    }

    //Constructs the maze's floor and its walls, and carves out the actual structure
    IEnumerator ComissionMaze()
    {   
        BuildFloor();

        yield return GenerateMaze(mazeGenStepSize);
        yield return ConnectCells(mazeGenStepSize);

        nav.BuildNavMesh();
    }

    //Constructs anything that needs the maze to exist first
    //Currently doesn't need any yielding, but I'm leaving it
    //an IEnumerator for now
    IEnumerator ComissionAddOns()
    {
        if(generateMinotaur) SpawnMinotaur();
        if(generateCassandra) SpawnCassandra();

        for(int i = 0; i < portalPairCount; ++i) SpawnPortals();

        yield return null;
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
        var floor = Uobj.Instantiate(mazeFloor, activeMaze.gameObject.transform);

        //Scale and position the floor
        floor.transform.localScale = new Vector3(
            mazeWidth * sizeScale,
            0.33f,
            mazeLength * sizeScale
        );

        floor.transform.localPosition = new Vector3(
            (floor.transform.localScale.x / posScale) - mazeScale,
            1f,
            (floor.transform.localScale.z / posScale) - mazeScale
        );

        //First-Pass Nav Mesh
        nav = floor.GetComponent<NavMeshSurface>();
        nav.BuildNavMesh();
    }

    IEnumerator GenerateMaze(int step)
    {
        //For each (i, j) coordinate...
        int stepCount = 0;
        for(int i = 0; i < mazeWidth; ++i)
        {
            for(int j = 0; j < mazeLength; ++j)
            {
                //Make a new cell and add it to the maze array
                var newcell = MakeCell(i, j);
                maze[i, j] = newcell;
                
                //every stepCount cells, pause to let the engine catch up
                ++stepCount;
                if(stepCount > step)
                {
                    //pause to let the engine render a frame
                    //This is why the entire generation is a coroutine
                    yield return null;
                    stepCount = 0;
                }
            }
        }
    }

    MazeCell MakeCell(int i, int j)
    {
        //create cell
        var cell = Uobj.Instantiate(mazeCell, activeMaze.gameObject.transform);
        var cellData = cell.GetComponent<MazeCell>();

        //Move it to its spot
        cell.transform.localPosition = new Vector3
        (
            (float)i * mazeScale * 1.5f,
            0f,
            (float)j * mazeScale * 1.5f
        );

        //scale it
        cell.transform.localScale = cell.transform.localScale * mazeScale;

        //initialize it
        cellData.Initialize(new Vector2Int(i, j));
        return cellData;
    }

    IEnumerator ConnectCells(int step)
    {
        //this list represents all of the cells adjacent to cells in the maze.
        List<MazeCell> frontierCells = new List<MazeCell>();
        List<MazeCell> inCells = new List<MazeCell>();

        //Step 0: Add a random to cell to the maze
        //This could be any cell, but for now I'm just gonna make it be (0,0)
        inCells.Add(maze[0, 0]);
        MakeAllNeighborsFrontier(maze[0,0], inCells, frontierCells);

        //Step 1: While there are frontier cells...
        int stepCount = 0;
        while(frontierCells.Count != 0)
        {
            //Step 1.1: Pick a random cell
            MazeCell toConnect = frontierCells[rnd.Next(frontierCells.Count)];

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
            if(IsValidCell(x, y) && inCells.Contains(maze[x, y])) validCells.Add(maze[x, y]);
        });

        //add all valid cells adjacent to the center cell
        validate(c.x+1, c.y);
        validate(c.x-1, c.y);
        validate(c.x, c.y+1);
        validate(c.x, c.y-1);

        //pick a random valid cell and connect to it
        var otherCell = validCells[rnd.Next(validCells.Count)];
        center.Connect(otherCell);    
    }

    void MakeAllNeighborsFrontier(MazeCell center, List<MazeCell> inCells, List<MazeCell> frontierCells)
    {
        Vector2Int c = center.Coordinate;

        //Determines if the maze cell should be added to the "frontier" of the maze, which are cells
        //that are adjacent to cells in the maze but are not themselves in the main
        Action<int, int> frontify = new Action<int, int>( (x, y) => 
        {
            if(IsValidCell(x, y) && !inCells.Contains(maze[x, y]) && !frontierCells.Contains(maze[x, y]))
            {
                frontierCells.Add(maze[x, y]);
            }
            
        });
    
        //Frontify all of the center's neighbors
        frontify(c.x+1, c.y);
        frontify(c.x-1, c.y);
        frontify(c.x, c.y+1);
        frontify(c.x, c.y-1);
    }

    //Simple bounds check on the array. Can C# do this automatically?
    //Stolen helper from the regular Maze class 'cause it's too helpful
    public bool IsValidCell(int i, int j)
    {
        return (i >= 0 && mazeWidth > i) && (j >= 0 && mazeLength > j);
    }

    //POINT-OF-INTEREST METHODS--------------------------------------------------------------------
    void SpawnMinotaur()
    {
        var spawnPos = activeMaze[minotaurSpawn.x, minotaurSpawn.y].anchorCoord;
        Uobj.Instantiate(minotaur, spawnPos, Quaternion.identity);
    }

    void SpawnCassandra()
    {
        var spawnPos = activeMaze[cassandraSpawn.x, cassandraSpawn.y].anchorCoord;
        Uobj.Instantiate(cassandra, spawnPos, Quaternion.identity).SendMessage("BeginWander", activeMaze);
    }

    void SpawnPortals()
    {
        //Get two random cells
        //We need to do it directly because the width and length of activeMaze
        //isn't set yet
        //Pad is how far a portal should be from a wall
        int pad = 2;
        var randcell = new System.Func<MazeCell>
        ( 
            () => maze[rnd.Next(pad, mazeWidth - pad), rnd.Next(pad, mazeLength - pad)] 
        );
        var a = randcell();
        var b = randcell();

        //And make the portals idk it's not rocket science
        Uobj.Instantiate(nclidian).GetComponent<NclidianController>().PlacePortals(
            new MazeNeighbors(a, maze),
            new MazeNeighbors(b, maze)
        );
    }

    //These two methods are were left over from room replacer testing. They might get reused.
    /*void SpawnMonolith()
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
    }*/
}

//This class is just a UnityEvent that can pass a float (or other data)
//It passes information about maze generation parameters to its listeners
[Serializable]
public class MazeGenEvent : UnityEvent <float> {};

