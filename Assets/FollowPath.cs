using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPath : MonoBehaviour
{
    public Transform path;
    public float speed = 0.5f;
    int idx = 0;
   
     
    void Update()
    {
        var targetPos = path.GetChild(idx).position;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        var dist = Vector3.Distance(transform.position, targetPos);
        if (dist < 0.5f)
        {
            idx = (idx + 1) % path.childCount; //next point
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(-(targetPos - transform.position).normalized, transform.parent.up), 0.1f);
        //transform.forward = Vector3.LerpUnclamped(transform.forward, , 0.1f);
        //transform.up = transform.parent.up;
    }
}
