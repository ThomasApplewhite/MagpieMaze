using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //The Array of MazeCells that represent the maze
    private int[,] Maze;

    // Start is called before the first frame update
    void Start()
    {
        Maze = new int[MazeWidth, MazeLength];
        StartCoroutine(GenerateMaze(MazeGenStepSize));
    }

    IEnumerator GenerateMaze(int step)
    {
        int stepCount = 0;
        for(int i = 0; i < MazeWidth; ++i)
        {
            for(int j = 0; j < MazeLength; ++j)
            {
                MakeCell(i, j);
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

    void MakeCell(int i, int j)
    {
        //create cell
        var cell = Instantiate(MazeCell, this.gameObject.transform);

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
        cell.SendMessage("Initialize", new Vector2Int(i, j));
    }
}
