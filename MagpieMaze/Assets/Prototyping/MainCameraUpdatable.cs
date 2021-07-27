
/*Copyright (c) 2020 Sebastian Lague
Modifications Copyright (c) 2021 Magpie Paulsen

Full License pending. Do not modify!

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