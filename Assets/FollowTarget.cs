﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField]
    GameObject followTarget;

    [SerializeField]
    float xOffset = 0;
    [SerializeField]
    float yOffset = 0;
    [SerializeField]
    float zOffset = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = followTarget.transform.position + new Vector3(xOffset, yOffset, zOffset);
    }
}
