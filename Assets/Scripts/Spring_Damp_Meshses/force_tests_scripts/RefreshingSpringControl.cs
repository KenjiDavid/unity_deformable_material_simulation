using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshingSpringControl : MonoBehaviour
{


    public SpringJoint springJoint; // Referencia al componente SpringJoint

    // Valores iniciales del SpringJoint
    private float initialSpring;
    private float initialDamper;
    private float initialTolerance;
    private float initialDistance;

    // Configuración personalizada
    public float maxDeformationMultiplier = 3f; // Límite máximo de deformación como múltiplo del valor original
    public float SpringIncrMultiplier = 1f;
    public float DampingIncrMultiplier = 1f;
    public float ToleranceIncrMultiplier = 1f;
    private float maxDeformationDistance = 0.0f;
    private bool isBroken = false; // Estado del muelle

    // Start is called before the first frame update
    void Start()
    {
        
        
        if (springJoint == null)
        {
            Debug.LogError("No se encontró SpringJoint en el objeto.");
            enabled = false;
            return;
        }

        // Guarda los valores iniciales del SpringJoint
        initialSpring = springJoint.spring;
        initialDamper = springJoint.damper;
        initialTolerance = springJoint.tolerance;

        // Calcular la distancia inicial
        Vector3 connectedPosition = springJoint.connectedBody != null
            ? springJoint.connectedBody.position
            : springJoint.connectedAnchor;
        initialDistance = Vector3.Distance(transform.position, connectedPosition);

        // Calcular el límite máximo de deformación
        maxDeformationDistance = initialDistance * maxDeformationMultiplier;

        

    }








    // Update is called once per frame
    void Update()
    {


        if (springJoint != null) UpdateSpringLogic();


        //if (springJoint == null || isBroken) return;


    } //end update


    private void UpdateSpringLogic()
    {
        // Calcula la distancia actual entre los objetos conectados
        Vector3 connectedPosition = springJoint.connectedBody != null
            ? springJoint.connectedBody.position
            : springJoint.connectedAnchor;
        float currentDistance = Vector3.Distance(transform.position, connectedPosition);

        // Comprueba si supera el límite de deformación
        //if (currentDistance > maxDeformationDistance) //OPCIONAL
        //{
        //    Debug.Log("se ha roto un muelle");
        //    BreakSpring();
        //    return;
        //}

        // Calcular el factor de deformación basado en la diferencia
        float deformationFactor = (currentDistance - initialDistance) / initialDistance;

        // Actualiza los valores del SpringJoint en función del desplazamiento
        springJoint.spring = initialSpring + Mathf.Pow(deformationFactor * SpringIncrMultiplier, 2);
        springJoint.damper = initialDamper + Mathf.Pow(deformationFactor * DampingIncrMultiplier, 2);
        springJoint.tolerance = initialTolerance + deformationFactor * ToleranceIncrMultiplier * initialTolerance;
    }




    //OPCIONAL, SE PUEDE APLICAR EL BREAK FORCE DESDE EL MENU PRINCIPAL
    private void BreakSpring()
    {
        // Rompe el muelle
        isBroken = true;

        // Se rompe el muelle
        springJoint.breakForce = 0;

        // Si no quedan más SpringJoint en el objeto, destruimos el Rigidbody

        Rigidbody rb = GetComponent<Rigidbody>();
        SpringJoint sp = rb.GetComponent<SpringJoint>();
        SphereCollider sc = rb.GetComponent<SphereCollider>();

        if (sp != null) //comprueba si quedan muelles asignados a ese nodo, si no, elimina el collider. EN FASE DE PRUEBA
        {
            Destroy(sc);
            Debug.Log("Rigidbody destruido porque no quedan más SpringJoint.");
        }

    }



}
