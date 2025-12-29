using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Malla_1 : MonoBehaviour
{

    private Mesh originalMesh;
    private Vector3[] originalVertices;
    private Vector3[] currentVertices;

    // Ajusta este valor según la fuerza de recuperación deseada
    public float elasticForce = 0.5f;

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        originalMesh = meshFilter.mesh;
        originalVertices = originalMesh.vertices;
        currentVertices = originalMesh.vertices;
    }

    void Update()
    {
        // Simula la recuperación elástica
        for (int i = 0; i < currentVertices.Length; i++)
        {
            // Aplica la fuerza de recuperación
            currentVertices[i] += (originalVertices[i] - currentVertices[i]) * elasticForce * Time.deltaTime;
        }

        // Actualiza la malla con los vértices modificados
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.vertices = currentVertices;
        mesh.RecalculateNormals(); // Recalcula las normales para que la iluminación se vea correctamente
    }

    // Método para aplicar una fuerza externa a la malla (ej. al lanzar una esfera)
    public void ApplyForce(Vector3 position, float force)
    {
        for (int i = 0; i < currentVertices.Length; i++)
        {
            float distance = Vector3.Distance(currentVertices[i], position);
            float influence = Mathf.Clamp01(1.0f - distance / 2.0f); // Ajusta el radio de influencia

            // Aplica la fuerza en función de la distancia
            currentVertices[i] += (position - currentVertices[i]) * influence * force;
        }
    }
}