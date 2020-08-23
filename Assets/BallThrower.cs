using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class BallThrower : MonoBehaviour
{
    public GameObject prefab;
    public Transform modelTarget;
    public float force = 10;
    public Text label;
    void Start()
    {
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        
        
    }
    private void Update ()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            ThrowBall();
            label.gameObject.SetActive(true);
        }
        
    }
        
    void ThrowBall()
    {
        GameObject obj = Instantiate(prefab, transform.position, transform.rotation);
        obj.transform.SetParent(modelTarget, true);
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * force, ForceMode.Impulse);
        Physics.gravity = -10 * modelTarget.transform.up;
    }
}
