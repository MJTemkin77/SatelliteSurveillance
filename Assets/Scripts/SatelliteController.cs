using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The SatteliteController is a singleton that will be 
/// visible to all within the game 
/// except for those tagged Army
/// </summary>
public class SatelliteController : MonoBehaviour
{
    #region Variables

    /// <summary>
    /// The GameObject used as the boundary.
    /// The satellite travels within this boundary.
    /// </summary>
    public GameObject boundary;

    /// <summary>
    /// The string value of the tag that 
    /// the satellite is search for.
    /// </summary>
    public string targetTag = "Army";

    /// <summary>
    /// Speed in milliseconds
    /// </summary>
    [Header("Modify the speed of the satellite")]
    public int motionFactor;


    /// <summary>
    /// The current direction of the satellite movement
    /// </summary>
    private Vector3 direction = Vector3.left;

    /// <summary>
    /// Cached reference to the bounds property of the boundary Game Object
    /// </summary>
    private Bounds bounds;

    /// <summary>
    /// LayerMask needed by CheckTarget in order to 
    /// bypass the Boundary game object.
    /// </summary>
    private int layerMask;

    // Following are borrowed from Thorn EventManager Chapter 5
    //Internal reference to Notifications Manager instance (singleton design pattern)
    private static SatelliteController instance = null;

    //Array of listener objects (all objects registered to listen for events)
    private Dictionary<EVENT_TYPE, List<IListener>> Listeners = new Dictionary<EVENT_TYPE, List<IListener>>();

    #endregion
    #region C# properties
    //-----------------------------------------------------------
    //Public access to instance
    public static SatelliteController Instance
    {
        get { return instance; }
        set { }
    }

    void Awake()
    {
        //If no instance exists, then assign this instance
        if (instance == null)
        {
            instance = this;
            layerMask = LayerMask.NameToLayer("Boundary");
            DontDestroyOnLoad(gameObject); //Prevent object from being destroyed on scene exit
        }
        else //Instance already exists, so destroy this one. This should be a singleton object
            DestroyImmediate(this);
    }


    #endregion


    //-----------------------------------------------------------
    #region Event-related methods
    //Called at start-up to initialize
    /// <summary>
    /// Function to add specified listener-object to array of listeners
    /// </summary>
    /// <param name="Event_Type">Event to Listen for</param>
    /// <param name="Listener">Object to listen for event</param>
    public void AddListener(EVENT_TYPE Event_Type, IListener Listener)
    {
        //List of listeners for this event
        List<IListener> ListenList = null;

        //New item to be added. Check for existing event type key. If one exists, add to list
        if (Listeners.TryGetValue(Event_Type, out ListenList))
        {
            //List exists, so add new item
            ListenList.Add(Listener);
            return;
        }

        //Otherwise create new list as dictionary key
        ListenList = new List<IListener>();
        ListenList.Add(Listener);
        Listeners.Add(Event_Type, ListenList); //Add to internal listeners list
    }
    //-----------------------------------------------------------
    /// <summary>
    /// Function to post event to listeners
    /// </summary>
    /// <param name="Event_Type">Event to invoke</param>
    /// <param name="Sender">Object invoking event</param>
    /// <param name="Param">Optional argument</param>
    public void PostNotification(EVENT_TYPE Event_Type, Component Sender, object Param = null)
    {
        //Notify all listeners of an event

        //List of listeners for this event only
        List<IListener> ListenList = null;

        //If no event entry exists, then exit because there are no listeners to notify
        if (!Listeners.TryGetValue(Event_Type, out ListenList))
            return;

        //Entry exists. Now notify appropriate listeners
        for (int i = 0; i < ListenList.Count; i++)
        {
            if (!ListenList[i].Equals(null)) //If object is not null, then send message via interfaces
                ListenList[i].OnEvent(Event_Type, Sender, Param);
        }
    }
    //-----------------------------------------------------------
    //Remove event type entry from dictionary, including all listeners
    public void RemoveEvent(EVENT_TYPE Event_Type)
    {
        //Remove entry from dictionary
        Listeners.Remove(Event_Type);
    }
    //-----------------------------------------------------------
    //Remove all redundant entries from the Dictionary
    public void RemoveRedundancies()
    {
        //Create new dictionary
        Dictionary<EVENT_TYPE, List<IListener>> TmpListeners = new Dictionary<EVENT_TYPE, List<IListener>>();

        //Cycle through all dictionary entries
        foreach (KeyValuePair<EVENT_TYPE, List<IListener>> Item in Listeners)
        {
            //Cycle through all listener objects in list, remove null objects
            for (int i = Item.Value.Count - 1; i >= 0; i--)
            {
                //If null, then remove item
                if (Item.Value[i].Equals(null))
                    Item.Value.RemoveAt(i);
            }

            //If items remain in list for this notification, then add this to tmp dictionary
            if (Item.Value.Count > 0)
                TmpListeners.Add(Item.Key, Item.Value);
        }

        //Replace listeners object with new, optimized dictionary
        Listeners = TmpListeners;
    }
    //-----------------------------------------------------------
    //Called on scene change. Clean up dictionary
    void OnLevelWasLoaded()
    {
        RemoveRedundancies();
    }
    //-----------------------------------------------------------
    #endregion

    #region Satellite method navigation
    // Start is called before the first frame update
    void Start()
    {
        bounds = boundary.GetComponent<MeshRenderer>().bounds;
    }


    /// <summary>
    /// Example of using the Unity event when the object 
    /// leaves the camera view. 
    /// If the scene view is also open then this event 
    /// will become true when the satellite goes "off-screen"
    /// in whichever view, game or scene, is larger on the
    /// axis that the satellite is travelling.
    /// </summary>
    private void OnBecameInvisible()
    {
        if (boundary == null)
            return;

        Vector3 pos = transform.position;
        Debug.Log($"Off-Screen at {pos.x}, {pos.y}, {pos.z}");
        Vector3 nextPos = boundary.transform.position - bounds.extents;
        Debug.Log($"Now appearing at {nextPos.x}, {nextPos.y}, {nextPos.z}");
        transform.position = nextPos;
    }

    /// <summary>
    /// Move the satellite to the left and then to the right 
    /// once the left boundary is exceeded.  The same goes for 
    /// the right side.
    /// </summary>
    void Update()
    {
        transform.Translate(motionFactor * Time.deltaTime * direction);

        if (!bounds.Contains(transform.position))
            ChangeDirection();

        if (CheckForTarget(out Vector3 target))
        {
            PostNotification(EVENT_TYPE.TARGET_FOUND, this, target);
        }

    }

    /// <summary>
    /// Check if the target is below the satellite.
    /// Return true if found and the position of 
    /// the targer.
    /// </summary>
    /// <param name="target">Out parameter that will return the target vector</param>
    /// <returns>True if found</returns>
    private bool CheckForTarget(out Vector3 target)
    {
        target = Vector3.zero;

        // Check the position below the satellite
        // Same position with 0 for height (y)
        Vector3 below = transform.position;
        below.y = 0f;

        if (Physics.Linecast(transform.position, below, out RaycastHit info, layerMask))
        {
            if (info.collider.CompareTag(targetTag))
            {
                target = info.collider.transform.position;
                Debug.Log($"Found {targetTag} at:{target.x}, {target.y}");
                return true;
            }
        }
        return false;
    }

    /// <summary>
    ///  Change direction if the satellite is outside the boundary 
    /// and reset the satellite at the closest point inside 
    /// the boundary
    /// Bounds.Contains and Bounds.ClosestPoint are both 
    /// standard Unity methods in the Bounds class.
    /// </summary>
    private void ChangeDirection()
    {
        direction = direction == Vector3.left ? Vector3.right : Vector3.left;

        // Or you can use this syntax for the line above.
        /*  
        if (direction == Vector3.left)
        {
            direction = Vector3.right;
        }
        else
        {
            direction = Vector3.left;
        }
        */
        // Move to the closest position inside the boundary
        Vector3 closestPoint = bounds.ClosestPoint(transform.position);
        transform.position = closestPoint;
    }
    #endregion Satellite method navigation
}
