using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;


public class MyStatusManager : ModelTargetBehaviour
{
    public Text mytext;
    // Start is called before the first frame update
    void Start()
    {
        GameObject obj=GameObject.Find("Text");
        Debug.Log(obj.name);
        mytext=obj.GetComponent<Text>();
    }
    
    public override void OnTrackerUpdate (Status newStatus, StatusInfo newStatusInfo){
        mytext.text = "status: "+newStatus;
        Debug.Log("status: "+newStatus);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
