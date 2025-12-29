using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using TMPro;





[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]



public class MeshDeformation_1_3_cloth_config_joint : MonoBehaviour
{

    Mesh mesh;

    public Chart_Script_2_config_joint chartData;

    [HideInInspector]
    public Vector3[] vertices;

    int[] triangles;
    Color[] colors;

    public int xSize = 20;
    public int zSize = 20;
    public float MeshScale = 1f;

    //public float Resolution = 1.0f;
    //me pregunta donde esta la posición inicial o origen del mesh
    public int initPosX = 0;
    public int initPosZ = 0;
    public int initPosY = 0;

    public Gradient gradient;

    //variables necesarias para normalizar el gradiente de color
    float minTerrainHeight;
    float maxTerrainHeight;

    public float lowest_point = 0; //variable publica de manera temporal, solo recoger datos

    //variables auxiliares
    private int first_time = 0; //para que en una funcion comun en Star y Update, solo me pase por una parte

    private int numNodes = 0;



    //---------------------------------------------------------------
    //PROPIEDADES MATERIAL MESH PARA TODOS LOS COLLIDERS
    // Crear un nuevo material físico para el mesh
    PhysicMaterial ElasticMeshMaterial;

    public float MeshDynamicFriction = 0.3f;    //friccion dinamica, valores sugeridos para piel humana entre 0.3-0.5
    public float MeshStaticFriction = 0.2f;     //friccion estatica, valores sugeridos para piel humana entre 0.2-0.4
    public float MeshBounciness = 0.0f;         //rebote, valores sugeridos para piel humana entre 0.0-0.1

    //---------------------------------------------------------------

    private int objectCount = 0; // Número total de objetos
    private GameObject[] objectVector; // Vector para almacenar los objetos

    //---------------------------------------------------------------
    //VARIABLES MUELLE-AMORTIGUADOR
    public float springConstant = 1.2f;  //constante del muelle
    public float dampingConstant = 0.6f;  //constante del amortiguador

    public bool enableGravityMesh;
    private bool previousEnableGravityMesh;

    public float minDistanceSpring = 0.0f;
    public float maxDistanceSpring = 0.0f;

    public float springTolerance = 0.025f;
    //-------------------------------------------------------------------
    //VARIABLES COLLIDER

    public float colliderRadius = 0.0f;
    private float maxColliderRadius = 0.0f;
    private float minColliderRadius = 0.0f;

    public float node_mass = 0.001f;

    //variables canvas
    private bool restartMesh = false;
    public Canvas_1_3 canvas_1;

    //variables capa fantasma o capa 0
    public bool AllowMesh0 = true;
    public Material Mesh0Material; 

    Vector3[] Mesh0Vertices;
    int[] Mesh0Triangles;
    Color[] Mesh0Colors;
    Mesh Mesh0;

    //hacer muelles puedan autoregularse
    public bool SpringAutoUpdate = false;

    //guardar posición muelles y lectura y escritura de vector de posicion en archivo .json
    private string folderPath; // Ruta de la carpeta
    private string filePath;  // Ruta completa del archivo
    private bool hasPressedG = false; // Flag para verificar si la tecla G ya fue presionada
    public bool chargeTemplate = false;
    public string fileName;


    // Start is called before the first frame update
    void Start()
    {
        //Crea un nuevo mesh
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.MarkDynamic(); //Indica que la malla se actualizará constantemente

        if (AllowMesh0 == true)
        {
            CreateMesh0();
        }
        Mesh0Vertices = new Vector3[(xSize + 1) * (zSize + 1)];  //se inicializan los 2 vectores para que no cause problemas
        Mesh0Triangles = new int[xSize * zSize * 6];


        //funciones par controlar el radio del colisionador

        //obtiene los valores min y max del radio del colsionador a partir de la distancia entre nodos
        //***modificar más tarde cuando se modifique caso escala/resolución, calculo de manera temporal, luego modificar
        
        float rad = MeshScale / 2;
        minColliderRadius = rad / 3;
        maxColliderRadius = rad * 4;
        
        if (minColliderRadius > colliderRadius || maxColliderRadius < colliderRadius) colliderRadius = rad;
        
        // Inicializa el rango y el valor inicial del slider
        canvas_1.UpdateSliderRange(minColliderRadius, maxColliderRadius);
        canvas_1.UpdateCurrentValue(colliderRadius);

        //te dice que tamaño y numero de nodos hay
        numNodes = (xSize + 1) * (zSize + 1);
        canvas_1.GridInfo.text = $"(Size Mesh {xSize*MeshScale}X{zSize * MeshScale}, number of nodes {numNodes})";

        //se inicializan las variables temporales del canvas para que no haya problemas al reiniciar el mesh
        tempInitPosX = initPosX;
        tempInitPosY = initPosY;
        tempInitPosZ = initPosZ;

        tempSizeX = xSize;
        tempSizeZ = zSize;
        tempMeshScale = MeshScale;

        tempSpringConstant = springConstant;
        tempDampingConstant = dampingConstant;

        tempMeshStaticFriction = MeshStaticFriction;
        tempMeshDynamicFriction = MeshDynamicFriction;
        tempMeshBounciness = MeshBounciness;

        tempColliderRadius =  colliderRadius;


        //me almacena el bool de gravedad anterior 
        previousEnableGravityMesh = enableGravityMesh;

        // Define la ruta de la carpeta "saved_data" en la raíz del proyecto
        folderPath = Path.Combine(Application.dataPath, "SavedPos");
        // Define la ruta del archivo .json
        filePath = Path.Combine(folderPath, "vector3ArrayData.json");


        //encuentra info del chart
        chartData = FindObjectOfType<Chart_Script_2_config_joint>();


        Debug.Log("GENERATING DEFAULT MESH");
        //creacion mesh
        CreateShape();
        UpdateMesh();

        first_time = 1;
    }



    //funcion inicial para crea el mesh e incorporarle todas las funciones fisicas de este
    void CreateShape()
    {


        //total de objetos vacios
        objectCount = (xSize + 1) * (zSize + 1);
        // Inicializar el vector de objetos vacios 
        objectVector = new GameObject[objectCount];


        // Crear un nuevo material físico para el mesh
        ElasticMeshMaterial = new PhysicMaterial();

        // Definir las propiedades del material para el mesh
        ElasticMeshMaterial.dynamicFriction = MeshDynamicFriction; //friccion dinamica
        ElasticMeshMaterial.staticFriction = MeshStaticFriction;  //friccion estatica
        ElasticMeshMaterial.bounciness = MeshBounciness;  // Qué tan "rebotante" es el objeto

        ElasticMeshMaterial.frictionCombine = PhysicMaterialCombine.Average;  // Cómo combinar la fricción con otros objetos
        ElasticMeshMaterial.bounceCombine = PhysicMaterialCombine.Maximum;  // Cómo combinar el rebote


        CreateVertices();
        CreateSpringNodes();
        AvoidCollisionFunction();
        CreateTriangles();
        AssignColors();

    }


    //me recalcula donde poner cada vertice, el cual se guarda en la matriz TerrainHeight y se va implementando uno a uno
    void CreateVertices()
    {
        maxTerrainHeight = initPosY;  //me soluciona el problema de colores con máx. y min. preestablecidos
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        if (AllowMesh0 == true)  Mesh0Vertices = new Vector3[(xSize + 1) * (zSize + 1)]; //permitir nodos de la capa fantasma


        //si es un archivo plantilla cargado desde un archivo .jos, solo lo carga una vez y luego entra en el bucle para crear los nodos
        if (chargeTemplate == true)
        {
            // Define la ruta del archivo JSON
            filePath = Path.Combine(Application.dataPath, "SavedPos", "templatePos", fileName + ".json");


            // Leer los datos del archivo JSON
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                vertices = JsonUtility.FromJson<Wrapper2>(json).array;
            }
            else
            {
                Debug.LogError("El archivo JSON no existe.");
            }
        }

        float y;
        for (int i = 0, z = initPosZ; z <= zSize + initPosZ; z++)  //ejes x y z horizontales, eje y vertical
        {
            for (int x = initPosX; x <= xSize + initPosX; x++)
            {
                //y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;



                y = initPosY;

 
                //CREA OBJETOS VACIOS Y LES PROPORCIONA "MASA"

                if (chargeTemplate == false)
                {
                    // Definir la posición del nodo
                    Vector3 nodePosition = new Vector3(initPosX + (x - initPosX) * MeshScale, initPosY + (y - initPosY) * MeshScale,
                                                        initPosZ + (z - initPosZ) * MeshScale);


                    // Guardar la posición en los vértices de la malla
                    vertices[i] = new Vector3(nodePosition.x - transform.position.x * 2, nodePosition.y - transform.position.y * 2, nodePosition.z - transform.position.z * 2);
                }
                
               

                //crear nodos capa fantasma/false
                if (AllowMesh0 == true)
                {
                    Mesh0Vertices[i] = vertices[i];
                    Mesh0Vertices[i].y = y + colliderRadius;
                }
                

                // Crear un nuevo GameObject vacío
                GameObject emptyObject = new GameObject("Node_" + i);

                // Opcional: Asignar la posición inicial del objeto
                emptyObject.transform.position = vertices[i]; // nodePosition; // new Vector3(x, y, z)/MeshScale;
                // Guardar el objeto en el vector
                objectVector[i] = emptyObject;
            
                // Emparentar el objeto al mesh para que sea parte de su jerarquía, sin que cambie su posicion
                emptyObject.transform.SetParent(this.transform, false);

                
                //añade un rigidbody al objeto vacio
                Rigidbody rb = emptyObject.AddComponent<Rigidbody>();

                // Comprobar las posiciones X y Z
                if (i < xSize + 1 || i % (xSize + 1) == 0 || i > numNodes - (xSize + 1) || (i + 1) % (xSize + 1) == 0)
                {
                    // Congelar todas las restricciones de movimiento y rotación
                    //rb.constraints = RigidbodyConstraints.FreezePosition;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    //rb.constraints = RigidbodyConstraints.FreezeRotation;
                    
                }
                else
                {
                    // Si no se cumple la condición, se dejan libres
                    rb.constraints = RigidbodyConstraints.None;

                    //congela las rotaciones de los nodos
                    //rb.constraints = RigidbodyConstraints.FreezeRotation; //si se habilita esa linea peta las fisicas de Unity
                    
                }
                //rb.constraints = RigidbodyConstraints.FreezeRotationY;
                //rb.constraints = RigidbodyConstraints.FreezeRotationY;
                // Cambia la masa del Rigidbody a 5
                rb.mass = node_mass;  //masa manta 0.01
                rb.drag = 0f;
                rb.angularDrag = 0.05f;
                rb.isKinematic = false;
                rb.interpolation = RigidbodyInterpolation.Interpolate; // Opciones: None, Interpolate, Extrapolate
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                //se le quita la gravedad al muelle POR VER SI ES CORRECTO
                rb.useGravity = enableGravityMesh;
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;


                //añade un rigidbody al objeto vacio
                SphereCollider sphereCollider = emptyObject.AddComponent<SphereCollider>();  //DESCOMENTAR MAS TARDE
                //BoxCollider sphereCollider = emptyObject.AddComponent<BoxCollider>();
                // Ajustar el radio del SphereCollider
                sphereCollider.radius = colliderRadius;  //DESCOMENTAR MAS TARDE
                //sphereCollider.size = new Vector3 (colliderRadius, colliderRadius, colliderRadius);
                // Hacer el collider un trigger si es necesario
                sphereCollider.isTrigger = false;  // Cambia a true si necesitas que sea un trigger*/

                // Asignar el material al collider
                sphereCollider.material = ElasticMeshMaterial;

                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;

                i++;
            }
        }



    }//fin create vertices

    

    //CURIOSIDAD A COMENTAR A ALBERTO, SOLO PERMITE UNA ESCALA DE 2! (EXPONENCIAL), SI NO PARTE DE LOS MUELLES NO LOS COLOCA
    //SIGUE HABIENDO PROBLEMAS CON LAS ESCALAS CON POSICIONES INICIALES DIFERENTES A 0
    //crea los muelles entre los nodos
    void CreateSpringNodes()
    {


        for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
        {
            GameObject obj = objectVector[i];
            if (obj != null)
            {

                
                //condiciones spring joint 1 (X-1, Z+1), posicion vector i + xSize                  
                if ( i > 0 && i % (xSize + 1) != 0 && i < numNodes - (xSize + 1))

                {
                    //Debug.Log("OK 1, hay " + (numNodes - xSize - zSize - 1));

                    // Obtiene los dos primeros objetos vacíos
                    GameObject firstObject = objectVector[i];
                    GameObject secondObject = objectVector[i + xSize];

                    CreateConfigurableJoint(firstObject, secondObject, springConstant * (float)Math.Sqrt(2), dampingConstant * (float)Math.Sqrt(2));

                }

                //condiciones spring joint 2 (X=, Z+1) , posicion vector i + xSize + 1
                if (i < numNodes - (xSize + 1))
                {
                    //Debug.Log("OK 2, hay " + (numNodes - xSize - 1));

                    // Obtiene los dos primeros objetos vacíos
                    GameObject firstObject = objectVector[i];
                    GameObject secondObject = objectVector[i + xSize + 1];

                    CreateConfigurableJoint(firstObject, secondObject, springConstant, dampingConstant);
                }

                //condiciones spring joint 3 (X+1, Z+1) , posicion vector i + xSize + 2
                if (i < numNodes - (xSize + 1) && (i + 1) % (xSize + 1) != 0)
                {
                    //Debug.Log("OK 3, hay " + (numNodes - xSize - zSize - 1));

                    // Obtiene los dos primeros objetos vacíos
                    GameObject firstObject = objectVector[i];
                    GameObject secondObject = objectVector[i + xSize + 2];

                    CreateConfigurableJoint(firstObject, secondObject, springConstant * (float)Math.Sqrt(2), dampingConstant * (float)Math.Sqrt(2));
                }

                //condiciones spring joint 4 (X+1, Z=) , posicion vector i + 1
                if ((i + 1) % (xSize + 1) != 0 )
                {
                   // Debug.Log("OK 4, hay " + (numNodes - zSize - 1));

                    // Obtiene los dos primeros objetos vacíos
                    GameObject firstObject = objectVector[i];
                    GameObject secondObject = objectVector[i + 1];

                    CreateConfigurableJoint(firstObject, secondObject, springConstant, dampingConstant);
                }

            }

        } //end for

    }//fin CreateSpringNodes


    

    
    //crea el muelle configurable
    void CreateConfigurableJoint(GameObject firstObject, GameObject secondObject, float k_spring, float c_damp)
    {
        // Crea un Spring Joint en el primer objeto
        ConfigurableJoint configurableJoint = firstObject.AddComponent<ConfigurableJoint>();
        configurableJoint.connectedBody = secondObject.GetComponent<Rigidbody>(); // Conecta el segundo objeto

        //Parámetros configurable joint
        configurableJoint.autoConfigureConnectedAnchor = true;

        // Limitación de movimiento en posición (XYZ)
        configurableJoint.xMotion = ConfigurableJointMotion.Limited;
        configurableJoint.yMotion = ConfigurableJointMotion.Limited;
        configurableJoint.zMotion = ConfigurableJointMotion.Limited;

        // Rotación libre en todos los ejes
        configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
        configurableJoint.angularYMotion = ConfigurableJointMotion.Free;
        configurableJoint.angularZMotion = ConfigurableJointMotion.Free;



        //Linear Limit Spring, valor muelle pasado el límite, por el momento 10 veces el original
        SoftJointLimitSpring limitSpring = configurableJoint.linearLimitSpring;
        limitSpring.spring = k_spring * 10;
        limitSpring.damper = c_damp * 10;
        configurableJoint.linearLimitSpring = limitSpring;

        //Linear Limit, por el momento establecido a 2 veces la escala
        SoftJointLimit linearLimitation = configurableJoint.linearLimit;
        linearLimitation.limit = MeshScale;
        linearLimitation.bounciness = 0;  // por el momento a 0, explorar más adelante
        linearLimitation.contactDistance = 0;
        configurableJoint.linearLimit = linearLimitation;

        //Angular limit, limite de los angulos, por el moemnto VACIO


        //valor muelle y amortiguamiento eje X
        JointDrive xAxisDrive = configurableJoint.xDrive;
        xAxisDrive.positionSpring = k_spring;
        xAxisDrive.positionDamper = c_damp;
        configurableJoint.xDrive = xAxisDrive;

        //valor muelle y amortiguamiento eje Y
        JointDrive yAxisDrive = configurableJoint.yDrive;
        yAxisDrive.positionSpring = k_spring;
        yAxisDrive.positionDamper = c_damp;
        configurableJoint.yDrive = yAxisDrive;

        //valor muelle y amortiguamiento eje Z
        JointDrive zAxisDrive = configurableJoint.zDrive;
        zAxisDrive.positionSpring = k_spring;
        zAxisDrive.positionDamper = c_damp;
        configurableJoint.zDrive = zAxisDrive;

        //decidir si se generaliza el giro angular o por separado
        configurableJoint.rotationDriveMode = RotationDriveMode.XYAndZ;

        //valor muelle angular X
        JointDrive xAngular = configurableJoint.angularXDrive;
        xAngular.positionSpring = k_spring * (float)Math.Pow(colliderRadius , 2);
        xAngular.positionDamper = c_damp * (float)Math.Pow(colliderRadius, 2);
        configurableJoint.angularXDrive = xAngular;

        //valor muelle angular YZ
        JointDrive yzAngular = configurableJoint.angularYZDrive;
        yzAngular.positionSpring = k_spring * (float)Math.Pow(colliderRadius, 2);
        yzAngular.positionDamper = c_damp * (float)Math.Pow(colliderRadius, 2);
        configurableJoint.angularYZDrive = yzAngular;



        //Slerp, comprobar más tarde que es
        JointDrive slerpDrive = new JointDrive();
        slerpDrive.positionSpring = k_spring * (float)Math.Pow(colliderRadius, 2);  // Rigidez del muelle (cuánta fuerza aplica para llegar a la rotación objetivo)
        slerpDrive.positionDamper = c_damp * (float)Math.Pow(colliderRadius, 2);    // Amortiguación (resistencia al movimiento)
        slerpDrive.maximumForce = float.PositiveInfinity;    // Fuerza máxima permitida

        configurableJoint.slerpDrive = slerpDrive;

        //distancia mínima para la deformación
        //configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        configurableJoint.projectionMode = JointProjectionMode.None;
        configurableJoint.projectionDistance = 0f;  
        configurableJoint.projectionAngle = 0f; 


        //comprobar más tarde para que sirve
        configurableJoint.configuredInWorldSpace = true;
        configurableJoint.swapBodies = true;
        configurableJoint.breakForce = float.PositiveInfinity;
        configurableJoint.breakTorque = float.PositiveInfinity;

        configurableJoint.enableCollision = true;
        configurableJoint.enablePreprocessing = true;

        configurableJoint.massScale = 1f;
        configurableJoint.connectedMassScale = 1;


    }//en configurable  joint
    


    //utilizo esta función para que los colliders de la malla se ignoren entre si, para que el funcionamiento de la malla sea mas optimo y
    //no se malgaste tanta potencia computacional en calcular los choques entre los empty objects de la malla
    void AvoidCollisionFunction()
    {
        for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
        {
            //cojo el primer empty object para vincularlo al resto
            GameObject fistObject = objectVector[i];
            Collider colliderA = fistObject.GetComponent<Collider>();

            for (int j = i + 1; j < (xSize + 1) * (zSize + 1); j++)
            {
                //cojo el segundo empty object para completar la funcion ignore collision
                GameObject secondObject = objectVector[j];
                Collider colliderB = objectVector[j].GetComponent<Collider>();

                // Asegúrate de que ambos objetos tienen un collider
                if (colliderA != null && colliderB != null)
                {
                    Physics.IgnoreCollision(colliderA, colliderB);  //hago que 2 colliders se ignoren entre si
                    //Debug.Log("Se cumple funcion avoid");
                }
            }
        }

    }



    //me crea los triangulos del mesh en función de donde estan situados los vertices
    void CreateTriangles()
    {
        int vert = 0;
        int tris = 0;
        triangles = new int[xSize * zSize * 6];

        if (AllowMesh0 == true)  Mesh0Triangles = new int[xSize * zSize * 6]; //permitir triangles de la capa fantasma

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

        if (AllowMesh0 == true)  Mesh0Triangles = triangles;  //copiar los trinagulos de la capa fantasma
   
    }//fin create triangles

    //Me asigna un gradiente de colores en el mesh en fucnión de su deformación
    void AssignColors()
    {
        colors = new Color[vertices.Length];
        Mesh0Colors = new Color[vertices.Length];  //permitir colores en la capa fantasma
        float height = 0;
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colors[i] = gradient.Evaluate(height);
                if (AllowMesh0 == true)
                {
                    Mesh0Colors[i] = gradient.Evaluate(height);
                }
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

        if (AllowMesh0 == true && Mesh0 != null)
        {
            // Limpiar y actualizar la malla
            Mesh0.Clear();

            Mesh0.vertices = Mesh0Vertices;
            Mesh0.triangles = Mesh0Triangles;
            Mesh0.colors = Mesh0Colors;

            // Recalcular las propiedades para un renderizado correcto
            Mesh0.RecalculateNormals();
            Mesh0.RecalculateBounds();
        }

        //COLLIDER MESH COMPLETO, NO INDIVIDUAL, SE COMENTA POR EL MOMENTO 
        // Ensure the MeshCollider is updated
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null; // Clear the MeshCollider
        meshCollider.sharedMesh = mesh; // Reassign the updated mesh*/

    }

    void Update()
    {

        //destrye y recrea el mesh con los nuevos datos o actualiza su posicion en funcion de los nodos físicos
        if (restartMesh == true) //me comprueba si se quiere reiniciar el mesh al cambiar las variables de este, recomendable hacerlo sin objetos en contacto
        {
            //elimina todos los elementos del diccionario y los game objects para crearlos de nuevo al regenerar el mesh. Regenerar sin borrar lo anterior dara
            //muchos problemas y es posible que no funcione, ya que no los gameObjects y sus posiciones no se controlan por codigo, y si se reinicia el codigo sin 
            //borrar hará un duplicado mal hecho

            Debug.Log("SE REINICIA EL MESH");
            //se eliminan primero todas las variables necesarias
            first_time = 0;

            //se elimminan los objetos
            foreach (GameObject obj in objectVector)
            {
                if (obj != null) Destroy(obj); // Destruye cada GameObject en la lista
            }
          
            CreateShape();
            UpdateMesh();
            restartMesh = false;
            first_time = 1;
        }
        else
        {
            AssignNewVertices();
            CreateTriangles();
            AssignColors();
            UpdateMesh();
        }
        
        if (AllowMesh0 == true)
        {
            GameObject meshObject = GameObject.Find("Mesh_0");
            if (meshObject == null)
            {
                CreateMesh0();
            }
        }
        else
        {
            GameObject meshObject = GameObject.Find("Mesh_0");
            if (meshObject != null)
            {
                Destroy(meshObject);
            }
        }

        if (enableGravityMesh != previousEnableGravityMesh)
        {
            // Recorre todos los hijos del objeto padre
            foreach (Transform child in transform)
            {
                // Busca el componente Rigidbody en el hijo
                Rigidbody rb = child.GetComponent<Rigidbody>();

                // Si el hijo tiene un Rigidbody, activa la gravedad
                if (rb != null)
                {
                    rb.useGravity = enableGravityMesh;
                }
            }
            previousEnableGravityMesh = enableGravityMesh;
        }

        if (Input.GetKeyDown(KeyCode.G) && !hasPressedG)
        {
            //guarda las posiciones actuales de la malla en un archivo .json


            // Verifica si el archivo existe
            if (!File.Exists(filePath))
            {
                Debug.Log("El archivo no existe. Creándolo ahora...");
                CreateInitialFile(); // Crea un archivo inicial con datos de ejemplo
            }
            else
            {
                Debug.Log($"El archivo ya existe en: {filePath}");
                Debug.Log($"Sobreescribiendo");
                CreateInitialFile();
            }

        }
        // Reiniciar el flag cuando la tecla "G" se haya soltado
        if (Input.GetKeyUp(KeyCode.G))
        {
            hasPressedG = false;
        }


    }//fin update

    //Actualiza la posición de los nodos del mesh con los empty object creados, para que así, los nodos se
    ////muevan con los gameobjects y en consecuencia se mueva el mesh
    void AssignNewVertices()
    {
        lowest_point = 0;
        //Debug.Log("Actualizando");
 
        for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
        {
            GameObject obj = objectVector[i];
            if (obj != null)
            {
                Vector3 meshMovement = this.transform.position;
                

                float x = obj.transform.position.x + meshMovement.x;
                float y = obj.transform.position.y + meshMovement.y;
                float z = obj.transform.position.z + meshMovement.z;

                
                // Definir la posición del nodo
                Vector3 nodePosition = new Vector3(x, y, z);
                //Vector3 nodePosition = new Vector3(x - transform.position.x * 2, y - transform.position.y * 2, z - transform.position.z * 2);

                // Guardar la posición en los vértices de la malla
                vertices[i] = new Vector3(nodePosition.x - transform.position.x * 2, nodePosition.y - transform.position.y * 2, nodePosition.z - transform.position.z * 2);
                //vertices[i] = nodePosition;

                //modificar nodos capa fantasma/false
                if (AllowMesh0 == true && Mesh0Vertices != null) //NO FUNICONA EL DESPLAZAMIENTO DE MESH 0
                {
                    //Mesh0Vertices[i] = new Vector3(nodePosition.x - transform.position.x * 2, nodePosition.y - transform.position.y * 2, nodePosition.z - transform.position.z * 2);
                    Mesh0Vertices[i] = nodePosition;
                    //Mesh0Vertices[i].x = nodePosition.x - transform.position.x * 2;
                    //Mesh0Vertices[i].y = nodePosition.y - transform.position.y * 2 + colliderRadius;
                    //Mesh0Vertices[i].z = nodePosition.z - transform.position.z * 2;

                    Mesh0Vertices[i].y = y + colliderRadius;
                    Debug.Log("posicion mesh 0 " + Mesh0Vertices[i]);
                    
                }

                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;

                if (y < lowest_point) lowest_point = y; 

            }
        } //end for

        //Debug.Log("Punto más bajo registrado " + lowest_point);

    }//fin AssignNewVertices



    //Crea una capa fantasma
    void CreateMesh0()
    {
        GameObject FalseLayer = new GameObject("Mesh_0");

        // Emparentar el objeto al mesh para que sea parte de su jerarquía, sin que cambie su posicion
        //FalseLayer.transform.SetParent(this.transform, false);

        // Añadir el componente MeshFilter y asignar la nueva malla
        MeshFilter Mesh0Filter = FalseLayer.AddComponent<MeshFilter>();
        Mesh0 = new Mesh();
        Mesh0Filter.mesh = Mesh0;

        // Añadir un MeshRenderer con un material para visualizar la malla
        MeshRenderer Mesh0Renderer = FalseLayer.AddComponent<MeshRenderer>();

        // Asignar el material desde el Inspector 
        Mesh0Renderer.material = Mesh0Material;
    }


    // Método para obtener el SpringJoint conectado a un Rigidbody específico
    SpringJoint GetSpringJointByConnectedBody(GameObject obj, Rigidbody targetConnectedBody)
    {
        SpringJoint[] springJoints = obj.GetComponents<SpringJoint>();
        foreach (SpringJoint joint in springJoints)
        {
            if (joint.connectedBody == targetConnectedBody)
            {
                return joint; // Retornar el SpringJoint correcto
            }
        }
        return null; // Retorna null si no se encuentra
    }


    //METODO DE GUARDADO Y LECTURA DE POSICIONES
    // Método para crear un archivo JSON inicial
    private void CreateInitialFile()
    {
        // Datos de ejemplo
        Vector3[] initialData = new Vector3[numNodes];

        System.Array.Copy(vertices, initialData, vertices.Length);

        // Guardar los datos en el archivo
        SaveVector3ArrayToFile(initialData);
        Debug.Log("Archivo creado con datos iniciales.");
    }


    // Método para guardar un array de Vector3 en un archivo JSON
    public void SaveVector3ArrayToFile(Vector3[] vertices)
    {
        // Convertir el array a JSON
        string json = JsonUtility.ToJson(new Wrapper2 { array = vertices }, true);

        // Escribir el JSON en el archivo
        File.WriteAllText(filePath, json);
        Debug.Log($"Array guardado en: {filePath}");
    }













    //CANVAS
    //MODIFICACIONES A LAS VARIABLES EN FUNCIÓN DEL CANVAS   

    //variables temporales del canvas

    private int tempInitPosX;
    private int tempInitPosY;
    private int tempInitPosZ;

    private int tempSizeX;
    private int tempSizeZ;
    private float tempMeshScale;

    private float tempSpringConstant;
    private float tempDampingConstant;

    private float tempMeshStaticFriction;
    private float tempMeshDynamicFriction;
    private float tempMeshBounciness;

    private float tempColliderRadius;

    //se cambia la posición original o punto de origen del mesh, ejes horizontales X y Z, vertical Y
    public void UpdateInitialPosX(int newValue)
    {
        tempInitPosX = newValue;
        Debug.Log("Se ha cambiado la variable initPosX exitosamente. Nuevo valor : " + tempInitPosX);
    }


    public void UpdateInitialPosZ(int newValue)
    {
        tempInitPosZ = newValue;
        Debug.Log("Se ha cambiado la variable initPosZ exitosamente. Nuevo valor : " + tempInitPosZ);
    }


    public void UpdateInitialPosY(int newValue)
    {
        tempInitPosY = newValue;
        Debug.Log("Se ha cambiado la variable initPosY exitosamente. Nuevo valor : " + initPosY);
    }


    public void UpdateXsizeConst(int newValue)
    {
        tempSizeX = newValue;
        Debug.Log("Se ha cambiado la variable xSize exitosamente. Nuevo valor : " + tempSizeX);
    }


    public void UpdateZsizeConst(int newValue)
    {
        tempSizeZ = newValue;
        Debug.Log("Se ha cambiado la variable zSize exitosamente. Nuevo valor : " + tempSizeZ);
    }


    public void UpdateScaleConst(float newValue)
    {
        tempMeshScale = newValue;
        Debug.Log("Se ha cambiado la variable MeshScale exitosamente. Nuevo valor : " + tempMeshScale);
    }


    public void UpdateSpringConst(float newValue)
    {
        tempSpringConstant = newValue;
        Debug.Log("Se ha cambiado la variable springConstant exitosamente. Nuevo valor : " + tempSpringConstant);
    }


    public void UpdateDampConst(float newValue)
    {
        tempDampingConstant = newValue;
        Debug.Log("Se ha cambiado la variable sampingConstant exitosamente. Nuevo valor : " + tempDampingConstant);
    }


    public void UpdateStaticConstant(float newValue)
    {
        tempMeshStaticFriction = newValue;
        Debug.Log("Se ha cambiado la variable MeshStaticFriction exitosamente. Nuevo valor : " + tempMeshStaticFriction);
    }


    public void UpdateDynamicConstant(float newValue)
    {
        tempMeshDynamicFriction = newValue;
        Debug.Log("Se ha cambiado la variable MeshDynamicFriction exitosamente. Nuevo valor : " + tempMeshDynamicFriction);
    }


    public void UpdateBounciness(float newValue)
    {
        tempMeshBounciness = newValue;
        Debug.Log("Se ha cambiado la variable MeshBounciness exitosamente. Nuevo valor : " + tempMeshBounciness);
    }

    
    public void UpdateColliderRadius(float newValue)  
    {
        if (newValue <= maxColliderRadius && newValue >= minColliderRadius)
        {
            tempColliderRadius = newValue;
            canvas_1.CollRadSlider.value = newValue; // Actualiza el Slider visualmente
            Debug.Log("Se ha cambiado la variable colliderRadius exitosamente. Nuevo valor : " + tempColliderRadius);
        }
        else
        {
            Debug.Log("VALOR DEL RADIO DE COLISIONADOR NO VÁLIDO");
        }
        
    }

    public void UpdateSliderValue(float newValue) 
    {
        tempColliderRadius = newValue;
        
        Debug.Log("Se ha cambiado la variable (slider) colliderRadius exitosamente. Nuevo valor : " + tempColliderRadius);
    }

//activa/desactiva la capa falsa Mesh0
public void UpdateAllowMesh0(bool isOn)
    {
        AllowMesh0 = isOn;
        Debug.Log("Se ha activado/desactivado la capa false de la malla exitosamente. Nuevo valor : " + AllowMesh0); //cambiar más tarde el contenido en función bool
    }

    public void RestartMesh()
    {
        //se asignan todos los nuevos valores temporales a los valores del programa

        initPosX = tempInitPosX;
        initPosY = tempInitPosY;
        initPosZ = tempInitPosZ;

        xSize = tempSizeX;
        zSize = tempSizeZ;
        MeshScale = tempMeshScale;

        springConstant = tempSpringConstant;
        dampingConstant = tempDampingConstant;

        MeshStaticFriction = tempMeshStaticFriction;
        MeshDynamicFriction = tempMeshDynamicFriction;
        MeshBounciness = tempMeshBounciness;

        colliderRadius = tempColliderRadius;

        float rad = MeshScale / 2;
        minColliderRadius = rad / 3;
        maxColliderRadius = rad * 4;
        
        // Inicializa el rango y el valor inicial del slider
        canvas_1.UpdateSliderRange(minColliderRadius, maxColliderRadius);
        //si el valor del radio del colisionador no entra dendtro de las especificaciones, coje la mitad del valor de la distancia entre nodos como referencia
        if(colliderRadius > maxColliderRadius || colliderRadius < minColliderRadius)
        {
            
            canvas_1.UpdateCurrentValue(colliderRadius);
            colliderRadius = rad * 2;
            canvas_1.CollRadSlider.value = colliderRadius; // Actualiza el Slider visualmente
            canvas_1.ColliderRad.text = rad.ToString("F2");
        }

        //te dice que tamaño y numero de nodos hay
        numNodes = (xSize + 1) * (zSize + 1);
        canvas_1.GridInfo.text = $"(Size Mesh {xSize * MeshScale}X{zSize * MeshScale}, number of nodes {numNodes})";

        restartMesh = true;
    }


}//FIN DEL PROGRAMA



// Clase auxiliar para serializar/deserializar un array de Vector3
[System.Serializable]
public class Wrapper2
{
    public Vector3[] array;
}



