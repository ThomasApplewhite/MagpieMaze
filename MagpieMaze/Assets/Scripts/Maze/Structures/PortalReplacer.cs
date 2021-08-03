using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalReplacer : MazeCellReplacer
{
    [Tooltip("The transform to rotate to align the portal")]
    public Transform portalRotator;

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
