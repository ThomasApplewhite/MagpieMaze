/*Copyright (c) 2021 Magpie Paulsen
Written by Thomas Applewhite

This program is free software; you can non-commercially distribute
this software without modification and with attribution under the Creative Commons
BY-NC-ND 4.0 License.

This program is distributed WITHOUT WARRANTY or FITNESS FOR A PARTICULAR PURPOSE.

You should have received a copy of the Creative Commons BY-NC-ND 4.0 License along
with this program. If not, see <https://creativecommons.org/licenses/by-nc-nd/4.0/>*/

using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    [Tooltip("The name of the maze scene to return to")]
    public string returnScene;

    public void RestartGame()
    {
        Debug.Log($"DeathMenu.RestartGame: Returning to {returnScene}...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(returnScene);
    }

    public void QuitGame()
    {
        Debug.Log("DeathMenu.QuitGame: Quitting Game...");
        Application.Quit();
    }
}
