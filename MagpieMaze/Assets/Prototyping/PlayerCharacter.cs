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
using UnityEngine.SceneManagement;

public class PlayerCharacter : MonoBehaviour
{
    [Tooltip("The particle system to emit footsteps from")]
    public ParticleSystem footstepSystem;

    [Tooltip("The pause menu to bring up when the game is paused")]
    public PauseMenu pauseMenu;

    [Tooltip("How long to delay footsteps for (bigger value = fewer footsteps")]
    public int footstepDelay = 10;

    [Header("Death Settings")]
    [Tooltip("How many seconds it should take for the camera to rotate towards the killer")]
    public float initiateTime = 0.5f;

    [Tooltip("How long the player and the killer should maintain eye contact before the player dies")]
    public float staredownTime = 0.25f;

    [Tooltip("The scene to send the player to on death")]
    public string deathSceneName;

    //[Tooltip("Where to place the player when they die")]
    //public Transform killLocation;

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

    //Game Over's the game
    public void Kill(GameObject killer)
    {
        Debug.Log("Player is dead!");

        //Step 1: Freeze NPCs
        foreach (FreezableNPC npc in FindObjectsOfType<FreezableNPC>()) npc.Freeze();

        //Step 2: Time-based killing
        StartCoroutine(TimeKill(killer));
    }

    public void Pause()
    {
        //GameObject.FindWithTag("PauseMenu").GetComponent<PauseMenu>().TogglePause();
        Debug.Log("PlayerCharacter.Pause: pausing game");
        pauseMenu.TogglePause();
    }

    //Handles the time-sensitive parts of killing the player
    IEnumerator TimeKill(GameObject killer)
    {
        //Step 1: Rotate the player towards the killer, and vice-versa.
        yield return LerpToGameObjectTop(killer, GameObject.FindWithTag("MainCamera").transform);

        //Step 2: wait for just a bit so the two friends can stare at each other
        yield return new WaitForSeconds(staredownTime);

        //Step 3: switch to the death scene
        SceneManager.LoadScene(deathSceneName);
    }

    /*This is borrowed from DialogueInitiator.LerpToCenterDialgoue. Rotates the player's camera to the top
    of a gameObject, or to the gameObject's position if it doesn't have a mesh renderer*/
    IEnumerator LerpToGameObjectTop(GameObject obj, Transform playerCamera)
    {
        //Calculate the position of obj's top (or obj's position,
        //if it doesn't have a mesh renderer)
        MeshRenderer objMesh;
        Vector3 trueObjTop;

        if(obj.TryGetComponent<MeshRenderer>(out objMesh))
        {
            trueObjTop = obj.transform.position + new Vector3(0f, objMesh.bounds.size.y, 0f);
        }
        else
        {
            objMesh = obj.GetComponentInChildren<MeshRenderer>();
            trueObjTop = objMesh != null 
            ? obj.transform.position + new Vector3(0f, objMesh.bounds.size.y, 0f)
            : obj.transform.position;
        }

        //Calculate the direction from the camera to obj
        Vector3 camToObj = (trueObjTop - playerCamera.position).normalized;

        /*BUT LOOK THE OPPISITE DIRECTION
        This is specifically for the minotaur, whose position is in the floor
        Because we want the player to look up
        By reflecting the vector across the plane normal'd by the player's up vector,
        we can get the other direction to look!*/
        camToObj = Vector3.Reflect(camToObj, this.gameObject.transform.up);
        

        //Save the initial forward vectors of the cam transform
        Vector3 camForward = playerCamera.forward;

        //While the maximum initialization time hasn't passed...
        var timePassed = 0f;
        while (timePassed < initiateTime)
        {
            //Lerp the forward vector of both the camera transform towards its ideal facing directions
            playerCamera.forward = Vector3.Lerp(camForward, camToObj, timePassed / initiateTime);

            //Let this tiny movement of both items happen
            yield return null;

            //Increment timer
            timePassed += Time.deltaTime;
        }

        //force the rotations the file step of the way, just in case
        playerCamera.forward = camToObj;
    }
}
