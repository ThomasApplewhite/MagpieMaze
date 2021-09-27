/*Copyright (c) 2020 Sebastian Lague
Modifications Copyright (c) 2021 Magpie Paulsen
Modifications written by Thomas Applewhite

This portion of the software is distributed under the MIT License. Please
see Sebastian Lague's MIT License (provided with this program) for more detail.*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPSSystemController : PortalTraveller {

    //Speed Values
    public float walkSpeed = 3;
    public float runSpeed = 6;
    public float smoothMoveTime = 0.1f;
    public float jumpForce = 8;
    public float gravity = 18;

    //Look Values
    public bool lockCursor;
    public float mouseSensitivity = 1.5f;
    public Vector2 pitchMinMax = new Vector2 (-40, 85);
    public float rotationSmoothTime = 0.1f;

    //Camera Values
    CharacterController controller;
    Camera cam;
    public float yaw;
    public float pitch;
    float smoothYaw;
    float smoothPitch;

    //Rotation Values
    float yawSmoothV;
    float pitchSmoothV;
    float verticalVelocity;
    Vector3 velocity;
    Vector3 smoothV;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    //Input Values
    Vector2 moveInputs = new Vector2(0f, 0f);
    Vector2 lookInputs = new Vector2(0f, 0f);

    //Other
    bool jumping;
    bool sprinting;
    float lastGroundedTime;
    bool disabled;
    

    void Start () {
        cam = Camera.main;
        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        controller = GetComponent<CharacterController> ();

        yaw = transform.eulerAngles.y;
        pitch = cam.transform.localEulerAngles.x;
        smoothYaw = yaw;
        smoothPitch = pitch;
    }

    void Update () {
        /*if (Input.GetKeyDown (KeyCode.P)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Break ();
        }
        if (Input.GetKeyDown (KeyCode.O)) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            disabled = !disabled;
        }*/

        if (disabled) {
            return;
        }

        Vector2 input = moveInputs; //new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));

        Vector3 inputDir = new Vector3 (input.x, 0, input.y).normalized;
        Vector3 worldInputDir = transform.TransformDirection (inputDir);

        float currentSpeed = sprinting ? runSpeed : walkSpeed;
        Vector3 targetVelocity = worldInputDir * currentSpeed;
        velocity = Vector3.SmoothDamp (velocity, targetVelocity, ref smoothV, smoothMoveTime);

        verticalVelocity -= gravity * Time.deltaTime;
        velocity = new Vector3 (velocity.x, verticalVelocity, velocity.z);

        var flags = controller.Move (velocity * Time.deltaTime);
        if (flags == CollisionFlags.Below) {
            jumping = false;
            lastGroundedTime = Time.time;
            verticalVelocity = 0;
        }

        /*if (Input.GetKeyDown (KeyCode.Space)) {
            float timeSinceLastTouchedGround = Time.time - lastGroundedTime;
            if (controller.isGrounded || (!jumping && timeSinceLastTouchedGround < 0.15f)) {
                jumping = true;
                verticalVelocity = jumpForce;
            }
        }*/

        float mX = lookInputs.x;
        float mY = lookInputs.y;

        // Verrrrrry gross hack to stop camera swinging down at start
        float mMag = Mathf.Sqrt (mX * mX + mY * mY);
        if (mMag > 5) {
            mX = 0;
            mY = 0;
        }

        yaw += mX * mouseSensitivity;
        pitch -= mY * mouseSensitivity;
        pitch = Mathf.Clamp (pitch, pitchMinMax.x, pitchMinMax.y);
        smoothPitch = Mathf.SmoothDampAngle (smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle (smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);

        //transform.eulerAngles = Vector3.up * smoothYaw;
        //cam.transform.localEulerAngles = Vector3.right * smoothPitch;

    }

    public void UpdateMoveInputs(InputAction.CallbackContext context)
    {
        moveInputs = context.ReadValue<Vector2>();
    }

    public void UpdateLookInputs(InputAction.CallbackContext context)
    {
        lookInputs = context.ReadValue<Vector2>();

        float mX = lookInputs.x;
        float mY = lookInputs.y;

        yaw += mX * mouseSensitivity;
        pitch -= mY * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, rotationSmoothTime);

        if(this.enabled)
        {
            transform.eulerAngles = Vector3.up * smoothYaw;
            cam.transform.localEulerAngles = Vector3.right * smoothPitch;
        }
        

        //transform.eulerAngles += new Vector3(0f, lookInputs.x, 0f); //left-right yaw
        //cam.transform.localEulerAngles += new Vector3(-lookInputs.y * mouseSensitivity, 0f, 0f); // up-down pitch

    }

    public void Jump()
    {
        float timeSinceLastTouchedGround = Time.time - lastGroundedTime;
        if (controller.isGrounded || (!jumping && timeSinceLastTouchedGround < 0.15f)) {
            jumping = true;
            verticalVelocity = jumpForce;
        }
    }

    public void Sprint()
    {
        Debug.Log("FPSSystemController: Sprint is not currently implemented");
    }

    public void DebugPause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Break ();
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        disabled = !disabled;
    }

    public override void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) {
        transform.position = pos;
        Vector3 eulerRot = rot.eulerAngles;
        float delta = Mathf.DeltaAngle (smoothYaw, eulerRot.y);
        yaw += delta;
        smoothYaw += delta;
        transform.eulerAngles = Vector3.up * smoothYaw;
        velocity = toPortal.TransformVector (fromPortal.InverseTransformVector (velocity));
        Physics.SyncTransforms ();
    }

}