
/*Copyright (c) 2020 Sebastian Lague
Modifications Copyright (c) 2021 Magpie Paulsen
Modifications written by Thomas Applewhite

This portion of the software is distributed under the MIT License. Please
see Sebastian Lague's MIT License (provided with this program) for more detail.

Also maybe move this to the player character script?
*/

using UnityEngine;

public class MainCameraUpdatable : MonoBehaviour {

    Portal[] portals;

    void Awake () {
        UpdatePortalArray();
    }

    public void UpdatePortalArray()
    {
        portals = FindObjectsOfType<Portal> ();
    }

    void OnPreCull () {

        for (int i = 0; i < portals.Length; i++) {
            portals[i].PrePortalRender ();
        }
        for (int i = 0; i < portals.Length; i++) {
            portals[i].Render ();
        }

        for (int i = 0; i < portals.Length; i++) {
            portals[i].PostPortalRender ();
        }

    }

}