using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Using alias to avoid nameclash with System.Random.Range (which is called Rnd elsewhere)
using Urnd = UnityEngine.Random;

public class Maze : MonoBehaviour
{
    [Tooltip("Whether or not this maze should generate automatically on scene start")]
    public bool generateOnStart = false;

    [Tooltip("The settings for generating this particular maze")]
    public MazeGenerator mazeSettings;

    public enum MazeStatus
    {
        UNGENERATED,
        GENERATING,
        GENERATED
    }

    //The status of the maze, and whether or not it has been generated
    public MazeStatus initialized { get; private set; } = MazeStatus.UNGENERATED;

    //The actual array of MazeCells
    public MazeCell[,] mazeArray { get; set; }

    //Allows for cells to be got, but not set, by directly indexing the maze
    //This isn't index safe, however, so be careful!
    public MazeCell this[int x, int y]
    {
        get
        {
            return mazeArray[x, y];
        }
    }

    //EZ-PZ wrapper for mazeArray's property of the same name
    public int width { get; private set; }

    //EZ-PZ wrapper for mazeArray's property of the same name
    public int length { get; private set; }

    /*
    ---Notes about Mazes---
    The origin cell is (0, 0), which is at 0, 0, 0 relative to the Maze parent object
    X Coordinates increment Eastward
    Y Coordinates increment Northward
    Yes I know it's esentially just a wrapper with some utility methods tacked on and with
        length swaped to the other size, but that's what I want right now.
    */

    void Start()
    {
        if(generateOnStart) Initialize();
    }

    //Initialize the maze, with or without custom generator settings
    public void Initialize()
    {
        Initialize(mazeSettings);
    }

    public void Initialize(MazeGenerator settings)
    {
        if(initialized != MazeStatus.UNGENERATED) return;

        StartCoroutine(GenTime(settings));
    }

    IEnumerator GenTime(MazeGenerator settings)
    {
        initialized = MazeStatus.GENERATING;

        //I'd set these in the generator, but IEnumerators
        //don't allow out parameters.
        this.width = settings.mazeWidth;
        this.length = settings.mazeLength;

        yield return settings.Generate(this);

        initialized = MazeStatus.GENERATED;
    }

    //Simple bounds check on the array. Can C# do this automatically?
    public bool IsValidCell(int i, int j)
    {
        return (i >= 0 && width > i) && (j >= 0 && length > j);
    }

    //The same as indexing the maze, but returns null on invalid indicies
    public MazeCell GetCell(int x, int y)
    {
        return IsValidCell(x, y) ? mazeArray[x, y] : null;
    }

    //Returns a random cell in the maze
    public MazeCell GetRandomCell()
    {
        return mazeArray[Urnd.Range(0, width), Urnd.Range(0, length)];
    }

    //Returns a random cell in the maze that has at least pad many cells between it and the
    //outside of the maze
    public MazeCell GetRandomCellWithPadding(int pad)
    {
        //1 is added to the max of the pad because System.Random.Next()'s upper bound is exclusive
        return mazeArray[Urnd.Range(pad, width - pad), Urnd.Range(pad, length - pad)];
    }

    //Returns an array of all the cells in a radius around a specified cell
    public MazeCell[] GetCellsInRadius(MazeCell center, int radius)
    {
        List<MazeCell> radiusCells = new List<MazeCell>();
        var c = center.Coordinate;

        //Step 1: get every cell on the same X or Y level as the center
        for(int i = 1; i <= radius; ++i)
        {
            if(IsValidCell(c.x+i, c.y)) radiusCells.Add(mazeArray[c.x+i, c.y]);
            if(IsValidCell(c.x-i, c.y)) radiusCells.Add(mazeArray[c.x-i, c.y]);
            if(IsValidCell(c.x, c.y+i)) radiusCells.Add(mazeArray[c.x, c.y+i]);
            if(IsValidCell(c.x, c.y-i)) radiusCells.Add(mazeArray[c.x, c.y-i]);
        }

        //Step 2: get every cell that isn't on the same X or Y level
        for(int i = 1; i <= radius; ++i)
        {
            for(int j = 1; j <= radius; ++j)
            {
                if(IsValidCell(c.x+i, c.y+j)) radiusCells.Add(mazeArray[c.x+i, c.y+j]);
                if(IsValidCell(c.x-i, c.y+j)) radiusCells.Add(mazeArray[c.x-i, c.y+j]);
                if(IsValidCell(c.x+i, c.y-j)) radiusCells.Add(mazeArray[c.x+i, c.y-j]);
                if(IsValidCell(c.x-i, c.y-j)) radiusCells.Add(mazeArray[c.x-i, c.y-j]);
            }
        }

        //Step 3: return the cells as an array
        //Keeping the radius as an array makes it easy to pass to a params argument
        return radiusCells.ToArray();
    }
}
