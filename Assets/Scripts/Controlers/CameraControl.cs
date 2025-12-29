using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    public float x_dist = 0;
    public float y_dist = 5;
    public float z_dist = -20;

    public float x_rot = 0;
    public float y_rot = 0;
    public float z_rot = 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + new Vector3(x_dist, y_dist, z_dist);
        transform.rotation = Quaternion.Euler(x_rot, y_rot, z_rot); 
    }

    private void LateUpdate()
    {
        transform.position = player.transform.position + new Vector3(x_dist, y_dist, z_dist);
        transform.rotation = Quaternion.Euler(x_rot, y_rot, z_rot);
    }
}
