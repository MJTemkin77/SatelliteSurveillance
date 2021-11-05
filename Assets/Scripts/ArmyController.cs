using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ArmyController : MonoBehaviour
{
    // Store the input movement to this vector
    private Vector2 movement;
    private Vector3 originalPosition;

    

    private void Awake()
    {
        originalPosition = transform.position;
        

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnMove(InputValue movementValue)
    {
        movement = movementValue.Get<Vector2>();
    }

    private void OnFire(InputValue mv)
    {
        transform.position = originalPosition;
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector3 localMove = new Vector3(-movement.x, 0, movement.y);
        transform.Translate(localMove * Time.deltaTime, Space.World);
    }
}
