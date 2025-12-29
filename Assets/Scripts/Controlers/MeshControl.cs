using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshControl : MonoBehaviour
{
    public float speed = 5.0f;

    private Vector3[] initialLocalPositions; // Guarda las posiciones locales iniciales de los hijos


    // Start is called before the first frame update
    void Start()
    {
        /*
        // Guardar las posiciones locales iniciales de los hijos
        int childCount = transform.childCount;
        initialLocalPositions = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            initialLocalPositions[i] = transform.GetChild(i).localPosition;
        }*/
    }

    void Update()
    {
        // Obtener entrada del teclado y calcular movimiento
        Vector3 movementInput = Vector3.zero;

        if (Input.GetKey(KeyCode.I)) movementInput.z = 1;
        if (Input.GetKey(KeyCode.K)) movementInput.z = -1;
        if (Input.GetKey(KeyCode.J)) movementInput.x = -1;
        if (Input.GetKey(KeyCode.L)) movementInput.x = 1;
        if (Input.GetKey(KeyCode.U)) movementInput.y = 1;
        if (Input.GetKey(KeyCode.O)) movementInput.y = -1;


        Move(movementInput);
        /*
        // Escalar movimiento por velocidad y tiempo
        Vector3 movement = movementInput.normalized * speed * Time.deltaTime;

        // Mover el objeto padre
        transform.position += movement;

        // Actualizar las posiciones de los hijos
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.position = transform.position + initialLocalPositions[i];
        }
        */

    }

    void Move(Vector3 direction)
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }

}
