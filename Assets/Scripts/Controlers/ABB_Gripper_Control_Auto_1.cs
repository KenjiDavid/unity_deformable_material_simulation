using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


//test control gripper mediante co-rutinas
public class ABB_Gripper_Control_Auto_1 : MonoBehaviour
{
    public ABB_Gripper_Control gripperControl;
    private bool moving = false;

    // Start is called before the first frame update
    void Start()
    {
        // Iniciar la secuencia de acciones
        StartCoroutine(Sequence());
    }

    public IEnumerator Sequence()
    {
        Debug.Log("Esperando a que se pulse la tecla 'H'...");
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                yield return StartMotion();
            }

            yield return null; // Esperar al siguiente frame
        }
        
        
    }




    public IEnumerator StartMotion()
    {
        Debug.Log("Moviendo Gripper");   

        yield return OpenGripper();
        yield return new WaitForSeconds(1);

        yield return ForwardGripper();
        yield return new WaitForSeconds(1);
        
        yield return CloseGripper();
        yield return new WaitForSeconds(1);

        yield return RotationGripper();
        yield return new WaitForSeconds(1);

        yield return OpenGripper();
        yield return new WaitForSeconds(1);

        yield return BackwardGripper();
        yield return new WaitForSeconds(1);
       

        yield return null; // Esperar al siguiente frame
    }

    //abre el gripper
    public IEnumerator OpenGripper()
    {
        gripperControl.in_position = false;
        Debug.Log("Abriendo Gripper");
        gripperControl.stroke = 20;
        gripperControl.speed = 20;
        gripperControl.start_movemet = true;

        yield return gripperControl.in_position == true;
    }

    //cierra el gripper
    IEnumerator CloseGripper()
    {
        gripperControl.in_position = false;
        Debug.Log("Cerrando Gripper");
        gripperControl.stroke = 5;
        gripperControl.speed = 20;
        gripperControl.start_movemet = true;

        yield return gripperControl.in_position == true;
    }

    //avanza el gripper
    IEnumerator ForwardGripper()
    {
        Debug.Log("moviendo hacia delante");
        Vector3 moveDirection = Vector3.forward;
        Vector3 targetPosition  = new Vector3(transform.position.x + 4, transform.position.y, transform.position.z);
        while (transform.position.x < -8.3f)
        {
            // Mueve el objeto hacia la posición objetivo
            transform.Translate(moveDirection * 1.5f * Time.deltaTime, Space.Self);

            // Mover y rotar el hijo de forma sincronizada

            yield return null; // Esperar al siguiente frame
        }
        
        yield return transform.position.x >= -8.3f; // Esperar al siguiente frame
    }


    
    


    

    //gira el gripper
    IEnumerator RotationGripper()
    {
        Vector3 moveDirection = Vector3.left; // Dirección del movimiento
        Vector3 rotationAxis = Vector3.forward; // Eje de rotación

        // Desacoplar temporalmente hijos que no deben moverse
        Transform childToExclude = transform.Find("Malla");
        if(childToExclude != null) childToExclude.SetParent(null); Debug.Log("encontrado");

        while (transform.rotation.z > -265 && transform.rotation.z < 0)
        {

            // Obtener el ángulo Z normalizado (-180 a 180)
            float zAngle = transform.eulerAngles.z;
            if (zAngle > 180) zAngle -= 360;

            // Verificar la condición de rotación
            if ( zAngle <= 90 && zAngle > 0) break;

            //Debug.Log($"Rotando, ángulo Z: {zAngle}");


            Debug.Log("Rotando");
            // Mover el objeto
            this.transform.Translate(moveDirection * 0.5f * Time.deltaTime, Space.Self);

            // Rotar el objeto
            this.transform.Rotate(rotationAxis, -5f * Time.deltaTime, Space.Self);
            yield return null; // Esperar al siguiente frame
        }

        // Reacoplar el hijo al padre
        if (childToExclude != null) childToExclude.SetParent(transform);

        yield return null; // Esperar al siguiente frame
    }

    IEnumerator BackwardGripper()
    {
        Debug.Log("moviendo hacia atras");
        Vector3 targetPosition = new Vector3(transform.position.x - 4, transform.position.y, transform.position.z);
        while (transform.position.x > -20.3f)
        {
            // Mueve el objeto hacia la posición objetivo
            this.transform.position = Vector3.MoveTowards(transform.position, targetPosition, 1.5f * Time.deltaTime);
            yield return null; // Esperar al siguiente frame
        }

        yield return transform.position.x <= -20.3f; // Esperar al siguiente frame
    }

}
