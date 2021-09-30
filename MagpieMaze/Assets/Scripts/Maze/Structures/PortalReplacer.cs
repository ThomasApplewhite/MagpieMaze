using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalReplacer : MazeCellReplacer
{
    public enum ReplacementState
    {
        DIRECT, //Copy Replacee's neighbors
        RANDOM, //Have Random neighbors
        OPEN    //Become neighbors on every side except the portal side
    };


    [Tooltip("The transform to rotate to align the portal")]
    public Transform portalRotator;

    //Places the portal and copies the replacee's connections
    public static void DirectPortalReplacement(GameObject portal, MazeNeighbors region)
    {
        //deparent the portal and do the replacement
        var portalCell = PortalReplacer.TransformReplace(portal, region);

        //Then assume all of the replaced cell's connections
        portalCell.CopyConnections(region.Owner);

        //Then align the portal with at least one of them
        portalCell.AlignPortalWithAnyConnection();
    }

    //Places the portal and makes random connections
    public static void RandomPortalReplacement(GameObject portal, MazeNeighbors region)
    {
        //First, disconnect all of the original cell's neighbors
        region.Owner.DisconnectAll();

        //deparent the portal and do the replacement
        var portalCell = PortalReplacer.TransformReplace(portal, region);

        //Then the replaced cell's north neighbor (and potentially others)
        //to the cell
        portalCell.Connect(region.North);

        //Now connect a random wall that isn't the northern wall
        //This UnityEngine statement generates a random int: 0, 1, or 2
        switch (UnityEngine.Random.Range(0, 3))
        {
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
    }

    //Places the portal and connects everything but the direction the portal is facing
    public static void OpenPortalReplacement(GameObject portal, MazeNeighbors region)
    {
        //deparent the portal and do the replacement
        var portalCell = PortalReplacer.TransformReplace(portal, region);

        //Then connect to every side
        portalCell.Connect(region.North);
        portalCell.Connect(region.South);
        portalCell.Connect(region.East);
        portalCell.Connect(region.West);
    }

    //Does universal setup with the transform of the portals
    static PortalReplacer TransformReplace(GameObject portal, MazeNeighbors region)
    {
        var portalCell = portal.GetComponent<PortalReplacer>();
        portal.transform.parent = region.Owner.gameObject.transform.parent;
        portalCell.Initialize(region.Owner);

        return portalCell;
    }

    //Rotates the portal in 90 degree segments to place it in a certain wall
    public void AlignPortalWithWall(WallDirection direction)
    {
        Vector3 alignedRotation;

        switch(direction)
        {
            case WallDirection.EAST:
                alignedRotation = new Vector3(0f, 90f, 0);
                break;

            case WallDirection.SOUTH:
                alignedRotation = new Vector3(0f, 90f, 0) * 2f;
                break;

            case WallDirection.WEST:
                alignedRotation = new Vector3(0f, 90f, 0) * 3f;
                break;
            
            default:
                alignedRotation = Vector3.zero;
                break;
        }

        portalRotator.Rotate(alignedRotation);
    }

    //Note to self, add specific maze-cell side connection method

    //Pick a random connection and align with that wall
    public void AlignPortalWithAnyConnection()
    {
        //First, create a list of all possible sides
        //I still can't get over the fact that C# doesnt have implicit arrays
        WallDirection[] s = { WallDirection.NORTH, WallDirection.SOUTH, WallDirection.EAST, WallDirection.WEST };
        List<WallDirection> sides = new List<WallDirection>(s);

        //While there are still potential sides...
        while(sides.Count != 0)
        {
            //Pick one at random
            var pickedSide = sides[UnityEngine.Random.Range(0, sides.Count - 1)];

            //If that side's a connection, align with it
            if(this.IsConnected(pickedSide))
            {
                AlignPortalWithWall(pickedSide);
                return;
            }
            //If it isn't, remove it from the potentials
            else
            {
                sides.Remove(pickedSide);
            }
        }

        Debug.LogWarning("PortalReplacer.AlignPortalWithAnyConnection: No connections to align with");
    }
}
