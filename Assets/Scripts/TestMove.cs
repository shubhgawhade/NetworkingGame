using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    float moveSpeed = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        Vector3 moveDir = new Vector3(horizontalInput, 0, verticalInput);

        Move(moveDir, Time.deltaTime);
    }
    
    public void Move(Vector3 moveDir, float timeBetweenTicks)
    {
        moveDir.Normalize();
        transform.position += moveDir * (moveSpeed * timeBetweenTicks);
    }
}