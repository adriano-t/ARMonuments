using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

[RequireComponent(typeof(ModelTargetBehaviour))]
public class MonumentStatusManager : MonoBehaviour
{
    public Text statusLabel;
    public Text statusInfoLabel;

    void Start ()
    {
        var mt = GetComponent<ModelTargetBehaviour>();
        mt.RegisterOnTrackableStatusChanged(OnStatusChange);
        mt.RegisterOnTrackableStatusInfoChanged(OnStatusInfoChange);
    }

    private void OnStatusChange (TrackableBehaviour.StatusChangeResult result)
    {
        statusLabel.text = "Status: " + result.NewStatus;
    }
    private void OnStatusInfoChange (TrackableBehaviour.StatusInfoChangeResult result)
    {
        statusInfoLabel.text = "Info: " + result.NewStatusInfo;
    }
}
