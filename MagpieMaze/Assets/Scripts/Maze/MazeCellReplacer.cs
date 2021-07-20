//Copyright (c) 2021 Magpie Paulsen
//Written by Thomas Applewhite

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellReplacer : MazeCell
{
    //A MazeCell Replacer replaces a MazeCell with something else, like some non-euclidian nonsense

    //The replaced cell in the middle. Necessarily requires replacers to have a "center" cell too
    private MazeCell centerReplacee;

    //Any other cells being replaced
    private MazeCell[] replacees;

    //Disables replacees so that the replacer can take their place
    public void Initialize(MazeCell centerReplacee, params MazeCell[] replacees)
    {
        //copy the replacee's scale and position, then disable the replacee for now
        this.centerReplacee = centerReplacee;
        this.replacees = replacees;

        this.gameObject.transform.localPosition = this.centerReplacee.gameObject.transform.localPosition;
        this.gameObject.transform.localScale = this.centerReplacee.gameObject.transform.localScale;

        //Disable all replacees here
        this.centerReplacee.gameObject.SetActive(false);
        foreach(MazeCell replacee in this.replacees) replacee.gameObject.SetActive(false);
    }

    public void Denitialize()
    {
        //return the replacees and self-destruct
        this.centerReplacee.gameObject.SetActive(true);
        foreach(MazeCell replacee in this.replacees) replacee.gameObject.SetActive(true);

        Destroy(this.gameObject);
    }
}
