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

public class NclidianController : MonoBehaviour
{
    public GameObject portalA;
    public GameObject portalB;

    //Replaces alphaCell and betaCell with portalA and portalB, respectively
    public void PlacePortals(MazeCell alphaCell, MazeCell betaCell)
    {
        System.Action<GameObject, MazeCell> portalReplace = (GameObject portal, MazeCell replacee) => 
        {
            portal.transform.parent = null;

            portal.GetComponent<MazeCellReplacer>().Initialize(replacee);

            portal.transform.Rotate(new Vector3(
                0f,
                UnityEngine.Random.Range(0, 4) * 90f,
                0f
            ));
        };

        portalReplace(portalA, alphaCell);
        portalReplace(portalB, betaCell);
    }

    void AttemptPortalReplace()
    {

    }

    /*
    Current brain-thinks on the portal problem
        Each portal cell has 1 portal. How can the portal be aligned such that this portal is accesible
        Wait hold on just remove all of the walls of a portal cell and connect with a random number
            of cells (including the north wall so the portal is always accessible both ways)
        The issue that this leaves is that portals will always be on the "north" wall of a cell
        Hopefully, players simply won't notice
    */
}
