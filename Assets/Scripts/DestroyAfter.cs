using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    void Update()
    {
        if (Vector3.Distance(transform.position, Camera.main.transform.position) > 100)
            Destroy(gameObject);
    }
}
