﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerLogic : MonoBehaviour {

    GameObject cam1;
    MoveCamera cameraLerpScript;
    SmoothMouseLook lookScript;
    Transform camTransform;
    Vector3 startPosition;

    public ParticleSystem gunParticle;

    GameObject ammoText;
    GameObject reloadText;

    enum Inventory
    {
        Rifle,
        Pistol,
        Knife,
        Katana,
        Sniper
    }

    enum PlayerStates
    {
        Run,
        Walk,
        Crouch,
        Prone,
        Idle,
        Jump
    }
    
    [System.Serializable]
    public struct Weapons
    {
        public string weaponName;
        public byte currentAmmo;
        public byte magazineSize;
        public byte remainingAmmo;
        public byte totalAmmo;
        public float shootDelay;
        public float reloadDelay;
        public float range;
        public float damageValue;
    }

    float walkHeight;
    float crouchHeight;
    float proneHeight;

    bool isWalking;

    byte currentWeaponIndex;

    public Texture2D crossHair;

    public byte playerSpeed;
    public float strafeSlow;
    public float speedModifier;
    public float jumpForce;
    public float gravity;

    float initialVelocity;
    float currentVelocity;
    float maxVelocity;
    float forwardAccelerationRate;
    float reverseAccelerationRate;
    float deccelerationRate;

    Vector3 moveDirection;

    CharacterController controller;

    RaycastHit hit;
    RaycastHit interactHit;

    Inventory currentSelected;
    PlayerStates currentState;
    public Weapons[] weapons;
    float timeStamp;
    bool reloadState;

    // Use this for initialization
    void Start() {
        cam1 = GameObject.Find("Main Camera");
        camTransform = cam1.transform;
        cameraLerpScript = cam1.GetComponent<MoveCamera>();
        lookScript = cam1.GetComponent<SmoothMouseLook>();

        startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        ammoText = GameObject.Find("AmmoText");
        reloadText = GameObject.Find("ReloadText");
        reloadText.SetActive(false);

        playerSpeed = 2;
        strafeSlow = 0.5f;
        speedModifier = 1.0f;
        jumpForce = 3.0f;
        gravity = 10.0f;

        crouchHeight = 1.75f;
        walkHeight = 2.5f;
        proneHeight = 1.0f;

        isWalking = false;

        controller = GetComponent<CharacterController>();

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        currentSelected = Inventory.Rifle;
        currentState = PlayerStates.Idle;
        currentWeaponIndex = 0;

        initialVelocity = 1.0f;
        currentVelocity = 0.0f;
        maxVelocity = 5.0f;
        forwardAccelerationRate = 2.5f;
        reverseAccelerationRate = 0.5f;
        deccelerationRate = 2.5f;

        timeStamp = Time.time;
        reloadState = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement Handler for when player is on and off the ground
        if (controller.isGrounded)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                currentVelocity += forwardAccelerationRate * Time.deltaTime;

                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.LeftControl))
                {
                    currentState = PlayerStates.Walk;
                }

                moveDirection = transform.TransformDirection(moveDirection);

                //Sprinting key (Left Shift)
                if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.Z))
                {
                    currentState = PlayerStates.Run;
                }
            }
            else if ((Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) &&!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.LeftControl))
            {
                currentState = PlayerStates.Idle;
                currentVelocity -= deccelerationRate * Time.deltaTime;
            }

            //Crouch key (Left Control)
            if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.LeftShift) && !Input.GetKeyDown(KeyCode.Z))
            {
                currentState = PlayerStates.Crouch;
                cameraLerpScript.LerpCamera("B");
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                cameraLerpScript.LerpCamera("A");
            }

            //Prone key ("Z" key)
            if (Input.GetKey(KeyCode.Z) && !Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.LeftShift))
            {
                currentState = PlayerStates.Prone;
                cameraLerpScript.LerpCamera("C");
            }
            else if (Input.GetKeyUp(KeyCode.Z))
            {
                cameraLerpScript.LerpCamera("A");
            }

            //Jumping (Space)
            if (Input.GetKey(KeyCode.Space))
            {
                moveDirection.y = jumpForce;
                currentState = PlayerStates.Jump;
            }
        }
        else if (!controller.isGrounded)
        {
            //Gravity drop
            moveDirection.y -= gravity * Time.deltaTime;
        }

        currentVelocity = Mathf.Clamp(currentVelocity, initialVelocity, maxVelocity);

        //Player movement modifier
        controller.Move(moveDirection * speedModifier * currentVelocity * Time.deltaTime);

        //Weapon change key handler
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            //If current weapon is not alredy the target weapon
            if (currentSelected != Inventory.Rifle)
            {
                currentSelected = Inventory.Rifle;
                currentWeaponIndex = 0;
                reloadState = false;
                timeStamp = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            //If current weapon is not alredy the target weapon
            if (currentSelected != Inventory.Pistol)
            {
                currentSelected = Inventory.Pistol;
                currentWeaponIndex = 1;
                reloadState = false;
                timeStamp = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            //If current weapon is not alredy the target weapon
            if (currentSelected != Inventory.Knife)
            {
                currentSelected = Inventory.Knife;
                currentWeaponIndex = 2;
                reloadState = false;
                timeStamp = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            //If current weapon is not alredy the target weapon
            if (currentSelected != Inventory.Katana)
            {
                currentSelected = Inventory.Katana;
                currentWeaponIndex = 3;
                reloadState = false;
                timeStamp = Time.time;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            //If current weapon is not alredy the target weapon
            if (currentSelected != Inventory.Sniper)
            {
                currentSelected = Inventory.Sniper;
                currentWeaponIndex = 4;
                reloadState = false;
                timeStamp = Time.time;
            }
        }
        
        //Reload key ("R" key)
        if (Input.GetKeyDown(KeyCode.R) && !reloadState)
        {
            reloadState = true;
            timeStamp = Time.time;
        }

        //Left Mouse Button Shoot
        if (Input.GetMouseButton(0))
        {
            if (Time.time > (timeStamp + weapons[currentWeaponIndex].shootDelay) && !reloadState)
            {
                ShootRay();
                weapons[currentWeaponIndex].currentAmmo -= 1;
                gunParticle.Emit(1);
                timeStamp = Time.time;
            }
        }

        //Check for Reload need
        if (weapons[currentWeaponIndex].currentAmmo <= 0 && !reloadState)
        {
            reloadState = true;
            timeStamp = Time.time;
        }

        if (reloadState)
        {
            reloadText.SetActive(true);
            if (Time.time >= timeStamp + weapons[currentWeaponIndex].reloadDelay)
            {
                ReloadWeapon();
            }
        }

        //Check current movement state to adjust speed multiplier accordingly
        switch (currentState)
        {
            case PlayerStates.Walk:
                controller.height = walkHeight;
                lookScript.minimumY = -80f;
                lookScript.maximumY = 80f;
                speedModifier = 1.0f;
                break;
            case PlayerStates.Run:
                controller.height = walkHeight;
                lookScript.minimumY = -80f;
                lookScript.maximumY = 80f;
                speedModifier = 2.0f;
                break;
            case PlayerStates.Crouch:
                controller.height = crouchHeight;
                lookScript.minimumY = -40f;
                lookScript.maximumY = 80f;
                speedModifier = 0.5f;
                break;
            case PlayerStates.Prone:
                controller.height = proneHeight;
                lookScript.minimumY = 0f;
                lookScript.maximumY = 40f;
                speedModifier = 0.25f;
                break;
            case PlayerStates.Idle:
                controller.height = walkHeight;
                lookScript.minimumY = -80f;
                lookScript.maximumY = 80f;
                speedModifier = 1.0f;
                break;
            case PlayerStates.Jump:
                controller.height = walkHeight;
                lookScript.minimumY = -80f;
                lookScript.maximumY = 80f;
                speedModifier = 1.0f;
                break;
        }

        Ray ray = new Ray(camTransform.position, camTransform.forward);
        if (Physics.Raycast(ray, out interactHit, 10))
        {
            if (interactHit.collider.tag == "Interactable")
            {
                if (Input.GetKey(KeyCode.E))
                {
                    interactHit.collider.gameObject.SendMessage("Activate");
                }
            }
        }

        ammoText.GetComponent<Text>().text = weapons[currentWeaponIndex].currentAmmo + "/" + weapons[currentWeaponIndex].remainingAmmo;
    }

    //Shooting raycast check
    void ShootRay()
    {
        //Fires ray from camera, centre of screen into 3D space
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        if (Physics.Raycast(ray, out hit, weapons[currentWeaponIndex].range))
        {
            print(hit.collider.name);
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan);
            if (hit.collider.tag == "Mutant")
            {
                hit.collider.gameObject.SendMessage("TakeDamage", weapons[currentWeaponIndex].damageValue);
            }
        }
    }

    //Enemy detection
    void OnControllerColliderHit(ControllerColliderHit collider)
    {
        if (collider.gameObject.tag == "Mutant")
        {
            KillPlayer();
        }
    }

    //Die function
    void KillPlayer()
    {
        print("die");
        transform.position = startPosition;
    }

    public bool IsWalking()
    {
        if (currentState == PlayerStates.Walk || currentState == PlayerStates.Run)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        return isWalking;
    }

    //Reload Weapon Function
    void ReloadWeapon()
    {
        //Check if remaining ammo is not enough to fill up a full magazine
        if (weapons[currentWeaponIndex].remainingAmmo < weapons[currentWeaponIndex].magazineSize)
        {
            weapons[currentWeaponIndex].currentAmmo = weapons[currentWeaponIndex].remainingAmmo;
            weapons[currentWeaponIndex].remainingAmmo = 0;
        }
        else //Normal reload
        {
            weapons[currentWeaponIndex].currentAmmo = weapons[currentWeaponIndex].magazineSize;
            weapons[currentWeaponIndex].remainingAmmo -= weapons[currentWeaponIndex].magazineSize;
        }

        reloadState = false;
        reloadText.SetActive(false);
    }
    
    
    /*
    var bulletSpeed: float = 1000; // bullet speed in meters/second var shotSound: AudioClip;

    //Delayed raycast shooting (KIV)
    function Fire()
    {
        if (audio) audio.PlayOneShot(shotSound); // the sound plays immediately 
        var hit: RaycastHit; // do the exploratory raycast first: 
        if (Physics.Raycast(transform.position, transform.forward, hit))
        {
            var delay = hit.distance / bulletSpeed; // calculate the flight time 
            var hitPt = hit.point;
            hitPt.y -= delay * 9.8; // calculate the bullet drop at the target 
            var dir = hitPt - transform.position; // use this to modify the shot direction 
            yield WaitForSeconds(delay); // wait for the flight time 

            // then do the actual shooting: 
            if (Physics.Raycast(transform.position, dir, hit)
            {
                // do here the usual job when something is hit: 
                // apply damage, instantiate particles etc. 
            }
        }
    }*/
}
