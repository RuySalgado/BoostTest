using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeLookCameraControlls : MonoBehaviour
{

    [SerializeField] GameObject followTarget;
    [SerializeField] float xSensitivity;
    [SerializeField] float ySensitivity;

    Vector2 _look;

    public void OnLook(InputValue input)
    {
        _look = input.Get<Vector2>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        
        followTarget.transform.rotation *= Quaternion.Lerp(followTarget.transform.rotation, Quaternion.AngleAxis(_look.x * Time.deltaTime * xSensitivity, Vector3.up), 1);
        followTarget.transform.rotation *= Quaternion.Lerp(followTarget.transform.rotation, Quaternion.AngleAxis(-_look.y * Time.deltaTime * ySensitivity, Vector3.right), 1);
        //followTarget.transform.rotation *= Quaternion.AngleAxis(-_look.y * ySensitivity * Time.deltaTime, Vector3.right);

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
}
