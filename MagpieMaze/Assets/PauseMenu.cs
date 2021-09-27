/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

public class PauseMenu : UnityEngine.MonoBehaviour
{
    //On startup, turn this canvas off, if it isn't already off
    public void Start()
    {
        this.gameObject.SetActive(false);
    }

    //Toggle the on/off of the menu, as well as toggle-freeze everything
    public void TogglePause()
    {
        //then freeze (or unfreeze) everything depending on what the new status of the menu is
        foreach (FreezableNPC npc in FindObjectsOfType<FreezableNPC>())
        {
            if(!this.gameObject.activeInHierarchy) npc.Freeze();
            else npc.Unfreeze();
        }

        //toggle the on/off of this object
        this.gameObject.SetActive(!this.gameObject.activeInHierarchy);
    }

    public void QuitGame()
    {
        UnityEngine.Debug.Log("PauseMenu.QuitGame: Quitting Game...");
        UnityEngine.Application.Quit();
    }
}
