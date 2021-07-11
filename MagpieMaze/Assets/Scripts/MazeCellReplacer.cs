using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellReplacer : MazeCell
{
    //A MazeCell Replacer replaces a MazeCell with something else, like some non-euclidian nonsense

    private MazeCell replacee;

    public void Initialize(MazeCell replacee)
    {
        //copy the replacee's scale and position, then disable the replacee for now
        this.replacee = replacee;

        this.gameObject.transform.localPosition = this.replacee.gameObject.transform.localPosition;
        this.gameObject.transform.localScale = this.replacee.gameObject.transform.localScale * 1.5f;

        this.replacee.gameObject.SetActive(false);
    }

    public void Denitialize()
    {
        //return the replacee and self-destruct
        this.replacee.gameObject.SetActive(true);

        Destroy(this.gameObject);
    }
}
