using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class hoverController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private GameObject centerThruster;
    [SerializeField] private List<GameObject> terrainOrientationCasters;
    [SerializeField] private GameObject massCenter;
    [SerializeField] private GameObject propulsor;

    [SerializeField] private float propulsionForce = 10f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float hoverConstant = 10f;
    [SerializeField] private float thrusterDistance = 2f;
    [SerializeField] private float jumpForce = 9f;

    [SerializeField] float horizontalFriction = 30;

    public float speed = 10.0f;
    public float maxVelocityChange = 10.0f;

    private Vector2 _move;
    public bool isGrounded = false;
    public bool isGripping = false;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody.centerOfMass = massCenter.transform.localPosition;
    }

    public void OnMove(InputValue input)
    {
        _move = input.Get<Vector2>();
    }

    public void OnReset()
    {
        transform.position = new Vector3(35.4700012f, 6.36000013f, 44.3133087f);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
    public void OnGrip(InputValue input)
    {
        isGripping = input.Get<float>() > 0 ? true : false;
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            _rigidbody.AddForce(transform.up * jumpForce * _rigidbody.mass, ForceMode.Impulse);

        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        ApplyHoverForce();
        ApplyGripForce();

        _rigidbody.AddForce(transform.TransformDirection(Vector3.right) * -transform.InverseTransformVector(_rigidbody.velocity).x * horizontalFriction);
    }

    private void HandleMovement()
    {

        if (Mathf.Abs(_move.y) > Mathf.Epsilon)
        {
            if (isGrounded && _move.y > Mathf.Epsilon)
            {
                Vector3 targetVelocity = new Vector3(0, 0, _move.y);
                targetVelocity *= propulsionForce;
                targetVelocity = transform.TransformDirection(targetVelocity);
                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = _rigidbody.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = isGripping ? Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange) : 0;
                _rigidbody.AddForceAtPosition(velocityChange, propulsor.transform.position, ForceMode.VelocityChange);
            }
            else
            {
                Vector3 targetVelocity = transform.right * _move.y * 3;
                Vector3 VelocityChange = targetVelocity - _rigidbody.angularVelocity;
                _rigidbody.AddTorque(VelocityChange, ForceMode.VelocityChange);
            }
        }
        if (Mathf.Abs(_move.x) > Mathf.Epsilon)
        {
            float _turnSpeed = isGrounded ? turnSpeed : 3;
            Vector3 targetVelocity = transform.up * _move.x * _turnSpeed;
            Vector3 VelocityChange = targetVelocity - _rigidbody.angularVelocity;
            _rigidbody.AddTorque(VelocityChange, ForceMode.VelocityChange);
        }

    }

    private void ApplyHoverForce()
    {
        RaycastHit hit;
        //Debug.DrawRay(thruster.transform.position, (-transform.up * thrusterDistance), Color.red);
        if (Physics.Raycast(centerThruster.transform.position, transform.TransformDirection(Vector3.down), out hit, thrusterDistance, 1 << 8))
        {
            float inpulse = (1 - hit.distance / thrusterDistance);
            inpulse = Mathf.Pow(inpulse, 2);

            Vector3 force = transform.TransformDirection(Vector3.up) * inpulse * hoverConstant;
            force += Vector3.down * _rigidbody.mass;
            _rigidbody.AddForceAtPosition(force, centerThruster.transform.position);
        }
        else
        {
            isGrounded = false;
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(transform.up, GetAverageNormal()) * transform.rotation, 3.7f * Time.fixedDeltaTime);

    }

    private Vector3 GetAverageNormal()
    {
        List<Vector3> hits = new List<Vector3>();
        foreach (GameObject caster in terrainOrientationCasters)
        {
            RaycastHit hit;

            if (Physics.Raycast(caster.transform.position, transform.TransformDirection(Vector3.down), out hit, thrusterDistance * 1.6f, 1 << 8))
            {
                hits.Add(hit.normal);
            }
        }
        Vector3 average = new Vector3(0, 0, 0);
        for (int i = 0; i < hits.Count; i++)
        {
            average += hits[i];
        }
        if (!(hits.Count == 0))
        {
            average = average / hits.Count;
        }
        // WARNING: breaking single risponsibility
        if (hits.Count >= 3)
        {
            isGrounded = true;
        }
        return average;

    }

    private void ApplyGripForce()
    {
        if (isGripping)
        {
            Debug.DrawRay(transform.position, -transform.up * 12 * _rigidbody.mass);
            _rigidbody.AddForce(-transform.up * 12 * _rigidbody.mass);
        }
    }
}
