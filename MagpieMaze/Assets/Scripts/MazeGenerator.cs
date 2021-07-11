using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    [Tooltip("How long the maze should be")]
    public int MazeLength = 10;
    
    [Tooltip("How wide the maze should be")]
    public int MazeWidth = 10;

    [Tooltip("How big each cell should be")]
    public float MazeScale = 3f;

    [Tooltip("How many cells should be generated at a time (bigger number = faster but needs better computer)")]
    public int MazeGenStepSize = 10;

    [Tooltip("The prefab of a Maze Cell")]
    public GameObject MazeCell;

    [Tooltip("The prefab of a Replacer to just replace some things")]
    public GameObject ReplacerCell;

    [Tooltip("The prefab of the floor")]
    public GameObject MazeFloor;

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

    IEnumerator ComissionMaze()
    {
        BuildFloor();

        yield return GenerateMaze(MazeGenStepSize);
        yield return ConnectCells(MazeGenStepSize);

        nav.BuildNavMesh();

        //StartCoroutine(AddRandomReplacers());
    }

    //The current scaling and positioning calculation use constants
    //that were only tested with a maze scale of 5, so be careful
    void BuildFloor()
    {
        float sizeScale = 8f;
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
        int stepCount = 0;
        for(int i = 0; i < MazeWidth; ++i)
        {
            for(int j = 0; j < MazeLength; ++j)
            {
                var newcell = MakeCell(i, j);
                Maze[i, j] = newcell;
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

    IEnumerator AddRandomReplacers()
    {
        while(true)
        {
            //pick a random cell
            int randX = Rnd.Next(MazeWidth);
            int randY = Rnd.Next(MazeLength);

            //replace it for a little while
            MazeCellReplacer replacer = Instantiate(ReplacerCell, 
                this.gameObject.transform).GetComponent<MazeCellReplacer>();
        
            Debug.Log("Replacing Maze Cell");
            replacer.Initialize(Maze[randX, randY]);

            yield return new WaitForSeconds(3f);

            replacer.Denitialize();
        }
        
    }

    void ConnectToRandomNeighbor(MazeCell center, List<MazeCell> inCells)
    {
        List<MazeCell> validCells = new List<MazeCell>();
        var c = center.Coordinate;

        Action<int, int> validate = new Action<int, int>( (x, y) => 
        {
            if(IsValidCell(x, y) && inCells.Contains(Maze[x, y])) validCells.Add(Maze[x, y]);
        });

        //add all valid cells
        validate(c.x+1, c.y);
        validate(c.x-1, c.y);
        validate(c.x, c.y+1);
        validate(c.x, c.y-1);

        //connect to a random valid cell

        var otherCell = validCells[Rnd.Next(validCells.Count)];
        center.Connect(otherCell);    
    }

    void MakeAllNeighborsFrontier(MazeCell center, List<MazeCell> inCells, List<MazeCell> frontierCells)
    {
        Vector2Int c = center.Coordinate;

        Action<int, int, bool> frontify = new Action<int, int, bool>( (dyn, stat, dynamicX) => 
        {
            if(dynamicX)
            {
                if(IsValidCell(dyn, stat) && !inCells.Contains(Maze[dyn, stat]) 
                    && !frontierCells.Contains(Maze[dyn, stat]))
                {
                    frontierCells.Add(Maze[dyn, stat]);
                }
            }
            else
            {
                if(IsValidCell(stat, dyn) && !inCells.Contains(Maze[stat, dyn]) 
                    && !frontierCells.Contains(Maze[stat, dyn]))
                {
                    frontierCells.Add(Maze[stat, dyn]);
                }
            }
            
        });
    
        frontify(c.x+1, c.y, true);
        frontify(c.x-1, c.y, true);
        frontify(c.y+1, c.x, false);
        frontify(c.y-1, c.x, false);
    }

    bool IsValidCell(int i, int j)
    {
        return (i >= 0 && MazeWidth > i) && (j >= 0 && MazeLength > j);
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
}
