﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent (typeof(CollectableLogic))]
public class PlayerLogic : MonoBehaviour {

    GameObject gameController;
    SettingsConfig settingsScript;

    GameObject cam1;
    GameObject focusCam;
    GameObject scopeCam;
    GameObject scopeFocusCam;
    GameObject leanPivot;
    SmoothMouseLook lookScript;
    Transform camTransform;
    Vector3 startPosition;

    CollectableLogic collectScript;

    public ParticleSystem gunParticle;
    public ParticleSystem pistolParticle;

    public bool pauseGame;
    public bool itemView;
    public bool inspectView;
    public bool optionsView;

    public bool aimDownSight;
    public bool frontfree;
    public bool rightfree;
    public bool leftfree;

    public Transform originalRotation;
    public Transform leftRotation;
    public Transform rightRotation;

    GameObject ammoText;
    GameObject reloadText;
    GameObject weaponIcon;

    GameObject hitMarker;

    GameObject healthBar;

    GameObject bloodOverlay;
    Color bloodAlpha;
    GameObject shadowOverlay;
    Color shadowAlpha;

    GameObject itemMenu;
    GameObject pauseMenu;
    GameObject optionsMenu;

    public GameObject rifleObject;
    public GameObject rifleSight;
    public GameObject rifleHip;

    public GameObject pistolObject;
    public GameObject knifeObject;

    public GameObject bottlePrefab;

    Animator rifleAnimator;
    Animator knifeAnimator;

    float lerpStart;
    float lerpTime;

    public enum Inventory
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
        public Sprite weaponSprite;
        public bool locked;
        public int currentAmmo;
        public int magazineSize;
        public int remainingAmmo;
        public int totalAmmo;
        public float shootDelay;
        public float reloadDelay;
        public float range;
        public float damageValue;
    }

    public float walkHeight;
    public float crouchHeight;
    public float proneHeight;

    bool isWalking;

    public bool focus;

    public byte currentWeaponIndex;

    public float playerHealth;
    public float playerSpeed;
    public float strafeSlow;
    public float jumpForce;
    public float gravity;

    public float speedModifier;
    public float runModifier;
    public float walkModifier;
    public float crouchModifier;
    public float proneModifier;

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
    RaycastHit aimHit;

    RaycastHit surfaceHit;

    public Inventory currentSelected;
    PlayerStates currentState;
    public Weapons[] weapons;
    public float timeStamp;
    public bool reloadState;
    public bool holdingBottle;

    float regenTimer;
    float regenDelay;
    bool regenHealth;

    [FMODUnity.EventRef]
    public string footsteps = "event:/PlayerFootstep";
    FMOD.Studio.EventInstance walkingEv;
    FMOD.Studio.ParameterInstance walkingParam;
    public FMOD.Studio.ParameterInstance surfaceParam;

    [FMODUnity.EventRef]
    public string panting = "event:/PlayerPant";
    FMOD.Studio.EventInstance pantingEv;
    FMOD.Studio.ParameterInstance pantingParam;

    [FMODUnity.EventRef]
    public string rifleSound = "event:/Rifle";

    [FMODUnity.EventRef]
    public string pistolSound = "event:/Pistol";

    [FMODUnity.EventRef]
    public string knifeSound = "event:/Knife";


    float param;

    // Use this for initialization
    void Start() {
        gameController = GameObject.Find("GameController");
        settingsScript = gameController.GetComponent<SettingsConfig>();

        cam1 = GameObject.Find("Main Camera");
        focusCam = GameObject.Find("FocusCamera");
        scopeCam = GameObject.Find("ScopeCamera");
        scopeFocusCam = GameObject.Find("ScopeFocusCamera");
        leanPivot = GameObject.Find("LeanPivot");
        camTransform = cam1.transform;
        lookScript = cam1.GetComponent<SmoothMouseLook>();

        collectScript = GetComponent<CollectableLogic>();

        pauseGame = false;
        itemView = false;
        inspectView = false;

        rifleAnimator = rifleObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        knifeAnimator = knifeObject.GetComponent<Animator>();

        startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        aimDownSight = false;
        frontfree = false;
        rightfree = false;
        leftfree = false;

        hitMarker = GameObject.Find("HitMarker");
        hitMarker.SetActive(false);

        ammoText = GameObject.Find("AmmoText");
        reloadText = GameObject.Find("ReloadText");
        reloadText.SetActive(false);
        weaponIcon = GameObject.Find("Weapon Icon");

        healthBar = GameObject.Find("Health Bar");

        bloodOverlay = GameObject.Find("BloodOverlay");
        bloodAlpha = bloodOverlay.GetComponent<Image>().color;
        shadowOverlay = GameObject.Find("ShadowOverlay");
        shadowAlpha = shadowOverlay.GetComponent<Image>().color;

        itemMenu = GameObject.Find("ItemSelect");
        itemMenu.SetActive(false);
        pauseMenu = GameObject.Find("PauseMenu");
        pauseMenu.SetActive(false);
        optionsMenu = GameObject.Find("OptionsMenu");
        optionsMenu.SetActive(false);

        lerpStart = 0f;
        lerpTime = 0.2f;

        playerHealth = settingsScript.playerHealth;
        playerSpeed = settingsScript.playerSpeed;
        strafeSlow = settingsScript.strafeSlow;
        jumpForce = settingsScript.jumpForce;
        gravity = settingsScript.gravity;

        speedModifier = settingsScript.walkModifier;
        runModifier = settingsScript.runModifier;
        walkModifier = settingsScript.walkModifier;
        crouchModifier = settingsScript.crouchModifier;
        proneModifier = settingsScript.proneModifier;

        initialVelocity = settingsScript.initialVelocity;
        currentVelocity = settingsScript.currentVelocity;
        maxVelocity = settingsScript.maxVelocity;
        forwardAccelerationRate = settingsScript.forwardAccelerationRate;
        reverseAccelerationRate = settingsScript.reverseAccelerationRate;
        deccelerationRate = settingsScript.deccelerationRate;

        walkHeight = settingsScript.walkHeight;
        crouchHeight = settingsScript.crouchHeight;
        proneHeight = settingsScript.proneHeight;

        isWalking = false;

        focus = false;

        controller = GetComponent<CharacterController>();

        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        currentSelected = Inventory.Rifle;
        currentState = PlayerStates.Idle;
        currentWeaponIndex = 0;

        timeStamp = Time.time;
        reloadState = false;

        regenTimer = 0.0f;
        regenDelay = 5.0f;
        regenHealth = false;

        walkingEv = FMODUnity.RuntimeManager.CreateInstance(footsteps);
        walkingEv.getParameter("Speed", out walkingParam);
        walkingEv.getParameter("Surface", out surfaceParam);
        walkingEv.start();
        walkingParam.setValue(0.0f);
        surfaceParam.setValue(0.0f);

        pantingEv = FMODUnity.RuntimeManager.CreateInstance(panting);
        pantingEv.getParameter("Panting", out pantingParam);
        pantingEv.start();
        pantingParam.setValue(0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            SaveLoad.Save();
        }

        if (pauseGame)
        {
            pauseMenu.SetActive(true);

            //Pause Key
            if (Input.GetKeyDown(KeyCode.Escape) && !itemView)
            {
                pauseGame = false;
            }

            if (itemView)
            {
                itemMenu.SetActive(true);

                if (Input.GetKeyDown(KeyCode.F) && !inspectView)
                {
                    ButtonTrigger("inspect");
                }

                if (inspectView)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        collectScript.InspectItem();
                        inspectView = false;
                    }
                }
                else if (!inspectView)
                {
                    if (Input.GetKeyDown(KeyCode.Escape))
                    {
                        itemView = false;
                    }
                }
            }
            else if (!itemView)
            {
                itemMenu.SetActive(false);
            }
        }
        else
        {
            pauseMenu.SetActive(false);
            itemMenu.SetActive(false);

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

                    Ray surfaceRay = new Ray(transform.position, -transform.up);

                    if (Physics.Raycast(surfaceRay, out surfaceHit, controller.height + 1))
                    {
                        Debug.LogFormat("name:{0}       tag:{1}",surfaceHit.collider.name, surfaceHit.collider.tag);
                        if (surfaceHit.collider.tag == "Concrete")
                        {
                            ChangeSurface("Concrete");
                        }
                        else if (surfaceHit.collider.tag == "Wood")
                        {
                            ChangeSurface("Wood");
                        }
                        else if (surfaceHit.collider.tag == "Dirt")
                        {
                            ChangeSurface("Dirt");
                        }
                        else
                        {

                        }
                    }
                }
                else if ((Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.LeftControl))
                {
                    moveDirection = new Vector3(0, 0, 0);
                    moveDirection = transform.TransformDirection(moveDirection);

                    currentState = PlayerStates.Idle;
                    currentVelocity -= deccelerationRate * Time.deltaTime;
                }

                //Crouch key (Left Control)
                if (Input.GetKey(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.LeftShift) && !Input.GetKeyDown(KeyCode.Z))
                {
                    currentState = PlayerStates.Crouch;
                }
                else if (Input.GetKeyUp(KeyCode.LeftControl))
                {

                }

                //Prone key ("Z" key)
                if (Input.GetKey(KeyCode.Z) && !Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetKeyDown(KeyCode.LeftShift))
                {
                    currentState = PlayerStates.Prone;
                }
                else if (Input.GetKeyUp(KeyCode.Z))
                {

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

            if (Input.GetAxis("Horizontal") > 0 || Input.GetAxis("Vertical") > 0)
            {
                walkingParam.setValue(speedModifier * currentVelocity);
            }
            else
            {
                walkingParam.setValue(0);
            }

            //Weapon change key handler
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //If current weapon is not alredy the target weapon
                if (currentSelected != Inventory.Rifle)
                {
                    if (currentSelected == Inventory.Knife)
                    {
                        knifeAnimator.SetBool("Swapping", true);
                        StartCoroutine(SwitchWeapon(0));
                    }
                    else
                    {
                        StartCoroutine(SwitchWeapon(0));
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //If current weapon is not alredy the target weapon
                if (currentSelected != Inventory.Pistol)
                {
                    if (currentSelected == Inventory.Rifle)
                    {
                        rifleAnimator.SetBool("Swapping", true);
                        StartCoroutine(SwitchWeapon(1));
                    }
                    else if (currentSelected == Inventory.Knife)
                    {
                        knifeAnimator.SetBool("Swapping", true);
                        StartCoroutine(SwitchWeapon(1));
                    }
                    else
                    {
                        StartCoroutine(SwitchWeapon(1));
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                //If current weapon is not alredy the target weapon
                if (currentSelected != Inventory.Knife)
                {
                    if (currentSelected == Inventory.Rifle)
                    {
                        rifleAnimator.SetBool("Swapping", true);
                        StartCoroutine(SwitchWeapon(2));
                    }
                    else
                    {
                        StartCoroutine(SwitchWeapon(2));
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha4) && !weapons[3].locked)
            {
                //If current weapon is not alredy the target weapon
                if (currentSelected != Inventory.Katana)
                {
                    SwitchWeapon(3);
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha5) && !weapons[4].locked)
            {
                //If current weapon is not alredy the target weapon
                if (currentSelected != Inventory.Sniper)
                {
                    SwitchWeapon(4);
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
                if (!holdingBottle)
                {
                    if (Time.time > (timeStamp + weapons[currentWeaponIndex].shootDelay) && !reloadState)
                    {
                        ShootRay();
                        if (currentSelected != Inventory.Knife)
                        {
                            weapons[currentWeaponIndex].currentAmmo -= 1;
                        }

                        if (currentWeaponIndex == 0)
                        {
                            rifleAnimator.SetBool("Firing", true);
                            FMODUnity.RuntimeManager.PlayOneShot(rifleSound);
                            gunParticle.Emit(1);
                        }
                        else if (currentWeaponIndex == 1)
                        {
                            FMODUnity.RuntimeManager.PlayOneShot(pistolSound);
                            pistolParticle.Emit(1);
                        }
                        else if (currentWeaponIndex == 2)
                        {
                            knifeAnimator.SetBool("Attacking", true);
                            FMODUnity.RuntimeManager.PlayOneShot(knifeSound);
                        }
                        timeStamp = Time.time;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (currentSelected == Inventory.Rifle)
                {
                    rifleAnimator.SetBool("Firing", false);
                }
                else if (currentSelected == Inventory.Knife)
                {
                    knifeAnimator.SetBool("Attacking", false);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (holdingBottle)
                {
                    ThrowBottle();
                }
            }

            if (Input.GetMouseButtonDown(1) && !aimDownSight)
            {
                Ray aimRay = new Ray(transform.position, transform.forward);
                Ray aimRayLeft = new Ray(transform.position, (transform.forward-transform.right));
                Ray aimRayRight = new Ray(transform.position, (transform.forward + transform.right));

                if (Physics.Raycast(aimRay, out aimHit, 2))
                {
                    if (Physics.Raycast(aimRayLeft, out aimHit, 2))
                    {
                        if (Physics.Raycast(aimRayRight, out aimHit, 2))
                        {
                            frontfree = false;
                            leftfree = false;
                            rightfree = false;
                        }
                        else
                        {
                            rightfree = true;
                        }
                    }
                    else 
                    {
                        leftfree = true;
                    }
                }
                else
                {
                    frontfree = true;
                }
                lerpStart = 0f;
                aimDownSight = true;
            }

            if (Input.GetMouseButtonUp(1))
            {
                lerpStart = 0f;
                aimDownSight = false;
            }

            //Collectable item menu
            if (Input.GetKeyDown(KeyCode.I))
            {
                itemView = true;
            }

            //Pause Key
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                pauseGame = true;
            }

            //Check for Reload need
            if (currentSelected != Inventory.Knife)
            {
                if (weapons[currentWeaponIndex].currentAmmo <= 0 && !reloadState)
                {
                    reloadState = true;
                    timeStamp = Time.time;
                }
            }

            if (reloadState)
            {
                rifleAnimator.SetBool("Reload", true);
                reloadText.SetActive(true);
                if (Time.time >= timeStamp + weapons[currentWeaponIndex].reloadDelay)
                {
                    ReloadWeapon();
                }
            }

            //Health Handler
            if (playerHealth < 100.0f)
            {
                if (regenTimer < regenDelay)
                {
                    regenTimer += Time.deltaTime;
                }
                else
                {
                    regenHealth = true;
                }
            }

            if (playerHealth < 50.0f)
            {
                bloodAlpha = bloodOverlay.GetComponent<Image>().color;
                bloodAlpha.a += 0.05f;
            }
            else if (playerHealth > 50.0f)
            {
                bloodAlpha = bloodOverlay.GetComponent<Image>().color;
                bloodAlpha.a -= 0.05f;
            }

            if (regenHealth)
            {
                playerHealth += 1.0f;
                if (playerHealth > 100)
                {
                    playerHealth = 100.0f;
                    regenTimer = 0.0f;
                    regenHealth = false;
                }
            }

            //Focus
            if (focus)
            {
                shadowAlpha = shadowOverlay.GetComponent<Image>().color;
                shadowAlpha.a += 0.05f;
                focusCam.SetActive(true);
            }
            else
            {
                shadowAlpha.a -= 0.05f;
                focusCam.SetActive(false);
            }

            //Check current movement state to adjust speed multiplier accordingly
            switch (currentState)
            {
                case PlayerStates.Walk:
                    controller.height = walkHeight;
                    lookScript.minimumY = -80f;
                    lookScript.maximumY = 80f;
                    speedModifier = settingsScript.walkModifier;
                    pantingParam.setValue(0.0f);
                    rifleAnimator.SetBool("Sprinting", false);
                    rifleAnimator.SetBool("Jump", false);
                    break;
                case PlayerStates.Run:
                    controller.height = walkHeight;
                    lookScript.minimumY = -80f;
                    lookScript.maximumY = 80f;
                    speedModifier = settingsScript.runModifier;
                    pantingParam.setValue(1.0f);
                    rifleAnimator.SetBool("Sprinting", true);
                    rifleAnimator.SetBool("Jump", false);
                    break;
                case PlayerStates.Crouch:
                    controller.height = crouchHeight;
                    lookScript.minimumY = -40f;
                    lookScript.maximumY = 80f;
                    speedModifier = settingsScript.crouchModifier;
                    pantingParam.setValue(0.0f);
                    rifleAnimator.SetBool("Sprinting", false);
                    rifleAnimator.SetBool("Jump", false);
                    break;
                case PlayerStates.Prone:
                    controller.height = proneHeight;
                    lookScript.minimumY = 0f;
                    lookScript.maximumY = 40f;
                    speedModifier = settingsScript.proneModifier;
                    pantingParam.setValue(0.0f);
                    rifleAnimator.SetBool("Sprinting", false);
                    rifleAnimator.SetBool("Jump", false);
                    break;
                case PlayerStates.Idle:
                    controller.height = walkHeight;
                    lookScript.minimumY = -80f;
                    lookScript.maximumY = 80f;
                    speedModifier = settingsScript.walkModifier;
                    pantingParam.setValue(0.0f);
                    rifleAnimator.SetBool("Sprinting", false);
                    rifleAnimator.SetBool("Jump", false);
                    break;
                case PlayerStates.Jump:
                    controller.height = walkHeight;
                    lookScript.minimumY = -80f;
                    lookScript.maximumY = 80f;
                    pantingParam.setValue(0.0f);
                    rifleAnimator.SetBool("Sprinting", false);
                    StartCoroutine(PlayerJump());
                    break;
            }

            //Item Interaction & Collection
            if (Input.GetKeyDown(KeyCode.F))
            {
                Ray ray = new Ray(camTransform.position, camTransform.forward);
                if (Physics.Raycast(ray, out interactHit, 5))
                {
                    Debug.Log(interactHit.collider.name);
                    if (interactHit.collider.tag == "Interactable")
                    {
                        interactHit.collider.gameObject.SendMessage("Activate");
                    }
               
                    if (interactHit.collider.tag == "Collectable")
                    {
                        collectScript.CollectItem(interactHit.collider.gameObject);
                    }

                    if (interactHit.collider.tag == "Bottle")
                    {
                        holdingBottle = true;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            holdingBottle = !holdingBottle;
        }
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }


        //UI Elements
        //Ammo Text
        ammoText.GetComponent<Text>().text = weapons[currentWeaponIndex].currentAmmo + "|" + weapons[currentWeaponIndex].remainingAmmo;

        //HUD Weapon Icon
        weaponIcon.GetComponent<Image>().sprite = weapons[currentWeaponIndex].weaponSprite;

        //Health Bar
        healthBar.GetComponent<Image>().fillAmount = (playerHealth / 100) * 0.61f;
        healthBar.GetComponent<Image>().fillAmount = Mathf.Clamp(healthBar.GetComponent<Image>().fillAmount, 0f, 0.66f) + 0.05f;

        //Shadow and Blood Overlay
        bloodAlpha.a = Mathf.Clamp(bloodAlpha.a, 0.0f, 1.0f);
        bloodOverlay.GetComponent<Image>().color = bloodAlpha;
        shadowAlpha.a = Mathf.Clamp(shadowAlpha.a, 0.0f, 0.8f);
        shadowOverlay.GetComponent<Image>().color = shadowAlpha;

        //Debug.LogFormat("health:{0}        Fill:{1}", playerHealth, healthBar.GetComponent<Image>().fillAmount);
        //Debug.Log(originalRotation.rotation);
        //Weapon Wheel
        switch (currentSelected)
        {
            case Inventory.Rifle:

                if (aimDownSight)
                {
                    lerpStart += Time.deltaTime;
                    if (lerpStart >= lerpTime)
                    {
                        lerpStart = lerpTime;
                    }
                    rifleObject.transform.position = Vector3.Lerp(rifleHip.transform.position, rifleSight.transform.position, lerpStart / lerpTime);

                    if (leftfree)
                    {
                        leanPivot.transform.rotation = Quaternion.Lerp(originalRotation.rotation, leftRotation.rotation, lerpStart / lerpTime);
                    }
                    else if (rightfree)
                    {
                        leanPivot.transform.rotation = Quaternion.Lerp(originalRotation.rotation, rightRotation.rotation, lerpStart / lerpTime);
                    }

                    if (focus)
                    {
                        focusCam.SetActive(false);
                    }
                }

                if (!aimDownSight)
                {
                    lerpStart += Time.deltaTime;
                    if (lerpStart >= lerpTime)
                    {
                        lerpStart = lerpTime;

                        frontfree = false;
                        leftfree = false;
                        rightfree = false;
                    }
                    rifleObject.transform.position = Vector3.Lerp(rifleSight.transform.position, rifleHip.transform.position, lerpStart / lerpTime);

                    if (leftfree)
                    {
                        leanPivot.transform.rotation = Quaternion.Lerp(leftRotation.rotation, originalRotation.rotation, lerpStart / lerpTime);
                    }
                    else if (rightfree)
                    {
                        leanPivot.transform.rotation = Quaternion.Lerp(rightRotation.rotation, originalRotation.rotation, lerpStart / lerpTime);
                    }

                    if (!focus)
                    {
                        focusCam.SetActive(true);
                    }
                }
                break;

            case Inventory.Pistol:
                break;

            case Inventory.Knife:
                break;
        }
    }

    //Shooting raycast check
    void ShootRay()
    {
        //Fires ray from camera, centre of screen into 3D space
        Ray ray = new Ray(camTransform.position, camTransform.forward);
        if (Physics.Raycast(ray, out hit, weapons[currentWeaponIndex].range))
        {
            Debug.DrawRay(ray.origin, ray.direction, Color.cyan);
            if (hit.collider.tag == "Mutant")
            {
                hit.collider.gameObject.SendMessage("TakeDamage", weapons[currentWeaponIndex].damageValue);
                StartCoroutine(Blink());
            }
            else if (hit.collider.tag == "MutantHead")
            {
                hit.collider.transform.parent.gameObject.SendMessage("TakeDamage", weapons[currentWeaponIndex].damageValue * 100);
                StartCoroutine(Blink());
            }
            else if (hit.collider.tag == "MutantBack" && currentWeaponIndex == 2)
            {
                hit.collider.transform.parent.gameObject.SendMessage("TakeDamage", weapons[currentWeaponIndex].damageValue * 1000);
            }
            Debug.Log(hit.collider.name);
        }
        GunNoise();
    }

    void ThrowBottle()
    {
        GameObject clone;

        clone = (GameObject)Instantiate(bottlePrefab, gunParticle.transform.position, cam1.transform.rotation);
        clone.GetComponent<Rigidbody>().AddForce(cam1.transform.forward * 1000);
    }

    void GunNoise()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject.GetComponent<MutantOne>() != null)
            {
                hitColliders[i].SendMessage("RecordLastSeen", transform);
                hitColliders[i].SendMessage("LosePlayer");
            }
            i++;
        }
    }

    IEnumerator Blink()
    {
        Debug.Log("hold");
        hitMarker.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        hitMarker.SetActive(false);
    }

    //Enemy detection
    void OnControllerColliderHit(ControllerColliderHit collider)
    {
        /*if (collider.gameObject.tag == "Mutant")
        {
            KillPlayer();
        }*/
    }

    public void TakeDamage(float value)
    {
        playerHealth -= value;
        regenTimer = 0.0f;
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

        rifleAnimator.SetBool("Reload", false);
        reloadState = false;
        reloadText.SetActive(false);
    }

    public IEnumerator SwitchWeapon(int target)
    {
        yield return new WaitForSeconds(0.3f);
        Debug.Log(target);
        if (target == 0)
        {
            currentSelected = Inventory.Rifle;
            currentWeaponIndex = 0;
            reloadState = false;

            rifleObject.SetActive(true);
            pistolObject.SetActive(false);
            knifeObject.SetActive(false);

            timeStamp = Time.time;
        }
        else if (target == 1)
        {
            currentSelected = Inventory.Pistol;
            currentWeaponIndex = 1;
            reloadState = false;

            rifleObject.SetActive(false);
            pistolObject.SetActive(true);
            knifeObject.SetActive(false);

            timeStamp = Time.time;
        }
        else if (target == 2)
        {
            currentSelected = Inventory.Knife;
            currentWeaponIndex = 2;
            reloadState = false;

            rifleObject.SetActive(false);
            pistolObject.SetActive(false);
            knifeObject.SetActive(true);

            timeStamp = Time.time;
        }
        else if (target == 3 && !weapons[3].locked)
        {
            currentSelected = Inventory.Katana;
            currentWeaponIndex = 3;
            reloadState = false;
            timeStamp = Time.time;
        }
        else if (target == 4 && !weapons[4].locked)
        {
            currentSelected = Inventory.Sniper;
            currentWeaponIndex = 4;
            reloadState = false;
            timeStamp = Time.time;
        }
    }

    public IEnumerator PlayerJump()
    {
        yield return new WaitForSeconds(0.25f);
        rifleAnimator.SetBool("Jump", true);
    }
    
    public void ChangeSurface(string target)
    {
        if (target == "Concrete")
        {
            surfaceParam.setValue(1.0f);
        }
        else if (target == "Dirt")
        {
            surfaceParam.setValue(2.0f);
        }
        else if (target == "Wood")
        {
            surfaceParam.setValue(3.0f);
        }
    }

    public void ButtonTrigger(string target)
    {
        switch (target)
        {
            case "artifacts":
                itemView = !itemView;
                break;

            case "pause":
                pauseGame = !pauseGame;
                break;

            case "inspect":
                if (!collectScript.Empty())
                {
                    inspectView = !inspectView;
                    collectScript.InspectItem();
                }
                break;

            case "options":
                optionsView = !optionsView;
                break;
        }
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
