using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputProcessing : MonoBehaviour
{

    [HideInInspector]
    public float X;
    [HideInInspector]
    public float Z;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        X = Input.GetAxis("Horizontal");
        Z = Input.GetAxis("Vertical");
    }

    public Vector3 GetDirection()
    {
        return new Vector3(X, 0, Z);
    }
}
