using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class hoverController : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private GameObject centerThruster;
    [SerializeField] private List<GameObject> terrainOrientationCasters;
    [SerializeField] private GameObject frontCounterForceObj;
    [SerializeField] private GameObject massCenter;
    [SerializeField] private GameObject propulsor;

    [SerializeField] private float propulsionForce = 10f;
    [SerializeField] private float boostForce = 10f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float hoverConstant = 10f;
    [SerializeField] private float thrusterDistance = 2f;
    [SerializeField] private float jumpForce = 9f;
    [SerializeField] private float gripForce = 5f;

    [SerializeField] float horizontalFriction = 30;

    public float speed = 10.0f;
    public float maxVelocityChange = 10.0f;

    private Vector3 gravity = Physics.gravity;

    private Vector2 _move;
    public bool isGrounded = false;
    public bool isGripping = false;
    public bool isBoosting = false;
    [HideInInspector]
    public Vector3 averageNormal = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody.centerOfMass = massCenter.transform.localPosition;
    }

    #region InputActions
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
        isGripping = !isGripping;
    }

    public void OnJump()
    {
        if (isGrounded)
        {
            _rigidbody.AddForce(transform.up * jumpForce * _rigidbody.mass, ForceMode.Impulse);

        }
    } 

    public void OnBoost(InputValue input)
    {
        isBoosting = input.Get<float>() > Mathf.Epsilon;
    }
    #endregion

    private void FixedUpdate()
    {
        CheckAverageNormal();
        CheckGrounded();
        HandleMovement();
        ApplyBoost();
        ApplyHoverForce();
        ApplyGravity();
        ApplyHorizotalFriction();
        Debug.Log(_rigidbody.velocity.magnitude);

    }

    private void ApplyBoost()
    {
        if (isBoosting)
        {
            _rigidbody.AddForce(transform.TransformDirection(Vector3.forward) * 0.01f * boostForce, ForceMode.VelocityChange);
        }
    }

    private void ApplyHorizotalFriction()
    {
        _rigidbody.AddForce(transform.TransformDirection(Vector3.right)  * -transform.InverseTransformVector(_rigidbody.velocity).x * horizontalFriction);
    }

    private void ApplyGravity()
    {
        Vector3 _gravity = isGrounded && isGripping ? transform.TransformDirection(gravity * 1.2f) : gravity;
        _rigidbody.AddForce(_gravity * _rigidbody.mass);
    }

    private void HandleMovement()
    {

        if (Mathf.Abs(_move.y) > Mathf.Epsilon)
        {
            if (isGrounded && _move.y > Mathf.Epsilon)
            {
                Vector3 floorNormal = averageNormal;
                Vector3 propulsion = new Vector3(0, 0, _move.y);
                propulsion *= propulsionForce;
                propulsion = transform.TransformDirection(propulsion);
                propulsion = Vector3.ProjectOnPlane(propulsion, floorNormal);
                Debug.DrawRay(transform.position, propulsion, Color.red);
                _rigidbody.AddForceAtPosition(propulsion * 0.01f, propulsor.transform.position, ForceMode.VelocityChange);
            }
            else
            {
                Vector3 targetVelocity = transform.right * _move.y * 5;
                Vector3 VelocityChange = targetVelocity - _rigidbody.angularVelocity;
                _rigidbody.AddTorque(VelocityChange, ForceMode.VelocityChange);
            }
        }
        if (Mathf.Abs(_move.x) > Mathf.Epsilon)
        {
            float _turnSpeed = isGrounded ? turnSpeed : 5;
            Vector3 targetVelocity = transform.up * _move.x * _turnSpeed;
            Vector3 VelocityChange = targetVelocity - _rigidbody.angularVelocity;
            _rigidbody.AddTorque(VelocityChange, ForceMode.VelocityChange);
        }

    }

    private void ApplyHoverForce()
    {
        RaycastHit hit;
        if (Physics.Raycast(centerThruster.transform.position, transform.TransformDirection(Vector3.down), out hit, thrusterDistance, 1 << 8))
        {
            float inpulse = (1 - hit.distance / thrusterDistance);
            inpulse = Mathf.Pow(inpulse, 2);

            Vector3 force = averageNormal * inpulse * hoverConstant;
            force += Physics.gravity * 1.9f;
            _rigidbody.AddForceAtPosition(force, centerThruster.transform.position);
            //if (_move.y > Mathf.Epsilon && isGrounded)
            //{
            //    _rigidbody.AddForceAtPosition(force * 0.2f, frontCounterForceObj.transform.position);

            //}
            //Debug.DrawRay(centerThruster.transform.position, force, Color.red);
            Debug.DrawRay(centerThruster.transform.position, _rigidbody.velocity, Color.red);

        }

        // TODO: Rotate with torque (maybe)
        if (isGrounded)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation, 15 * Time.fixedDeltaTime);
        }
    }

    private void CheckAverageNormal()
    {
        List<Vector3> hits = new List<Vector3>();
        foreach (GameObject caster in terrainOrientationCasters)
        {
            RaycastHit hit;
            if (Physics.Raycast(caster.transform.position, transform.TransformDirection(Vector3.down), out hit, thrusterDistance * 1.6f, 1 << 8))
            {
                Debug.DrawRay(caster.transform.position, transform.TransformDirection(Vector3.down) * thrusterDistance * 1.6f, Color.red);

                hits.Add(hit.normal);
            }
        }
        Vector3 average = averageNormal;
        for (int i = 0; i < hits.Count; i++)
        {
            average += hits[i];
        }
        if (hits.Count != 0)
        {
            average = average / hits.Count;
        }
        averageNormal = average;

    }

    private void CheckGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(centerThruster.transform.position, transform.TransformDirection(Vector3.down), out hit, thrusterDistance * 1.3f, 1 << 8))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
}
