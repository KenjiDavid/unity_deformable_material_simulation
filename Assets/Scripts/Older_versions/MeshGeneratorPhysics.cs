using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class MeshGeneratorPhysics : MonoBehaviour
{

    public Mesh mesh;

    Vector3[] vertices;
    int[] triangles;

    Color[] colors;

    public int xSize = 20;
    public int zSize = 20;
    public float MeshScale = 1f;
    //me pregunta donde esta la posición inicial o origen del mesh
    public int initPosX = 0;
    public int initPosZ = 0;
    public int initPosY = 0;

    public Gradient gradient;

    //variables necesarias para normalizar el gradiente de color
    float minTerrainHeight;
    float maxTerrainHeight;
    List<float> TerrainHeight;
    
    //HECER MAS TARDE !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    float[] DeformationCoeficient; //sirve para hacer zonas en el mesh con diferentes tasas de defromacion
    

    //variables colisionador
    public GameObject player;  //ME COJE EL OBJETO QUE SE ESTA PROBANDO, EN ESTE CASO LA ESFERA
    private Collider groundCollider; //COLISIONADOR

    //variables auxiliares OnColision para la posicion
    private float x_pos, y_pos, z_pos;


    //variables auxiliares
    private int first_time = 0;


    //Datos objeto que colisiona
    public Rigidbody ObjectTest; //NO ESTA BIEN DEL TODO, NO ME OBTIENE CORRECTAMENTE LA MASA DEL OBJETO, SE UTILIZA VARIABLE AUXILIAR

    //variables deformacion
    Vector3[] originalVertices;

    //sirve para saber en cada momento si se esta deformando o no
    public bool isDeforming = false;
    public bool isRecovering = false;
    private float bouncingDeformation = 0.2f; //sirve para que cuando rebote por primera vez, se pueda deformar

    //variables temporizador auxiliar deformacion
    private float recoverDelay = 0.4f; // Retraso antes de la recuperación en segundos
    private float recoverTimer = 0f;

    private float lostContact = 0.0f;  //variable auxuiliar para por si pierde contacto con collison stay y no salta on collision exit

    //---------------------------------------------------------------

    // Variables que se obtendrán a partir de las físicas
    public float deformationSpeed;
    public float maxDeformation;
    public float influenceRadius = 1.0f; // variable ajustable


    //---------------------------------------------------------------

    //---------------------------------------------------------------
    //VARIABLES MUELLE-AMORTIGUADOR
    //variables esfera
    private Vector3 sphereSpeed;  //auxiliar para saber volicidad en y de la esfera
    //variables muelle

    //posicion y velocidad del muelle
    public float springSpeed; 
    public float springDisplacement = 0f; 

    public float springConstant = 1.2f;  //constante del muelle
    private float PsRest = 0.0f;  //posicion incial del muelle
    public float dampingConstant = 0.6f;  //constante del amortiguador

    public float x_offset = 0.0f;
    //---------------------------------------------------------------
    public float sphereMass; //se tiene que sustituir por un game component de la masa del objeto

    //variables pruebas


    // Start is called before the first frame update
    void Start()
    {
        //Crea un nuevo mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        

        // Inicializa TerrainHeight como una lista
        TerrainHeight = new List<float>((xSize + 1) * (zSize + 1));

        //obteine informacion del objeto que colisona
        ObjectTest = player.GetComponent<Rigidbody>();

        //obtiene variables que afectan a la deformacion del mesh

        PsRest = initPosY; // transform.position.y;
        // se pone a valor inicial el desplazamiento del mesh, en la variable springDisplacement
        springDisplacement = initPosY;


        // Llena la lista con ceros
        for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
        {
            TerrainHeight.Add(initPosY);
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

        //coje la masa de la esfera, se puede comentar para controlarlo solo desde UNITY
        //sphereMass = ObjectTest.mass;

        //calculo cual será la x final, cuando se detenga
        x_offset = FindOffsetDeformation(sphereMass, springConstant, initPosY);

        
    }

    void RefreshMesh() //cambiar despues para que haga similar a start()
    { 

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
        maxTerrainHeight = initPosY;  //me soluciona el problema de colores con máx. y min. preestablecidos
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        float y;
        for (int i = 0, z = 0 + initPosZ; z <= zSize + initPosZ; z++)  //ejes x y z horizontales, eje y vertical
        {
            for (int x = 0 + initPosX; x <= xSize + initPosX; x++)
            {
                //float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;

                if (first_time == 0)
                {

                    y = initPosY;

                    //poner el terreno a la altura original y guardarlo en un vector

                }
                else
                {
                    y = TerrainHeight[i];

                }

                vertices[i] = new Vector3(x, y, z)/MeshScale;  //FUNCIONA A MEDIAS, cambia la escala/tamaño de los triangulos

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

        for (int z = 0 + initPosZ; z < zSize + initPosZ; z++)
        {
            for (int x = 0 + initPosX; x < xSize + initPosX; x++)
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


    /*
    //dibuja el lugar donde estan los vertices
    private void OnDrawGizmos()
    {
        //me crea una esfera en cada vertice
        if (vertices == null)
            return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f); 
        }
    }*/
    


    //Detector de colisiones
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("HA ENTRADO EN CONTACTO");

        //Debug.Log("Enter collision: " + collision.transform.name);
        isDeforming = true;
        isRecovering = false; // No recuperar mientras se está deformando
        recoverTimer = recoverDelay; // Reiniciar el temporizador de recuperación

        Vector3 closestOnGround = groundCollider.ClosestPointOnBounds(player.transform.position);
        //guarda la posicion incial
        x_pos = (int)Mathf.Round(closestOnGround.x);
        y_pos = (int)Mathf.Round(closestOnGround.y - 1.5f);  //la posicion de la esfera hace referencia al centro de la esfera, modicficar´cuando se tenga un centro diferente
        z_pos = (int)Mathf.Round(closestOnGround.z);

        //ME DEFORNA EL MATERIAL AL CONTACTO, EN VEZ DE EMPEZAR A DEFORMAR EN COL_STAY, SOLO UNA VEZ CUANDO NO SE ESTA DEFORMANDO

        //me ayda a crear una deformación en el momento que rebota con el mesh
        if (bouncingDeformation <= 0.0f)
        {
            Debug.Log("CONTACTO");

            (springDisplacement, springSpeed) = StepSpringState(springDisplacement, springSpeed, sphereMass);
            //asigno los parámetros de la deformación
            maxDeformation = springDisplacement;
            deformationSpeed = springSpeed;
            influenceRadius = sphereMass * 2;  //MODIFCAR MÁS TARDE
                                                    //luego hacer que la función reciba input
            DeformVerticesPhysics();


            CreateShape();
            UpdateMesh();
            groundCollider = player.transform.GetComponent<Collider>(); //componente muy importante, permite activar los collider y hacer que el mesh tenga físicas


        }
        bouncingDeformation = recoverDelay;
    }




    void OnCollisionStay(Collision collision)
    {
        Vector3 closestOnGround = groundCollider.ClosestPointOnBounds(player.transform.position);
        x_pos = (int)Mathf.Round(closestOnGround.x);
        y_pos = (int)Mathf.Round(closestOnGround.y - 1.5f);
        z_pos = (int)Mathf.Round(closestOnGround.z);
        //Debug.Log("posicion dde contacto con el suelo x: " + x_pos + " y: " + y_pos + " z: " + z_pos);   //opcional, quitar mas tarde


        //SE DECIDE SI SE SIMULA CON FISCAS VISULAMENTE REALISTAS O FISICAS REALES (SUMATORIO DE FUERZAS)
        Debug.Log("DEFORMANDO");
        (springDisplacement, springSpeed) = StepSpringState(springDisplacement, springSpeed, sphereMass);
        //asigno los parámetros de la deformación
        maxDeformation = springDisplacement;
        deformationSpeed = springSpeed;
        influenceRadius = sphereMass * 2;  //MODIFCAR MÁS TARDE
        //luego hacer que la función reciba input
        DeformVerticesPhysics();

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
        Debug.Log("Deja de hacer contacto");
        lostContact = 0.0f;   //me lo pone a cero si no ha saltado por esta manera
    }
    //Probar si late hhace que no se recupere tan facilmente




    void FixedUpdate()
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

            Vector3 sphereSpeed = ObjectTest.GetPointVelocity(player.transform.position); //me consigue la velocidad de la bola a tiempo real
            //Debug.Log("collision speed:" + sphereSpeed);
            if (sphereSpeed[1] != 0.0f)
            {
                springSpeed = sphereSpeed[1];
            }
            //RecoverVertices();  //HACE FALTA REVISAR EL MÉTODO Y ADAPTARLO

            //Debug.Log("RECOVERING VERTICES");

        }
        if (lostContact >= 0.0f)
        {
            lostContact -= Time.deltaTime;

        }
        else
        {
            isDeforming = false;

        }

        //Debug.Log("valor variable y inicial" + initPosY);
        //Debug.Log("valor variable y PsRest" + PsRest);
        

    }//fin update



    // Method to find the index of a vertex given its x and z coordinates, no se usa por el momento
    int FindVertexIndex(int x, int z)
    {
        return z * (xSize + 1) + x;
    }



    //Calcula y devuelve las variables de deformación (desplazamiento) y velocidad del mesh
    public Tuple<float, float> StepSpringState(float Ps0, float Vs0, float masa)
    {
        // Constante gravitacional
        float gravity = -9.81f;

        // Fuerzas que actúan sobre la esfera
        float forceGravity = masa * gravity;
        float forceSpring = -springConstant * (Ps0 - PsRest);
        float forceDamping = -dampingConstant * Vs0;

        // Suma de fuerzas
        float forceSummation = forceGravity + forceSpring + forceDamping;

        // Aceleración de la esfera
        float As0 = forceSummation / masa;

        // Calcular posición en el siguiente paso
        float Ps1 = Ps0 + Vs0 * Time.deltaTime;

        // Calcular velocidad en el siguiente paso
        float Vs1 = Vs0 + As0 * Time.deltaTime;


        // Debug.Log("posicion muelle " + Ps1 + " velocidad " + Vs1);
        return Tuple.Create(Ps1, Vs1);
    }




    public float gaussianInfluence;
    public float MaxDeformedPoint;
    //Calcular los nuevos vértices en función de las físicas aplicadas
    void DeformVerticesPhysics()
    {
        MaxDeformedPoint = 0.0f;
        
        // Iterar sobre todos los vértices
        for (int i = 0; i < vertices.Length; i++)
        {

            Vector3 vertex = originalVertices[i];

            // Calcular la distancia al punto de impacto en el plano XZ
            float distanceToImpact = Vector3.Distance(new Vector3(vertex.x, vertex.y, vertex.z), new Vector3(x_pos, vertex.y, z_pos));

            //calculo el grado de influencia segun el radio que se tiene
            float influence = influenceRadius - Math.Abs(distanceToImpact);
            if (influence <= 0)
            {
                influence = 0;
            }
            else
            {
                influence = (influence / influenceRadius); //calculo en %
            }

            // Calcular el valor Gaussiano basado en la distancia  
            gaussianInfluence = GaussianInfluence(distanceToImpact, influenceRadius);
            if (gaussianInfluence > 1) Debug.Log("HAY ERROR");


            float yChange = maxDeformation * influence * gaussianInfluence; // * gaussianInfluence;

            if (yChange == 0)
            {
                yChange = vertex.y;
            }
            // Aplicar el cambio en Y asegurándose de no exceder el rango permitido
            float currentHeight = TerrainHeight[i];

            
            // Limitar el cambio dentro de los rangos permitidos, según maxDeformation e initPosY
            float targetHeight = Mathf.Clamp(yChange, Mathf.Min(initPosY, maxDeformation), Mathf.Max(initPosY, maxDeformation));


            // Interpolación suave hacia el objetivo

            if (isDeforming && influence > 0)
            {
                TerrainHeight[i] = Mathf.Lerp(currentHeight, targetHeight, Mathf.Abs(deformationSpeed));

                // Mantener el registro del punto más bajo deformado
                if (targetHeight < MaxDeformedPoint)
                {
                    MaxDeformedPoint = targetHeight;
                }
            }
            else
            {
                // Si no está deformando o fuera del radio de influencia, mantener la altura original
                TerrainHeight[i] = initPosY;
            }

        }


    }


    //Controla la gaussiana para que la deformación sea mas redondeada
    float GaussianInfluence(float distance, float influenceRadius)
    {
        // Parámetros de la gaussiana
        float sigma = influenceRadius; // Ajusta sigma para controlar la amplitud de la curva
        float exponent = -(distance * distance) / (2 * sigma * sigma);
        float gaussian = Mathf.Exp(exponent);

        return gaussian;
    }

    //me encuentra la deformacion del muelle enfuncion solo de la masa, deformacion final, no es necario, solo visualizacion
    public float FindOffsetDeformation(float masa, float springConstant , int initalPosY)
    {
        //final estatico, sumatorio de fuerzas = 0
        float gravity = -9.81f;
        float x_final = ((masa * gravity) / springConstant) + initalPosY;
        return x_final;
    }


    //REVISAR MÉTODO MÁS ADELANTE, no funciona con fisicas, simplemente devuelve el mesh a su estado original
    /*
    void RecoverVertices()
    {
        Debug.Log("RECOVERING");
        bool allVerticesRecovered = true;
        for (int i = 0; i < vertices.Length; i++)
        {
            float currentHeight = TerrainHeight[i];
            float targetHeight = originalVertices[i].y;
            TerrainHeight[i] = Mathf.Lerp(currentHeight, targetHeight, Math.Abs(deformationSpeed) * Time.deltaTime);// se recupera al doble de velocidad que se deforma
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
    }*/






    //MODIFICACIONES A LAS VARIABLES EN FUNCIÓN DEL CANVAS   

    //se cambia la posición original o punto de origen del mesh, ejes horizontales X y Z, vertical Y
    public void UpdateInitialPosX(int newValue)
    {

        initPosX = newValue;
        //RECALCULAR MESH
        first_time = 0;
        Start();

    }

    public void UpdateInitialPosZ(int newValue)
    {

        initPosZ = newValue;
        //RECALCULAR MESH
        first_time = 0;
        Start();

    }
    public void UpdateInitialPosY(int newValue)
    {

        initPosY = newValue;
        //RECALCULAR MESH
        first_time = 0;
        Start();
        

    }


    public void UpdateSphereWeight(float newValue)
    {

        sphereMass = newValue;
        //RECALCULAR MESH
        x_offset = FindOffsetDeformation(sphereMass, springConstant, initPosY);

    }

    public void UpdateSprindConst(float newValue)
    {
        springConstant = newValue;
        //RECALCULAR MESH
        x_offset = FindOffsetDeformation(sphereMass, springConstant, initPosY);

    }
    public void UpdateDampConst(float newValue)
    {
        dampingConstant = newValue;
        //RECALCULAR MESH
    }


    public void UpdateXsizeConst(int newValue)
    {
        xSize = newValue;
        //RECALCULAR MESH
        first_time = 0;
        Start();
    }

    public void UpdateZsizeConst(int newValue)
    {
        zSize = newValue;
        //RECALCULAR MESH
        first_time = 0;
        Start();
    }

    public void UpdateScaleConst(float newValue)
    {
        
        //RECALCULAR MESH
        //transform.localScale = new Vector3(newValue, 1, newValue);
        MeshScale = newValue;
        first_time = 0;
        Start();

    }

}//fin programa



//***********************************************************************************************************************************




