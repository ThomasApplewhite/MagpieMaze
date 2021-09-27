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

public class PlayerCharacter : MonoBehaviour
{
    [Tooltip("Where to place the player when they die")]
    public Transform killLocation;

    [Tooltip("The particle system to emiy footsteps from")]
    public ParticleSystem footstepSystem;

    [Tooltip("The pause menu to bring up when the game is paused")]
    public PauseMenu pauseMenu;

    [Tooltip("How long to delay footsteps for (bigger value = fewer footsteps")]
    public int footstepDelay = 10;

    //how far footsteps should be from the center of the player
    private float footstepGap = 0.5f;

    //the CharacterController component of the player
    private CharacterController playerController;

    //a delta value to add to every time the player moves
    private int footstepDelta = 0;

    //an alternator to determine a left or right footstep
    private int footstepSide = 1;
    

    // Start is called before the first frame update
    void Start()
    {
        playerController = this.gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(playerController.velocity != Vector3.zero)
        {
            ++footstepDelta;

            if(footstepDelta > footstepDelay)
            {
                var ep = new UnityEngine.ParticleSystem.EmitParams();
                ep.position = 
                    this.gameObject.transform.position -                //player pos
                    new Vector3(0f, playerController.height/2, 0f) +    //offset to foot level;
                    this.gameObject.transform.right * footstepGap * footstepSide;
                footstepSide *= -1;
                ep.rotation = this.gameObject.transform.rotation.eulerAngles.y;
                footstepSystem.Emit(ep, 1);
                footstepDelta = 0;
            }
        }
    }

    public void Kill()
    {
        Debug.Log("Player is dead!");
        this.gameObject.transform.position = killLocation.position;
    }

    public void Pause()
    {
        //GameObject.FindWithTag("PauseMenu").GetComponent<PauseMenu>().TogglePause();
        Debug.Log("PlayerCharacter.Pause: pausing game");
        pauseMenu.TogglePause();
    }
}
