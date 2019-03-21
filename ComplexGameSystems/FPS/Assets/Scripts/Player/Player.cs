using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Mechanics")]
    public int health = 100;
    public float runSpeeed = 7.5f;
    public float walkSpeed = 6f;
    public float gravity = 10f;
    public float crouchSpeed = 4f;
    public float jumpHeight = 20f;
    public float interactRange = 10f;
    public float groundRayDistance = 1.1f;

    [Header("References")]
    public Camera attachedCamera;
    public Transform hand;

    // Animation
    private Animator anim;

    // Movement 
    private CharacterController controller;
    // Current movement
    private Vector3 movement;

    //Weapons
    public Weapon currentWeapon;
    public List<Weapon> weapons;
    private int currentWeaponIndex = 0;

    void OnDrawGizmos()
    {
        Ray groundRay = new Ray(transform.position, -transform.up);
        Gizmos.DrawLine(groundRay.origin, groundRay.origin + groundRay.direction * groundRayDistance);
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {

    }

    #region Initialisation
    void CreateUI()
    {

    }
    
    void RegisterWeapons()
    {

    }
    #endregion

    #region Controls
    /// <summary>
    /// Moves the character controller in direction of input
    /// </summary>
    /// <param name="inputH"> Horizontal Input </param>
    /// <param name="inputV"> Vertical Input </param>
    void Move(float inputH, float inputV)
    {
        // Create direction from input
        Vector3 input = new Vector3(inputH, 0, inputV);
        // Localise direction to player
        input = transform.TransformDirection(input);
        // Set move speed
        float moveSpeed = walkSpeed;
        // Apply movement
        movement.x = input.x * moveSpeed;
        movement.z = input.z * moveSpeed;
    }

    #endregion

    #region Combat    
    /// <summary>
    /// Switches between weapons with given direction
    /// </summary>
    /// <param name="direction"> -1 to 1 number for list selection </param>
    void SwitchWeapon(int direction)
    {

    }
    
    /// <summary>
    /// Disables GameObjects of every attached weapon
    /// </summary>
    void DisableAllWeapons()
    {

    }

    /// <summary>
    /// Adds weapon to list and attaches to player's hand
    /// </summary>
    /// <param name="weaponToPickup"> Weapon to place in hand </param>
    void Pickup(Weapon weaponToPickup)
    {

    }

    /// <summary>
    /// Removes weapon to list and removes from player's hand
    /// </summary>
    /// <param name="weaponToDrop"> Weapon to remove from hand </param>
    void Drop(Weapon weaponToDrop)
    {

    }

    /// <summary>
    /// Sets currentWeapon to weapon at given index
    /// </summary>
    /// <param name="index"> Weapon Index </param>
    void SelectWeapon(int index)
    {

    }
    #endregion

    #region Actions
    /// <summary>
    /// Player movement using CharacterController
    /// </summary>
    void Movement()
    {
        // Get input from user
        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");
        Move(inputH, inputV);
        // Is the controller grounded
        Ray groundRay = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        // If jump is pressed
        if(Physics.Raycast(groundRay, out hit, groundRayDistance))
        {
            // If jump is pressed
            if(Input.GetButtonDown("Jump"))
            {
                // Move controller up
                movement.y = jumpHeight;
            }
        }
        
        // Apply gravity 
        movement.y -= gravity * Time.deltaTime;

        movement.y = Mathf.Max(movement.y, -gravity);
        // Move the controller
        controller.Move(movement * Time.deltaTime);
    }
    /// <summary>
    /// Interaction with items in the world
    /// </summary>
    void Interact()
    {

    }
    /// <summary>
    /// Using the current weapon to fire a bullet
    /// </summary>
    void Shooting()
    {

    }
    /// <summary>
    /// Cycling through availale weapons
    /// </summary>
    void Switching()
    {

    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        Movement();
        Interact();
        Shooting();
        Switching();
    }
}
