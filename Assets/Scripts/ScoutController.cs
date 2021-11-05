using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoutController : MonoBehaviour, IListener
{
    [Header("The largest distance the scout can move toward the target")]
    public float motionAmount;

    private Vector3 nextTargetPosition = Vector3.zero;


    /// <summary>
    /// Capture the target position and move towards the target
    /// </summary>
    /// <param name="Event_Type"></param>
    /// <param name="Sender"></param>
    /// <param name="Param"></param>
    public void OnEvent(EVENT_TYPE Event_Type, Component Sender, object Param = null)
    {
        nextTargetPosition = (Vector3) Param ;


        Vector3 travelVector = Vector3.MoveTowards(transform.position, nextTargetPosition, .01f);
        transform.position = travelVector;
    }

    // Start is called before the first frame update
    void Start()
    {
        SatelliteController.Instance.AddListener(EVENT_TYPE.TARGET_FOUND, this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
