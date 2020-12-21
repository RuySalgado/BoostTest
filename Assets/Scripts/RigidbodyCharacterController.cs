using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


public class RigidbodyCharacterController: MonoBehaviour
{

	public float speed = 10.0f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public bool isGrounded = true;
	public float jumpHeight = 2.0f;
	Rigidbody _rigidbody = null;
	[SerializeField]
	GameObject centerGroundCaster = null;
    [SerializeField]
    GameObject frontGroundCaster = null;
    [SerializeField]
    GameObject backGroundCaster = null;
    [SerializeField]
	GameObject followTarget = null;
    [SerializeField]
    float xRotationSpeed = 1f;


    public float sensitivityX = 3F;
	public float sensitivityY = 3F;
	[SerializeField]
	float rotationSpeed = 1F;
    [SerializeField]
    float aligmentSpeed = 1F;
    Quaternion targetNormalAlignment;

	// input states
	float _forward = 0f;
	Vector2 _turn;
	Vector2 _look;
    public bool grip = false;

	void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
		_rigidbody = GetComponent<Rigidbody>();
		_rigidbody.freezeRotation = true;
		_rigidbody.useGravity = false;
	}

    #region InputHandlers

    public void OnTurn(InputValue inputValue)
    {
        _turn = inputValue.Get<Vector2>();

    }

    private void HandleTurn()
    {
        transform.rotation *= Quaternion.AngleAxis(_turn.x * rotationSpeed, Vector3.up);
    }



    private void HandleForward()
    {
        if (isGrounded)
        {
            Vector3 targetVelocity = new Vector3(0, 0, _turn.y);
            if (targetVelocity.magnitude > Mathf.Epsilon)
            {
                targetVelocity = transform.TransformDirection(targetVelocity);
                targetVelocity *= speed;
                Debug.DrawRay(transform.position, targetVelocity);
                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = _rigidbody.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = grip ? Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange) : 0;
                _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        } else
        {
            transform.rotation *= Quaternion.AngleAxis(_turn.y * xRotationSpeed, Vector3.right);
        }
        
    }

    public void OnLook(InputValue inputValue)
    {
        _look = inputValue.Get<Vector2>();
    }

    private void HandleLook()
    {
        followTarget.transform.rotation *= Quaternion.AngleAxis(_look.x * sensitivityX, Vector3.up);
        followTarget.transform.rotation *= Quaternion.AngleAxis(-_look.y * sensitivityY, Vector3.right);

        var angles = followTarget.transform.localEulerAngles;
        angles.z = 0;

        var angle = followTarget.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if (angle < 180 && angle > 40)
        {
            angles.x = 40;
        }


        followTarget.transform.localEulerAngles = angles;
    }

    // Gets called twice per button press but the second one will do nothing since the player will now be not grounded. 
    public void OnJump() {
        if (isGrounded)
        {
            _rigidbody.velocity += transform.TransformDirection(new Vector3(0, CalculateJumpVerticalSpeed(), 0));

        }	
	}

    private void OnFire()
    {
        grip = !grip;
    }

	#endregion

	private void Update()
    {
        HandleLook();
        CheckGrounded();

	}

    void FixedUpdate()
	{
		HandleForward();
		HandleTurn();
        RotateToGroundNormal();
        Debug.Log(_rigidbody.velocity);
        if (!isGrounded)
        {
            
        }




        // We apply gravity manually for more tuning control
        _rigidbody.AddForce(new Vector3(0, -gravity * _rigidbody.mass, 0));
        if (grip)
        {
            _rigidbody.AddForce(transform.TransformDirection(new Vector3(0, -30 * _rigidbody.mass, 0)));

        }

	}

	private void CheckGrounded()
	{
        isGrounded = Physics.Raycast(frontGroundCaster.transform.position, transform.TransformDirection(Vector3.down), 0.3f, 1 << 8) &&
        Physics.Raycast(backGroundCaster.transform.position, transform.TransformDirection(Vector3.down), 0.3f, 1 << 8);
	}


	float CalculateJumpVerticalSpeed()
	{
		// From the jump height and gravity we deduce the upwards speed 
		// for the character to reach at the apex.
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}

    private void RotateToGroundNormal()
    {
        RaycastHit frontHit;
        RaycastHit backHit;
        bool frontCast = Physics.Raycast(new Ray(frontGroundCaster.transform.position, transform.TransformDirection(Vector3.down)), out frontHit, 0.5f, 1 << 8);
        bool backCast = Physics.Raycast(new Ray(backGroundCaster.transform.position, transform.TransformDirection(Vector3.down)), out backHit, 0.5f, 1 << 8);
        if (frontCast && backCast)
        {
            Vector3 compositeNormal = frontHit.normal + backHit.normal;
            Debug.DrawRay(centerGroundCaster.transform.position, compositeNormal * 0.3f, Color.red);
            Debug.DrawRay(frontGroundCaster.transform.position, transform.TransformDirection(Vector3.down) * 0.5f, Color.red);
            Debug.DrawRay(backGroundCaster.transform.position, transform.TransformDirection(Vector3.down) * 0.5f, Color.red);
            targetNormalAlignment = Quaternion.FromToRotation(transform.up, compositeNormal) * transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetNormalAlignment, aligmentSpeed * Vector3.Angle(transform.TransformDirection(Vector3.up), compositeNormal)/25);
        }

    }


}