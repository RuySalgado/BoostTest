using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraSwitcher : MonoBehaviour
{
    hoverController controller;

    [SerializeField] CinemachineFreeLook freeLook;
    [SerializeField] CinemachineFreeLook freeLookLocked;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<hoverController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isGripping && controller.isGrounded && !freeLookLocked.isActiveAndEnabled)
        {
            freeLookLocked.gameObject.SetActive(true);
            freeLook.gameObject.SetActive(false);
            Debug.Log("enabling");
        }
        if (!controller.isGripping || !controller.isGrounded && !freeLook.isActiveAndEnabled)
        {
            freeLook.gameObject.SetActive(true);
            freeLookLocked.gameObject.SetActive(false);
        }
    }
}
