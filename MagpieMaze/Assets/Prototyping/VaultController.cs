using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaultController : MonoBehaviour
{
    [Tooltip("The portals that will be placed into the maze (make sure to only to use" + 
        " either the A or the B portal, not both)")]
    public PortalReplacer[] nclidians;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void IntegrateValue(Maze maze)
    {
        foreach(PortalReplacer portal in nclidians)
        {
            
        }
    }
}
