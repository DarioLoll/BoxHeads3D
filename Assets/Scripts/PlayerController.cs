
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    #region fields
    [SerializeField]
    private bool isLeft = false;

    [SerializeField]
    private float jumpForce = 7f;

    [SerializeField]
    private float moveSpeed = 200f;

    [SerializeField]
    private float kickStrength = 750f;

    bool isGrounded = false;

    private Rigidbody2D rigidBody;

    private InputActions inputActions;
    #endregion

    #region properties
    public bool IsLeft
    {
        get 
        { 
            return isLeft; 
        }
        set 
        { 
            isLeft = value; 
        }
    }

    public bool IsKicking
    {
        get => (IsLeft && inputActions.MovementLeft.Kick.IsInProgress()) ||
                !IsLeft && inputActions.MovementRight.Kick.IsInProgress();
    }
    #endregion

    #region methods
    //Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        inputActions = new InputActions();
        if (IsLeft)
        {
            inputActions.MovementLeft.Enable();
            inputActions.MovementLeft.Jump.performed += Jump;
        }
        else
        {
            inputActions.MovementRight.Enable();
            inputActions.MovementRight.Jump.performed += Jump;
        }

    }

    private void Jump(InputAction.CallbackContext obj)
    {
        if (isGrounded)
        {
            rigidBody.AddForce(new Vector2(0, jumpForce * rigidBody.mass ), ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        float input;
        if (inputActions.MovementLeft.enabled)
        {
            input = inputActions.MovementLeft.Move.ReadValue<float>();
        }
        else
        {
            input = inputActions.MovementRight.Move.ReadValue<float>();
        }

        rigidBody.velocity = new Vector2(input * moveSpeed * Time.deltaTime, rigidBody.velocity.y);
    }

    private void Update()
    {
        Transform boot = transform.GetChild(0);
        if ((inputActions.MovementLeft.Kick.inProgress && IsLeft) || (inputActions.MovementRight.Kick.inProgress && !IsLeft))
        {
            if (boot.localRotation.eulerAngles.z < 90)
            {
                boot.Rotate(0, 0, Time.deltaTime * kickStrength);
            }
        }
        else if (boot.localRotation.eulerAngles.z > 0)
        {
            if (boot.localRotation.eulerAngles.z > 180)
            {
                boot.Rotate(0, 0, 360 - boot.localRotation.eulerAngles.z);
            }
            else
            {
                boot.Rotate(0, 0, -Time.deltaTime * kickStrength);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3 /*Ground*/)
        {
            isGrounded = true;
        }
    }
    #endregion
}
