using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float speed = 5.0f;
    
    // Start is called before the first frame update
    void Start()
    {



    }

    // Update is called once per frame
    void Update()
    {

        
        Vector3 movementInput = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) movementInput.z = 1;
        if (Input.GetKey(KeyCode.S)) movementInput.z = -1;
        if (Input.GetKey(KeyCode.A)) movementInput.x = -1;
        if (Input.GetKey(KeyCode.D)) movementInput.x = 1;
        if (Input.GetKey(KeyCode.Q)) movementInput.y = 1;
        if (Input.GetKey(KeyCode.E)) movementInput.y = -1;

        Move(movementInput);



    }

    void Move(Vector3 direction)
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }

}
