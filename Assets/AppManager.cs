using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.UI;
using Vuforia;

public class AppManager : MonoBehaviour
{
    public Text statusLabel;
    public Text statusInfoLabel;
    public Text debugLabel;
    public Text monumentLabel;
    public Text foundLabel;
    public Monument[] monuments;
    public float range = 0.001f;
    private string currentDataset;
    private List<TrackableBehaviour> toDestroy = new List<TrackableBehaviour>();
    private List<ModelTargetBehaviour> modelTargets = new List<ModelTargetBehaviour>();
    //public Text label;
    //public GameObject prefab;
    //public Transform modelTarget;
    //public float force = 10;
    void Start ()
    {
        foundLabel.gameObject.SetActive(false);
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

#if PLATFORM_ANDROID
        bool wait = false;
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
            wait = true;
        }

        //if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
        //{
        //    Permission.RequestUserPermission(Permission.CoarseLocation);
        //    wait = true;
        //}


        StartCoroutine(StartLocationService(wait));
#endif

    }


    private void Update ()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            //ThrowBall();
            //label.gameObject.SetActive(true);
        }
        
    }

    IEnumerator StartLocationService (bool wait)
    {
        if (wait)
        {
            debugLabel.text += "Waiting for authorization\n";
            yield return new WaitForSeconds(5.0f);
        }

        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("LocationService Not enabled");
            debugLabel.text += "LocationService Not enabled\n";
            //yield break;
        }
        else
        {
            Debug.Log("LocationService is enabled!");
            debugLabel.text += "LocationService is enabled!\n";
        }

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes

        float maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            maxWait -= Time.deltaTime;
            yield return null;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait <= 0)
        {
            Debug.LogError("Timed out");
            debugLabel.text += "status: " + Input.location.status + '\n';
            debugLabel.text += "Timed out\n";

            debugLabel.text += "Restarting location service\n";
            Input.location.Stop();
            yield return new WaitForSeconds(3.0f);
            Input.location.Start();
        }


        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            debugLabel.text += "Unable to determine device location\n";
            yield break;
        }
        else
        {
            while (true)
            {
                // Access granted and location value could be retrieved
                Debug.Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
                debugLabel.text = "Latitude: " + Input.location.lastData.latitude + "\n";
                debugLabel.text += "Longitude: " + Input.location.lastData.longitude + "\n";

                var loc = new Vector2(Input.location.lastData.latitude, Input.location.lastData.longitude);
                bool found = false;
                foreach (var monument in monuments)
                {
                    if (monument.name == "Chiesa Santa Maria, Castelnuovo")
                        continue;

                    var dist = Vector2.Distance(loc, monument.position);
                    if (dist < range)
                    {
                        monumentLabel.text = "Monumento: " + monument.name + " (" + dist + "°)";
                        if (currentDataset != monument.dataset)
                        {
                            if (LoadAndActivateDataset(monument))
                            {

                                currentDataset = monument.dataset;
                                found = true;
                                break;
                            }
                            else
                                debugLabel.text += "Impossible to load dataset: " + monument.dataset + "\n";
                        }
                        else
                        {
                            found = true;
                            break;
                        }
                    }
                    else
                    {
                        debugLabel.text +=  monument.name + " too far.\n";
                    }
                }

                if(!found)
                    monumentLabel.text = "Nessun monumento nei dintorni";

                yield return new WaitForSeconds(5.0f);
            }
        }

        // Stop service if there is no need to query location updates continuously
        Input.location.Stop();
    }
    //void ThrowBall()
    //{
    //    GameObject obj = Instantiate(prefab, transform.position, transform.rotation);
    //    obj.transform.SetParent(modelTarget, true);
    //    Rigidbody rb = obj.GetComponent<Rigidbody>();
    //    rb.AddForce(transform.forward * force, ForceMode.Impulse);
    //    Physics.gravity = -10 * modelTarget.transform.up;
    //}


    public bool LoadAndActivateDataset (Monument monument)
    {
        //destroy the previous model targets
        foreach (var t in modelTargets)
            Destroy(t.gameObject);
        modelTargets.Clear();


        var datasetName = monument.dataset;
        ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        Debug.LogWarning("Loading Dataset: " + datasetName);
        //Create a new dataset object.
        DataSet dataset = objectTracker.CreateDataSet();
        //Load and activate the dataset if it exists.
        if (DataSet.Exists(datasetName))
        {
            if (!dataset.Load(datasetName))
            {            
                debugLabel.text += "Load fail\n";
                Debug.LogError("Load Fail");
                return false;
            }


            //Stop the tracker.
            objectTracker.Stop();

            //objectTracker.ActivateDataSet(dataset);
            if (!objectTracker.ActivateDataSet(dataset))
            {
                debugLabel.text += "Activate fail\n";
                Debug.LogError("Activate Fail");
                return false;
            }
        }
        else
        {
            debugLabel.text += "DataSet does not exists\n";
            Debug.LogError("DataSet does not exists");
            return false;
        }


        //Start the object tracker.
        bool started = objectTracker.Start();
        if (started)
        {

            IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
            toDestroy.Clear();
            foreach (TrackableBehaviour tb in tbs)
            {
                if (tb is ModelTargetBehaviour)
                {
                    // add additional script components for trackable
                    tb.gameObject.name = tb.TrackableName;
                    tb.gameObject.AddComponent<DefaultTrackableEventHandler>();
                    tb.gameObject.AddComponent<TurnOffBehaviour>();
                    var msm = tb.gameObject.AddComponent<MonumentStatusManager>();
                    msm.statusLabel = statusLabel;
                    msm.statusInfoLabel = statusLabel;

                    //spawn the corresponding augmentation object and attach to the trackable
                    var augmentationObject = GetAumentationObject(monument, tb.TrackableName);
                    augmentationObject.transform.SetParent(tb.gameObject.transform, false);

                    modelTargets.Add(tb as ModelTargetBehaviour);
                }
                else
                    toDestroy.Add(tb);//destroy this target (usually it spawns an image target)
            }

            //destroy all unwanted targets
            foreach(var obj in toDestroy)
                Destroy(obj.gameObject);
            toDestroy.Clear();
        }
        Debug.Log(started ? "Tracker started" : "Tracked failed to start");
        return started;
    }

    /// <summary>
    /// returns a copyof the augmentation object
    /// </summary>
    /// <param name="monument"></param>
    /// <param name="trackableName"></param>
    /// <returns></returns>
    private GameObject GetAumentationObject(Monument monument, string trackableName)
    {
        foreach(var mt in monument.targets)
        {
            if (mt.modelTarget == trackableName)
                return Instantiate(mt.prefab, Vector3.zero, Quaternion.identity);
        }
        return null;
    }
}


[System.Serializable]
public struct Monument
{
    public string name;
    public Vector2 position;
    public string dataset;
    public MonumentTarget[] targets;
}

[System.Serializable]
public struct MonumentTarget
{
    public string modelTarget;
    public GameObject prefab;
}