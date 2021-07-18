//Copyright (c) 2021 Magpie Paulsen
//Written by Thomas Applewhite

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [Tooltip("Where tp place the player when they die")]
    public Transform killLocation;

    /*// Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/

    public void Kill()
    {
        Debug.Log("Player is dead!");
        this.gameObject.transform.position = killLocation.position;
    }
}
