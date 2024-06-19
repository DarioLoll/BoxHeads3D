using System;
using System.Collections;
using System.Collections.Generic;
using Collectables;
using Managers;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    private InputActions _inputActions;
    /// <summary>
    /// The character controller component attached to the player
    /// </summary>
    private CharacterController _characterController;
    

    /// <summary>
    /// <inheritdoc cref="PlayerRotator"/>
    /// </summary>
    private PlayerRotator _playerRotator;
    
    private GamePlayer _gamePlayer;
    

    [SerializeField] private Animator animator;

    /// <summary>
    /// The movement speed of the player
    /// </summary>
    [SerializeField]
    private float walkingSpeed = 5f;

    /// <summary>
    /// The movement speed of the player when he's sprinting
    /// </summary>
    [SerializeField]
    private float sprintingSpeed = 10f;

    private static readonly int SpeedPercentage = Animator.StringToHash("SpeedPercentage");
    
    private static readonly int SwingTrigger = Animator.StringToHash("Swing");
    
    private const float SwingDuration = 0.5f;
    private float _swingTimer = SwingDuration;

    public bool IsWalking { get; private set; }
    public bool IsSprinting => _inputActions.OnFoot.Sprint.IsPressed();

    public event Action Drink;

    public float Speed
    {
        get
        {
            if (!IsSprinting) 
                return walkingSpeed;
            float thirstLevel = _gamePlayer.ThirstLevel;
            float hungerLevel = _gamePlayer.HungerLevel;
            if (thirstLevel >= GamePlayer.NoSprintingOnLevel || hungerLevel >= GamePlayer.NoSprintingOnLevel)
                return walkingSpeed;
            return sprintingSpeed;
        }
    }

    /// <summary>
    /// The velocity that is being added to the player's y velocity every frame (for gravity)
    /// </summary>
    private const float G = -9.81f;
    

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        Cursor.lockState = CursorLockMode.Locked;
        _inputActions = new InputActions();
        _inputActions.OnFoot.Enable();
        _playerRotator = GetComponent<PlayerRotator>();
        _gamePlayer = GetComponent<GamePlayer>();
        _characterController = GetComponent<CharacterController>();
        _playerRotator.Camera = Camera.main;
        CameraFollower cameraFollower = Camera.main!.GetComponent<CameraFollower>();
        cameraFollower.FollowObject = transform;
    }

    private void OnCollectableReached(ICollectable collectable)
    {
        var handItem = _gamePlayer.Inventory.HandSlot.Item;
        collectable.OnHit(handItem != null ? handItem.Value.Name : "empty");
    }


    // Update is called once per frame
    void Update()
    {
        if(!IsOwner) return;
        _swingTimer -= Time.deltaTime;
        if(_swingTimer < 0) _swingTimer = 0;
        if(_gamePlayer.IsInventoryOpen) return;
        Move(_inputActions.OnFoot.Movement.ReadValue<Vector2>());
        CheckForCollectable();
        _playerRotator.Look(_inputActions.OnFoot.Look.ReadValue<Vector2>());
        CheckForInteractable();
        Swing();
    }
    
    private void CheckForInteractable()
    {
        if (Physics.Raycast(_playerRotator.Camera.transform.position, _playerRotator.Camera.transform.forward,
                out var hit, 3f)
            && hit.transform.gameObject.name == "Terrain Chunk"
            && hit.point.y <= TerrainGenerator.WaterHeight)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                OnDrink();
            }
            HudManager.Instance.ShowInteractable("Drink", HudManager.Instance.WaterIcon);
        }
        else
        {
            HudManager.Instance.HideInteractable();
        }
    }

    private void Swing()
    {
        if(_swingTimer > 0) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            animator.SetTrigger(SwingTrigger);
            if(!Physics.Raycast(_playerRotator.Camera.transform.position, _playerRotator.Camera.transform.forward, 
                   out var hit, 2f)) return;
            ICollectable collectable;
            if (hit.transform.parent == null)
            {
                if(!hit.transform.TryGetComponent(out collectable)) 
                    return;
            }
            else if (!hit.transform.parent.TryGetComponent(out collectable)) return;
            OnCollectableReached(collectable);
            _swingTimer = SwingDuration;
        }
    }

    /// <summary>
    /// Handles the WASD input and gravitational pull for the player making him able to move around
    /// </summary>
    /// <param name="input">A normalized vector (both x and y must be between -1 and 1)</param>
    private void Move(Vector2 input)
    {
        IsWalking = input.sqrMagnitude > 0.001f;
        //The player is only able to sprint in the forward direction
        if (input.y < 0)
            input.y *= walkingSpeed;
        else input.y *= Speed;
        var speedPercentage = Math.Abs(input.y) / sprintingSpeed + Math.Abs(input.x) / walkingSpeed;
        speedPercentage = Mathf.Clamp(speedPercentage, 0, 1);
        animator.SetFloat(SpeedPercentage, speedPercentage);
        Vector3 motion = new Vector3(input.x * walkingSpeed, 0, input.y);
        motion.y += G; //adding gravity
        motion *= Time.deltaTime; //making the movement framerate independent

        //The move method requires a direction to move the character in
        //In unity, there is local space and world space
        //If I want to move the player 3m forward, then that would normally mean
        //+3 on the Z axis relative to the player (local space)
        //but since the player is able to look around, the forward direction is not always the same
        //So, for example, if the player is rotated 90 degrees on the y axis, then
        //moving forward would mean moving on the x axis in world space and not the z axis
        //By transforming the direction from local to world space, we can make sure that
        //the player always moves in the direction he's facing
        //The TransformDirection method transforms a direction from local to world space
        //and accounts for the orientation of the transform
        _characterController.Move(transform.TransformDirection(motion));
    }
    
    private void CheckForCollectable()
    {
        if (Physics.CapsuleCast(transform.position, transform.position + Vector3.up, 1.5f, transform.forward, out var hit, 2f))
        {
            if(hit.transform.TryGetComponent(typeof(ItemVm), out var itemVm))
                _gamePlayer.OnItemPickedUpServerRpc(hit.transform.GetComponent<NetworkObject>());
        }
    }

    protected virtual void OnDrink()
    {
        Drink?.Invoke();
    }
}
