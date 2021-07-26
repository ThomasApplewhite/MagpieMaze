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
    public virtual void Initialize(MazeCell centerReplacee, params MazeCell[] replacees)
    {
        //copy the replacee's scale and position, then disable the replacee for now
        this.centerReplacee = centerReplacee;
        this.replacees = replacees;
        this.Coordinate = this.centerReplacee.Coordinate;

        this.gameObject.transform.localPosition = this.centerReplacee.gameObject.transform.localPosition;
        this.gameObject.transform.localScale = this.centerReplacee.gameObject.transform.localScale;

        //Disable all replacees here
        this.centerReplacee.gameObject.SetActive(false);
        foreach(MazeCell replacee in this.replacees) replacee.gameObject.SetActive(false);
    }

    public virtual void Denitialize()
    {
        //return the replacees and self-destruct
        this.centerReplacee.gameObject.SetActive(true);
        foreach(MazeCell replacee in this.replacees) replacee.gameObject.SetActive(true);

        Destroy(this.gameObject);
    }
}
