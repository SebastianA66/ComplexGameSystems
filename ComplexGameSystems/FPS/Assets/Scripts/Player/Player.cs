using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour, IKillable
{
    [Header("Mechanics")]
    public int health = 100;
    public float runSpeeed = 7.5f;
    public float walkSpeed = 6f;
    public float gravity = 10f;
    public float crouchSpeed = 4f;
    public float jumpHeight = 20f;
    public float maxJumps = 2f;
    public float interactRange = 10f;
    public float groundRayDistance = 1.1f;

    private int jumps = 0;

    [Header("UI")]
    // Prefab of text to show up when interacting
    public GameObject interactUIPrefab;
    // Transform (Panel) to attach it to on start
    public Transform interactUIParent;

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

    // UI
    private GameObject interactUI;
    private TextMeshProUGUI interactText;

    void DrawRay(Ray ray, float distance)
    {
        Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * distance);
    }

    void OnDrawGizmos()
    {
        Ray interactRay = attachedCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(interactRay.origin, interactRay.origin + interactRay.direction * interactRange);

        Gizmos.DrawRay(interactRay);

        Gizmos.color = Color.red;
        Ray groundRay = new Ray(transform.position, -transform.up);
        Gizmos.DrawLine(groundRay.origin, groundRay.origin + groundRay.direction * groundRayDistance);
    }

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        CreateUI();
        RegisterWeapons();
    }

    // Use this for initialization
    void Start()
    {

    }

    #region Initialisation
    void CreateUI()
    {
        interactUI = Instantiate(interactUIPrefab, interactUIParent);
        interactText = interactUI.GetComponentInChildren<TextMeshProUGUI>();
    }
    
    void RegisterWeapons()
    {
        weapons = new List<Weapon>(GetComponentsInChildren<Weapon>());
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
        // Offset weapon index with direction
        currentWeaponIndex += direction;
        // Check if index is bellow zero
        if (currentWeaponIndex < 0)
        {
            // Loop back to end
            currentWeaponIndex = weapons.Count - 1;
        }
        // Check if index is exceding length
        if(currentWeaponIndex >= weapons.Count)
        {
            // Reset back to zero
            currentWeaponIndex = 0;
        }
        SelectWeapon(currentWeaponIndex);
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
        // Call pickup on the weapon
        weaponToPickup.Pickup();
        // Get transform
        Transform weaponTransform = weaponToPickup.transform;
        // Attach weapon to hand
        weaponTransform.SetParent(hand);
        // Zero rotation and position
        weaponTransform.localRotation = Quaternion.identity;
        weaponTransform.localPosition = Vector3.zero;
        // Add to list
        weapons.Add(weaponToPickup);
        // Select new weapon
        SelectWeapon(weapons.Count - 1);
    }

    /// <summary>
    /// Removes weapon to list and removes from player's hand
    /// </summary>
    /// <param name="weaponToDrop"> Weapon to remove from hand </param>
    void Drop(Weapon weaponToDrop)
    {
        // Drop weapon
        weaponToDrop.Drop();
        // Get the transform 
        Transform weaponTransform = weaponToDrop.transform;
        weaponTransform.SetParent(null);
        // Remove weapon from list
        weapons.Remove(weaponToDrop);
        
    }

    /// <summary>
    /// Sets currentWeapon to weapon at given index
    /// </summary>
    /// <param name="index"> Weapon Index </param>
    void SelectWeapon(int index)
    {
        index = index % weapons.Count;
        // Is index in range
        if (index >= 0 && index < weapons.Count)
        {
            // Disable all weapons
            DisableAllWeapons();
            // Select weapon
            currentWeapon = weapons[index];
            // Enable the current weapon weapon (using index)
            currentWeaponIndex = index;
        }
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
        bool isGrounded = Physics.Raycast(groundRay, out hit, groundRayDistance);
        bool isJumping = Input.GetButtonDown("Jump");
        bool canJump = jumps < maxJumps; // jumps = int, maxJumps = int
        // Is grounded?
        if(isGrounded)
        {
            // If jump is pressed
            if(isJumping)
            {
                jumps = 1;
                // Move controller up
                movement.y = jumpHeight;
            }
        }
        else
        {
            if (isJumping && canJump)
            {
                movement.y = jumpHeight;
                jumps++;
            }
        }
        
        // Apply gravity 
        movement.y -= gravity * Time.deltaTime;
        // Limit the gravity
        movement.y = Mathf.Max(movement.y, -gravity);
        // Move the controller
        controller.Move(movement * Time.deltaTime);
    }
    /// <summary>
    /// Interaction with items in the world
    /// </summary>
    void Interact()
    {
        // Disable interact UI
        interactUI.SetActive(false);
        // Create ray from centre of screen
        Ray interactRay = attachedCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        RaycastHit hit;
        // Shoots a ray in a range
        if(Physics.Raycast(interactRay, out hit, interactRange))
        {
            // Try getting IInteractable object
            IInteractable interact = hit.collider.GetComponent<IInteractable>();
            if(interact != null)
            {
                // Enable the UI
                interactUI.SetActive(true);
                // Change the text to item's title
                interactText.text = interact.GetTitle();
                // Get input from user
                if(Input.GetKeyDown(KeyCode.E))
                {
                    Weapon weapon = hit.collider.GetComponent<Weapon>();
                    if (weapon)
                    {
                        
                        Pickup(weapon);
                    }
                }

                
            }
        }
    }
    /// <summary>
    /// Using the current weapon to fire a bullet
    /// </summary>
    void Shooting()
    {
        if(currentWeapon)
        {
            if(Input.GetButton("Fire1"))
            {
                // Shoot
                currentWeapon.Shoot();
            }
        }
    }
    /// <summary>
    /// Cycling through availale weapons
    /// </summary>
    void Switching()
    {
        // If there is more than one weapon
        if(weapons.Count > 1)
        {
            float inputScroll = Input.GetAxis("Mouse ScrollWheel");
            // If scroll input has been made
            if(inputScroll != 0)
            {
                int direction = inputScroll > 0 ? Mathf.CeilToInt(inputScroll) : Mathf.FloorToInt(inputScroll);
                // Switch weapons up or down
                SwitchWeapon(direction);
            }
        }
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

    public void Kill()
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(int damage)
    {
        throw new System.NotImplementedException();
    }
}
