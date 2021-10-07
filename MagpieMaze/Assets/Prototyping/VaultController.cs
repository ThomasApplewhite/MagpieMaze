/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultController : MonoBehaviour
{
    [Tooltip("The portals that will be placed into the maze (make sure to only to use" + 
        " either the A or the B portal, not both)")]
    public PortalReplacer[] nclidians;

    [Tooltip("The parent gameobject of both the destination portals and the vault itself")]
    public Transform vaultStructure;

    [Tooltip("The prefab of the vault's occupant")]
    public GameObject occupant;

    [Tooltip("Where to spawn the occupant in the vault")]
    public Transform occupantLocation;

    //the final scale of the vault based on portal requirements
    Vector3 endScale;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IntegrateVault(Maze maze)
    {
        //Step 1: Integrate the portals into the maze
        foreach(PortalReplacer portal in nclidians)
        {
            var destCell = maze.GetRandomCellWithPadding(2);
            PortalReplacer.OpenPortalReplacement(portal.gameObject, new MazeNeighbors(destCell, maze));
            endScale = portal.gameObject.transform.localScale;
        }

        //Step 2: Update the scale of the vault itself
        vaultStructure.localScale = endScale;

        //Step 3: Spawn the vaults occupant
        Instantiate(occupant, occupantLocation);
    }
}
