/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modifcation and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You shoould have recieved a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NclidianController : MonoBehaviour
{
    public GameObject PortalA;
    public GameObject PortalB;

    //Replaces alphaCell and betaCell with portalA and portalB, respectively
    public void PlacePortals(MazeNeighbors alphaRegion, MazeNeighbors betaRegion)
    {
        //The original replacer method, which does not connect to the maze.
        //Currently unused.
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

        //This replacer method does neighbor connections and the initial replacement
        System.Action<GameObject, MazeNeighbors> connectedPortalReplace = 
            (GameObject portal, MazeNeighbors region) =>
        {
            //disconnect the host region from all of its neighbors
            region.Owner.Disconnect(region.North);
            region.Owner.Disconnect(region.South);
            region.Owner.Disconnect(region.East);
            region.Owner.Disconnect(region.West);

            //deparent the portal and do the replacement
            var portalCell = portal.GetComponent<MazeCellReplacer>();
            portal.transform.parent = null;
            portalCell.Initialize(region.Owner);

            //Then the replaced cell's north neighbor (and potentially others)
            //to the cell
            portalCell.Connect(region.North);

            //Now connect a random wall that isn't the northern wall
            //This UnityEngine statement generates a random int: 0, 1, or 2
            switch(UnityEngine.Random.Range(0, 3)) {
                case 0:
                    portalCell.Connect(region.West);
                    break;

                case 1:
                    portalCell.Connect(region.East);
                    break;

                default: //fires on anything other than 0 or 1
                    portalCell.Connect(region.South);
                    break;
            }
        };

        connectedPortalReplace(PortalA, alphaRegion);
        connectedPortalReplace(PortalB, betaRegion);

        //Update the list of portals the player knows about (make sure not to do this too often!)
        GameObject.FindWithTag("Player").BroadcastMessage("UpdatePortalArray");
    }

    /*
    Current brain-thinks on the portal problem
        Each portal cell has 1 portal. How can the portal be aligned such that this portal is accesible
        Wait hold on just remove all of the walls of a portal cell and connect with a random number
            of cells (including the north wall so the portal is always accessible both ways)
        The issue that this leaves is that portals will always be on the "north" wall of a cell
        Hopefully, players simply won't notice
        And yeah I can just pad to make sure portals dont go directly to the boundires of a maze
    */
}
