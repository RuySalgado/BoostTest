using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControlls : MonoBehaviour
{
    public float MovementSpeed = 10;

    Vector3 direction;

    Rigidbody _cc;

    // Start is called before the first frame update
    void Start()
    {
        _cc = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        direction = new Vector3(x, 0, z);
    }

    void FixedUpdate()
    {
        if (direction.magnitude >= 0.1f)
        {
            if (direction.magnitude > 1f) direction.Normalize();
            
            _cc.AddForce(transform.rotation * direction * MovementSpeed);
        }
    }
}
