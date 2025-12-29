using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class MeshGenerator : MonoBehaviour
{

    public Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    Color[] colors;

    public int xSize = 20;
    public int zSize = 20;

    public Gradient gradient;

    //variables necesarias para normalizar el gradiente de color
    float minTerrainHeight;
    float maxTerrainHeight;
    List<float> TerrainHeight;

    //variables colisionador
    public GameObject player;  //ME COJE EL OBJETO QUE SE ESTA PROBANDO, EN ESTE CASO LA ESFERA
    private Collider groundCollider; //COLISIONADOR

    //variables auxiliares OnColision para la posicion
    private float x_pos, y_pos, z_pos;
  

    //variables auxiliares
    private int first_time = 0;
    private float MaxDeformedPoint;

    //Datos objeto que colisiona
    public Rigidbody ObjectTest; //NO ESTA BIEN DEL TODO, NO ME OBTIENE CORRECTAMENTE LA MASA DEL OBJETO, SE UTILIZA VARIABLE AUXILIAR

    //variables deformacion
    Vector3[] originalVertices;
    public bool isDeforming = false;
    public bool isRecovering = false;
    private float bouncingDeformation = 0.2f; //sirve para que cuando rebote por primera vez, se pueda deformar

    //variables temporizador auxiliar deformacion
    private float recoverDelay = 0.2f; // Retraso antes de la recuperación en segundos
    private float recoverTimer = 0f;

    private float lostContact = 0.0f;  //variable auxuiliar para por si pierde contacto con collison stay y no salta on collision exit

    //---------------------------------------------------------------
    //VARIABLES DETINADAS A SER ELIMINADAS UNA VEZ SE IMPLEMENTE CORRECTAMENTE EL MUELLE, O SE PUEDE QUEDAR COMO MUESTRA
    // Variables que dependerán del peso del objeto
    private float deformationSpeed;
    private float maxDeformation;
    private float influenceRadius;

    public float deformationSpeedRatio = 0.25f;
    public float maxDeformationRatio = 0.5f;
    public float influenceRadiusRatio = 2.0f;

    //---------------------------------------------------------------
    public bool sim0_for1 = false;
    //---------------------------------------------------------------
    //VARIABLES MUELLE-AMORTIGUADOR





    //---------------------------------------------------------------
    public float ObjetcMass = 5; //se tiene que sustituir por un game component de la masa del objeto

    //

    // Start is called before the first frame update
    void Start()
    {
        //Crea un nuevo mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // Inicializa TerrainHeight como una lista
        TerrainHeight = new List<float>((xSize + 1) * (zSize + 1));

        //obteine informacion del objeto que colisona
        ObjectTest = GetComponent<Rigidbody>();

        //obtiene variables que afectan a la deformacion del mesh
        if (sim0_for1 == false) CalculateVariables();
        else CalculateVariables();//HAY QUE CAMBIARLO


        // Llena la lista con ceros
        for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
        {
            TerrainHeight.Add(0f);
        }

        CreateShape();
        UpdateMesh();

        groundCollider = player.transform.GetComponent<Collider>();
        first_time = 1;

        //hace copia de seguridad de los vérices originales
        originalVertices = mesh.vertices.Clone() as Vector3[];

        //componente muy importante, permite activar los collider y hacer que el mesh tenga físicas
        // Assign the mesh to the MeshCollider
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null; // Clear the MeshCollider first
        meshCollider.sharedMesh = mesh; // Reassign the updated mesh


    }


    void CreateShape()
    {
        
        CreateVertices();
        CreateTriangles();
        AssignColors();
    }


    //me recalcula donde poner cada vertice, el cual se guarda en la matriz TerrainHeight y se va implementando uno a uno
    void CreateVertices()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        float y;
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                //float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;

                if (first_time == 0)
                {
                    y = 0; // Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;

                }
                else
                {
                    y = TerrainHeight[i];

                }

                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;

                i++;
            }
        }
    }

    //me crea los triangulos del mesh en función de donde estan situados los vertices
    void CreateTriangles()
    {
        int vert = 0;
        int tris = 0;
        triangles = new int[xSize * zSize * 6];

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    //Me asigna un gradiente de colores en el mesh en fucnión de su deformación
    void AssignColors()
    {
        colors = new Color[vertices.Length];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);

                i++;
            }
        }
    }



    // Update Mesh, me actualiza el mesh a tiempo real por si hay cambios
    void UpdateMesh()
    {
        //me comprueba que los mesh esten limpios, y me los actualiza
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();


        mesh.RecalculateBounds();
        
        // Ensure the MeshCollider is updated
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null; // Clear the MeshCollider
        meshCollider.sharedMesh = mesh; // Reassign the updated mesh
    }


    
    /*//dibuja el lugar donde estan los vertices
    private void OnDrawGizmos()
    {
        //me crea una esfera en cada vertice
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f); 
        }
    }
    */


    //Detector de colisiones
    void OnCollisionEnter(Collision collision)
    {
        CalculateVariables();


        //Debug.Log("Enter collision: " + collision.transform.name);
        isDeforming = true;
        isRecovering = false; // No recuperar mientras se está deformando
        recoverTimer = recoverDelay; // Reiniciar el temporizador de recuperación

        Vector3 closestOnGround = groundCollider.ClosestPointOnBounds(player.transform.position);
        //guarda la posicion incial
        //collisions = collision.contactCount; //auxiliar, cuenta los puntos de colision
        x_pos = (int)Mathf.Round(closestOnGround.x);
        y_pos = (int)Mathf.Round(closestOnGround.y - 1);
        z_pos = (int)Mathf.Round(closestOnGround.z);

        //ME DEFORNA EL MATERIAL AL CONTACTO, EN VEZ DE EMPEZAR A DEFORMAR EN COL_STAY, SOLO UNA VEZ CUANDO NO SE ESTA DEFORMANDO

        
        if (bouncingDeformation <= 0.0f)
        {
            //SE DECIDE SI SE SIMULA CON FISCAS VISULAMENTE REALISTAS O FISICAS REALES (SUMATORIO DE FUERZAS)
            if (sim0_for1 == false) DeformingVertices();
            else DeformingVertices();//HAY QUE CAMBIARLO
            CreateShape();
            UpdateMesh();
            groundCollider = player.transform.GetComponent<Collider>(); //componente muy importante, permite activar los collider y hacer que el mesh tenga físicas
            Debug.Log("CONTACTO");
            
        }
        bouncingDeformation = recoverDelay;
    }

    


    void OnCollisionStay(Collision collision)
    {
        Vector3 closestOnGround = groundCollider.ClosestPointOnBounds(player.transform.position);
        //Debug.Log("Posicion de choque " + closestOnGround);
        //Debug.Log("Stay collision: " +  collision.transform.name);
        //collisions = collision.contactCount;
        x_pos = (int)Mathf.Round(closestOnGround.x);
        y_pos = (int)Mathf.Round(closestOnGround.y - 1);
        z_pos = (int)Mathf.Round(closestOnGround.z);
        //Debug.Log("posicion dde contacto con el suelo x: " + x_pos + " y: " + y_pos + " z: " + z_pos);   //opcional, quitar mas tarde
     

        //FindVertexIndex((int)x_pos,(int)z_pos); //encuentra la psoción del vértice


        Debug.Log("Masa " + ObjectTest.mass);


        //SE DECIDE SI SE SIMULA CON FISCAS VISULAMENTE REALISTAS O FISICAS REALES (SUMATORIO DE FUERZAS)
        if (sim0_for1 == false) DeformingVertices();
        else DeformingVertices();//HAY QUE CAMBIARLO


        CreateShape();
        UpdateMesh();
    
        groundCollider = player.transform.GetComponent<Collider>(); //componente muy importante, permite activar los collider y hacer que el mesh tenga físicas

        //repite que se esta haciendo, por precaución
        isDeforming = true;
        isRecovering = false; // No recuperar mientras se está deformando
        recoverTimer = recoverDelay; // Reiniciar el temporizador de recuperación
        lostContact = recoverDelay;

    }
    
    void OnCollisionExit(Collision collision)
    {
        isDeforming = false;
        recoverTimer = recoverDelay; // Iniciar el temporizador de recuperación
        //Debug.Log("Deja de hacer contacto: " + collision.transform.name);
        lostContact = 0.0f;   //me lo pone a cero si no ha saltado por esta manera
    }
    //Probar si late hhace que no se recupere tan facilmente
    

    

    void Update()
    {
        if (!isDeforming)
        {
            bouncingDeformation -= Time.deltaTime;
            recoverTimer -= Time.deltaTime;
            if (recoverTimer <= 0f)
            {
                isRecovering = true;
            }
        }
        else
        {
            isRecovering = false;
            recoverTimer = recoverDelay; // Reiniciar el temporizador de recuperación
        }

        if (isRecovering)
        {
            RecoverVertices();
            //Debug.Log("RECOVERING VERTICES");
            
        }
        if (lostContact >= 0.0f) lostContact -= Time.deltaTime;
        else isDeforming = false;
    }

    // Method to find the index of a vertex given its x and z coordinates
    int FindVertexIndex(int x, int z)
    {
        return z * (xSize + 1) + x;
    }




    void CalculateVariables()
    {
        if (ObjectTest != null)
        {
            // Calcular las variables basadas en la masa del objeto peso
            float weight = ObjetcMass;

            // Ajustar las variables en función del peso
            deformationSpeed = weight * deformationSpeedRatio; // Ajusta según necesites
            maxDeformation = weight * maxDeformationRatio; // Ajusta según necesites
            influenceRadius = weight * influenceRadiusRatio; // Ajusta según necesites
        }
        else
        {
            // Si no se encuentra el Rigidbody, valores por defecto
            deformationSpeed = 1f;
            maxDeformation = 1f;
            influenceRadius = 3f;
        }
    }




    //Deforma los vertices
    void DeformingVertices()
    {
        

        MaxDeformedPoint = 0.0f;

        // Itera sobre todos los vértices

        // Constantes para la función Gaussiana
        float sigma = influenceRadius / 3.0f;
        float sigma2 = sigma * sigma;

        // Iterar sobre todos los vértices
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = originalVertices[i];

            // Calcular la distancia al punto de impacto en el plano XZ
            float distanceToImpact = Vector3.Distance(new Vector3(vertex.x, 0, vertex.z), new Vector3(x_pos, 0, z_pos));

            // Calcular la influencia basada en una distribución Gaussiana
            float influence = Mathf.Exp(-distanceToImpact * distanceToImpact / (2 * sigma2));

            // Calcular el cambio en Y como una función sinusoidal atenuada por la distancia
            float yChange = -influence * maxDeformation; // * Mathf.Sin(Time.time * deformationSpeed);


            // Aplicar el cambio en Y asegurándose de no exceder el rango permitido
            float currentHeight = TerrainHeight[i];
            float targetHeight = Mathf.Clamp(yChange, -maxDeformation, maxDeformation);

            

            // Interpolación suave hacia el objetivo

            if (isDeforming)
            {
                TerrainHeight[i] = Mathf.Lerp(currentHeight, targetHeight, deformationSpeed * Time.deltaTime);
                if (TerrainHeight[i] < MaxDeformedPoint) MaxDeformedPoint = -TerrainHeight[i]; //if que pasa como seguro por si acaso.
            }     
            
        }



    }


    void RecoverVertices()
    {
        Debug.Log("RECOVERING");
        bool allVerticesRecovered = true;
        for (int i = 0; i < vertices.Length; i++)
        {
            float currentHeight = TerrainHeight[i];
            float targetHeight = originalVertices[i].y;
            TerrainHeight[i] = Mathf.Lerp(currentHeight, targetHeight, deformationSpeed * Time.deltaTime * 2);// se recupera al doble de velocidad que se deforma
            if (Mathf.Abs(TerrainHeight[i] - targetHeight) > 0.01f)
            {
                allVerticesRecovered = false;
            }
        }
        CreateShape();
        UpdateMesh();
        if (allVerticesRecovered)
        {
            isRecovering = false;
        }
    }





}//fin del programa
