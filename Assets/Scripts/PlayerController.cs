using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour
{
    #region fields
    /// <summary>
    /// If the player's goal is the left one
    /// </summary>
    [SerializeField] 
    private bool isLeft = false;

    /// <summary>
    /// The force that will be applied to the player on the Y-axis upon jumping.
    /// <para>Note: The mass of the player is ignored.</para>
    /// </summary>
    [SerializeField] 
    private float jumpForce = 7f;

    /// <summary>
    /// The movement speed of the player on the X-Axis in units per second
    /// </summary>
    [SerializeField] 
    private float moveSpeed = 4f;

    /// <summary>
    /// <see cref="moveSpeed"/> has to be multiplied by this number in order to convert it into units/s
    /// </summary>
    private const float MoveSpeedToUnitsPerSecondConversionRate = 50f;

    /// <summary>
    /// How fast the player's boot moves when it is kicking
    /// <para></para>!FIGURE OUT WHAT 750 MEANS!
    /// </summary>
    [SerializeField] 
    private float kickSpeed = 750f;
    

    /// <summary>
    /// If the player is currently touching the ground.
    /// </summary>
    private bool _isGrounded = false;

    /// <summary>
    /// Stores the reference to the Rigidbody2D component of the player
    /// </summary>
    private Rigidbody2D _rigidBody;

    /// <summary>
    /// The reference to the class that provides events and methods that
    /// this class uses in order to handle input
    /// </summary>
    private InputActions _inputActions;

    /// <summary>
    /// The BootPivot empty game object that is used to rotate the boot around the player
    /// </summary>
    private Transform _bootPivot;



    private bool _isOnNetwork;
    #endregion

    #region properties
    public bool IsLeft
    {
        get => isLeft;
        set => isLeft = value;
    }
    
    

    /// <summary>
    /// Returns true if the player is currently holding down the Kick button
    /// </summary>
    public bool IsKicking
    {
        get => (IsLeft && _inputActions.MovementLeft.Kick.IsInProgress()) ||
               !IsLeft && _inputActions.MovementRight.Kick.IsInProgress();
    }
    #endregion

    #region methods
    //Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _inputActions = new InputActions();
        _bootPivot = transform.GetChild(0);
        
        //Enabling input listening. Either for WASD or Arrow keys depending on isLeft
        if (IsLeft)
        {
            _inputActions.MovementLeft.Enable();
            _inputActions.MovementLeft.Jump.performed += Jump;
        }
        else
        {
            _inputActions.MovementRight.Enable();
            _inputActions.MovementRight.Jump.performed += Jump;
        }
        _isOnNetwork = NetworkManager.Singleton != null;
    }
    
    //Physics need to be done in FixedUpdate instead of Update
    private void FixedUpdate()
    {
        /*Since this script will be run on all player objects by every client,
         we need to check if the client is the owner to make sure that does not happen.*/
        if (!IsOwner && _isOnNetwork) return;
        
        HandleMovement();
    }
    
    //Update is called every frame
    private void Update()
    {
        /*Since this script will be run on all player objects by every client,
         we need to check if the client is the owner to make sure that does not happen.*/
        if (!IsOwner && _isOnNetwork) return;

        HandleKicking();
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        //The player needs to be on the ground in order to jump
        /*Since this script will be run on all player objects when a client 
        presses the jump key, we need to check if the client is the owner
        of this player object so that only that player jumps.*/
        if (_isGrounded && (IsOwner || !_isOnNetwork))
        {
            //Makes the player jump. Ignores the mass.
            _rigidBody.AddForce(new Vector2(0, jumpForce * _rigidBody.mass), ForceMode2D.Impulse);
            _isGrounded = false; //The player jumped and is therefore no longer on the ground
        }
    }
    
    //Called when the player collides with something
    private void OnCollisionEnter2D(Collision2D collision)
    {
        int groundLayer = 3;
        if (collision.gameObject.layer == groundLayer)
        {
            //The player collided with the ground
            _isGrounded = true;
        }
    }

    /// <summary>
    /// Reads player input and applies movement to the player accordingly.
    /// <para>Movement is applied by setting the velocity of the player's Rigidbody2D</para>
    /// </summary>
    private void HandleMovement()
    {
        float input;
        //Reading the input
        if (_inputActions.MovementLeft.enabled)
        {
            input = _inputActions.MovementLeft.Move.ReadValue<float>();
        }
        else
        {
            input = _inputActions.MovementRight.Move.ReadValue<float>();
        }

        //Calculating and applying the velocity on the x axis
        float velocity = input * Time.deltaTime * moveSpeed * MoveSpeedToUnitsPerSecondConversionRate;
        _rigidBody.velocity = new Vector2(velocity, _rigidBody.velocity.y);
    }

    private void HandleKicking()
    {
        //If the player is holding down the kick button, and the boot has not reached
        //the max rotation, it is rotated by a certain amount in the counter-clockwise (positive) direction
        if (IsKicking)
        {
            if (_bootPivot.localRotation.eulerAngles.z < 90) //max rotation for the boot is 90°
            {
                _bootPivot.Rotate(0, 0, Time.deltaTime * kickSpeed);
            }
        }
        //If the player is not holding down the kick button anymore, the boot should
        //start going back to its initial position (at 0°)
        else if (_bootPivot.localRotation.eulerAngles.z > 0)
        {
            RetractBoot();
        }
    }

    private void RetractBoot()
    {
        /*As the boot is rotated back to 0°, it will not stop at exactly 0°, but
             rather at a number that's slightly smaller than 0. This is because, every frame, a
             certain number is subtracted from the Z-Angle. To make sure the boot is at exactly
             0° when the player is not kicking, we check if it went beyond 0° and if it did, we
             calculate by how much and rotate it by that amount*/

        //>180° means the boot is in the left half of the circle (left of 0°)
        if (_bootPivot.localRotation.eulerAngles.z > 180)
        {
            //360 - boot's rotation gives us the amount we need to rotate the boot by so
            //it makes a full circle and is back at 0°
            _bootPivot.Rotate(0, 0, 360 - _bootPivot.localRotation.eulerAngles.z);
        }
        //If the boot hasn't returned to its initial position, it is rotated by a certain
        //amount in the clockwise (negative) direction
        else
        {
            _bootPivot.Rotate(0, 0, -Time.deltaTime * kickSpeed);
        }
    }
    #endregion
}