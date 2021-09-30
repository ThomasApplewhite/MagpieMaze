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

    /*
        Okay this is gonna get a bit fucky but hear me out
        This array works by having each ReplacementState's method in the same
        index as the actual enum value. This way, the array can be indexed by enum!
        No comparisons, I just plug in the correct enum and let the methods go!

        I know it's a bit silly, but the underlying idea of directly tying
        an enum value to a delegate isn't crazy. C# just doesn;t
        let me do it directly

        I think.
    */
    private System.Action<GameObject, MazeNeighbors>[] placementMethods;

    void Awake()
    {
        placementMethods = new System.Action<GameObject, MazeNeighbors>[] 
        {
            (portal, region) => PortalReplacer.DirectPortalReplacement(portal, region),
            (portal, region) => PortalReplacer.RandomPortalReplacement(portal, region),
            (portal, region) => PortalReplacer.OpenPortalReplacement(portal, region),
        };
    }

    //Replaces alphaCell and betaCell with portalA and portalB, respectively
    public void PlacePortals(MazeNeighbors alphaRegion, MazeNeighbors betaRegion,
        PortalReplacer.ReplacementState alphaState=PortalReplacer.ReplacementState.RANDOM,
        PortalReplacer.ReplacementState betaState=PortalReplacer.ReplacementState.RANDOM)
    {
        /*if(alphaState == ReplacementState.DIRECT) DirectPortalReplacement(PortalA, alphaRegion);
        else RandomPortalReplacement(PortalA, alphaRegion);

        if(betaState == ReplacementState.DIRECT) DirectPortalReplacement(PortalB, betaRegion);
        else RandomPortalReplacement(PortalB, betaRegion);*/

        placementMethods[(int)alphaState].Invoke(PortalA, alphaRegion);
        placementMethods[(int)betaState].Invoke(PortalB, betaRegion);
            
        //Update the list of portals the player knows about (make sure not to do this too often!)
        GameObject.FindWithTag("Player").BroadcastMessage("UpdatePortalArray");

        Destroy(this.gameObject);
    }
}
