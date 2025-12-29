using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]


public class MeshDeformation_3_MultiCapa : MonoBehaviour
{

    public Material myMaterial;

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

    public float node_mass = 0.001f;
    //---------------------------------------------------------------
    //PROPIEDADES MATERIAL MESH PARA TODOS LOS COLLIDERS
    // Crear un nuevo material físico para el mesh
    PhysicMaterial ElasticMeshMaterial;

    public float MeshDynamicFriction = 0.0f;    //friccion dinamica, valores sugeridos para piel humana entre 0.3-0.5
    public float MeshStaticFriction = 0.0f;     //friccion estatica, valores sugeridos para piel humana entre 0.2-0.4
    public float MeshBounciness = 0.0f;         //rebote, valores sugeridos para piel humana entre 0.0-0.1


    //---------------------------------------------------------------
    //VARIABLES MUELLE-AMORTIGUADOR
    public float springConstantHorizontal = 1.2f;  //constante del muelle horizontal
    public float dampingConstantHorizontal = 0.6f;  //constante del amortiguador horizontal

    public float springConstantVertical = 1.2f;  //constante del muelle Vertical
    public float dampingConstantVertical = 0.6f;  //constante del amortiguador Vertical

    public bool enableGravityMesh;

    public float minDistanceSpring = 0.0f;
    public float maxDistanceSpring = 0.0f;

  

    //VARIABLES MULTICAPA
    public int num_layers = 1;
    private int num_nodes_per_layer = 0;
    public bool freezeLastLayer;

    public bool ManualDistLayer = false;
    public float DistanceLayer = 1.0f;
    public float springIncrementation = 0.0f;
    public float dampingIncrementation = 0.0f;

    // Diccionario para almacenar datos de mallas
    Dictionary<string, (Vector3[], int[], Color[],GameObject[], Mesh)> meshDataDict = new Dictionary<string, (Vector3[], int[], Color[],GameObject[], Mesh)>();

    // Definir los vértices, triángulos y colores
    Vector3[] LayerVertices;
    int[] LayerTriangles;
    Color[] LayerColors;
    private Mesh LayerMesh;
    private GameObject[] LayerObjectVector; // Vector para almacenar los objetos 

    //VARIABLES COLLIDER

    public float colliderRadius = 0.0f;
    private float maxColliderRadius = 0.0f;
    private float minColliderRadius = 0.0f;

    //variables canvas
    private bool restartMesh = false;
    public Canvas_3 canvas_3;

    //variables capa fantasma o capa 0
    public bool AllowMesh0 = true;
    public Material Mesh0Material;

    Vector3[] Mesh0Vertices;
    int[] Mesh0Triangles;
    Color[] Mesh0Colors;
    Mesh Mesh0;

    private int numNodesLayer = 0;

    // Start is called before the first frame update
    void Start()
    {

        if (AllowMesh0 == true)
        {
            CreateMesh0();
        }
        Mesh0Vertices = new Vector3[(xSize + 1) * (zSize + 1)]; //se inicializan los 2 vectores para que no cause problemas
        Mesh0Triangles = new int[xSize * zSize * 6];

        //funciones par controlar el radio del colisionador

        //obtiene los valores min y max del radio del colsionador a partir de la distancia entre nodos
        //***modificar más tarde cuando se modifique caso escala/resolución, calculo de manera temporal, luego modificar

        float rad = MeshScale / 2;
        minColliderRadius = rad / 3;
        maxColliderRadius = rad * 4;
        colliderRadius = rad * 2;
        // Inicializa el rango y el valor inicial del slider
        canvas_3.UpdateSliderRange(minColliderRadius, maxColliderRadius);
        canvas_3.UpdateCurrentValue(colliderRadius);

        //te dice que tamaño y numero de nodos hay
        int numNodes = (xSize + 1) * (zSize + 1) * num_layers;
        canvas_3.GridInfo.text = $"(Size Mesh {xSize * MeshScale}X{zSize * MeshScale}, number of nodes {numNodes})";

        //numero nodos de 1 capa
        numNodesLayer = (xSize + 1) * (zSize + 1);

        //se inicializan las variables temporales del canvas para que no haya problemas al reiniciar el mesh
        tempInitPosX = initPosX;
        tempInitPosY = initPosY;
        tempInitPosZ = initPosZ;

        tempSizeX = xSize;
        tempSizeZ = zSize;
        tempMeshScale = MeshScale;

        tempHorSpringConstant = springConstantHorizontal;
        tempVertSpringConstant = springConstantVertical;
        tempIncrSpringConstant = springIncrementation;

        tempHorDampingConstant = dampingConstantHorizontal;
        tempVertDampingConstant = dampingConstantVertical;
        tempIncrDampingConstant = dampingIncrementation;

        tempMeshStaticFriction = MeshStaticFriction;
        tempMeshDynamicFriction = MeshDynamicFriction;
        tempMeshBounciness = MeshBounciness;

        tempColliderRadius = colliderRadius;

        tempNumLayers = num_layers;

        tempManualDistLayer = ManualDistLayer;
        tempDistanceLayers = DistanceLayer;

        tempFreezeLastLayer = freezeLastLayer;


        // Crear un nuevo material físico para el mesh
        ElasticMeshMaterial = new PhysicMaterial();

        //creacion mesh
        CreateShape();
        UpdateLayerMesh();

    }

    //funcion inicial para crea el mesh e incorporarle todas las funciones fisicas de este
    void CreateShape()
    {


        num_nodes_per_layer = (xSize + 1) * (zSize + 1);

        // Por ejemplo, inicializar 3 capas
        for (int i = 1; i <= num_layers; i++)
        {
            string dict_name = "Mesh_" + i;
            // Crear y almacenar mallas
            CreateMesh(dict_name, num_nodes_per_layer);

        }

        // Definir las propiedades del material para el mesh
        ElasticMeshMaterial.dynamicFriction = MeshDynamicFriction; //friccion dinamica
        ElasticMeshMaterial.staticFriction = MeshStaticFriction;  //friccion estatica
        ElasticMeshMaterial.bounciness = MeshBounciness;  // Qué tan "rebotante" es el objeto

        ElasticMeshMaterial.frictionCombine = PhysicMaterialCombine.Average;  // Cómo combinar la fricción con otros objetos
        ElasticMeshMaterial.bounceCombine = PhysicMaterialCombine.Maximum;  // Cómo combinar el rebote

        CreateLayerVertices();
        CreateLayerSpringNodes();
        CreateInterLayerSpringNodes();
        AvoidCollision();
        CreateLayerTriangles();
        AssignLayerColors();
        
    }



    //---------------------------------------------------------------------------------

    //CODIGOS PARA MESH MULTICAPA

    //Inicializa el diccionario para organizar el mesh multicapa para diferentes "capas" del diccionario
    void CreateMesh(string meshName, int num_nodes)
    {
        // Definir los vértices, triángulos y colores
        LayerVertices = new Vector3[num_nodes];
        LayerTriangles = new int[xSize * zSize * 6];
        LayerColors = new Color[num_nodes];
        LayerObjectVector = new GameObject[num_nodes];

        // Crear una nueva malla
        LayerMesh = new Mesh();
        // Asignar los vértices y triángulos a la malla
        // Inicializar los vértices a (0,0,0) o a un valor específico si es necesario
        for (int i = 0; i < LayerVertices.Length; i++)
        {
            LayerVertices[i] = Vector3.zero; // O cualquier otro valor predeterminado
            LayerColors[i] = Color.white; // O cualquier otro color predeterminado
        }

        // Inicializar los triángulos, por ejemplo, en cero o en un patrón específico
        for (int i = 0; i < LayerTriangles.Length; i++)
        {
            LayerTriangles[i] = 0; // O define una lógica para configurar triángulos válidos
        }


        // Agregar la malla y su información al diccionario
        meshDataDict[meshName] = (LayerVertices, LayerTriangles, LayerColors, LayerObjectVector, LayerMesh);

        // Si deseas crear un GameObject para esta malla, puedes hacerlo aquí
        GameObject meshObject = new GameObject(meshName);
        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        meshFilter.mesh = LayerMesh;
        meshObject.AddComponent<MeshRenderer>();
        LayerMesh.MarkDynamic(); //Indica que la malla se actualizará constantemente

        // Cargar el material desde la carpeta Assets y añadirlo al mesh, si no se añade, sera imposible colorarlo mas tarde
        //Material myMaterial = Resources.Load<Material>("Materials/TerrainMaterial1");
        meshObject.GetComponent<Renderer>().material = myMaterial;

        // Emparentar el objeto al mesh para que sea parte de su jerarquía, sin que cambie su posicion
        meshObject.transform.SetParent(this.transform, false);
    }


    //CURIOSIDAD A COMENTAR A ALBERTO, SOLO PERMITE UNA ESCALA DE 2! (EXPONENCIAL), SI NO PARTE DE LOS MUELLES NO LOS COLOCA
    //SIGUE HABIENDO PROBLEMAS CON LAS ESCALAS CON POSICIONES INICIALES DIFERENTES A 0
    //crea los muelles entre los nodos
    //me calcula donde poner cada vertice, el cual se guarda en la matriz TerrainHeight y se va implementando uno a uno
    void CreateLayerVertices()
    {
        float y;
        if (AllowMesh0 == true) Mesh0Vertices = new Vector3[(xSize + 1) * (zSize + 1)]; //permitir nodos de la capa fantasma
        for (int l = 1; l <= num_layers; l++)
        {
            //maxTerrainHeight = initPosY;  //me soluciona el problema de colores con máx. y min. preestablecidos, MODIFICAR MAS TARDE
            string meshName = "Mesh_" + l;
            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {
                // Acceder al array de vértices
                LayerVertices = meshData.Item1;
                LayerTriangles = meshData.Item2;
                LayerColors = meshData.Item3;
                LayerObjectVector = meshData.Item4;



                for (int i = 0, z = initPosZ; z <= zSize + initPosZ; z++)  //ejes x y z horizontales, eje y vertical
                {
                    for (int x = initPosX; x <= xSize + initPosX; x++)
                    {
                        //y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;

                        //para controlar manualmente o automaticamente el espacio entre capas
                        if (ManualDistLayer == false)
                        {
                            y = initPosY - ((l - 1) * colliderRadius * MeshScale);  //CAMBIAR MÁS TARDE A OTROS VALORES MAS ADECUADOS,
                        }
                        else
                        {
                            y = initPosY - (l - 1) * DistanceLayer * MeshScale;
                        }


                        //CREA OBJETOS VACIOS Y LES PROPORCIONA "MASA"

                        // Definir la posición del nodo
                        Vector3 nodePosition = new Vector3(initPosX + (x - initPosX) * MeshScale, initPosY + (y - initPosY) * MeshScale,
                                                    initPosZ + (z - initPosZ) * MeshScale);

                        // Guardar la posición en los vértices de la malla
                        LayerVertices[i] = new Vector3(nodePosition.x - transform.position.x * 2, nodePosition.y - transform.position.y * 2, nodePosition.z - transform.position.z * 2);

                        //crear nodos capa fantasma/false
                        if (AllowMesh0 == true && l == 1)
                        {
                            Mesh0Vertices[i] = nodePosition;
                            Mesh0Vertices[i].y = y + colliderRadius;
                        }

                        // Crear un nuevo GameObject vacío
                        GameObject emptyObject = new GameObject("Layer_" + l +"_Node_" + i);

                        // Opcional: Asignar la posición inicial del objeto
                        emptyObject.transform.position = nodePosition; // nodePosition; // new Vector3(x, y, z)/MeshScale;
                                                                       // Guardar el objeto en el vector
                        LayerObjectVector[i] = emptyObject;

                        // Emparentar el objeto al mesh para que sea parte de su jerarquía, sin que cambie su posicion
                        emptyObject.transform.SetParent(meshLayer.transform, false);

                        
                        //añade un rigidbody al objeto vacio
                        Rigidbody rb = emptyObject.AddComponent<Rigidbody>();

                        // Comprobar las posiciones X y Z
                        if (emptyObject.transform.position.x == initPosX || emptyObject.transform.position.x == (xSize / MeshScale) + initPosX
                            || emptyObject.transform.position.z == initPosZ || emptyObject.transform.position.z == (zSize / MeshScale) + initPosZ)

                        {
                            // Congelar todas las restricciones de movimiento y rotación
                            //rb.constraints = RigidbodyConstraints.FreezePosition;
                            rb.constraints = RigidbodyConstraints.FreezeAll;
                        }
                        else
                        {
                            // Si no se cumple la condición, se dejan libres
                            //rb.constraints = RigidbodyConstraints.None;

                            //rb.constraints = RigidbodyConstraints.FreezeAll; // eliminar mas tarde
                            //congela las rotaciones de los nodos
                            //rb.constraints = RigidbodyConstraints.FreezeRotation; //si se habilita esa linea peta las fisicas de Unity
                        }

                        //me congela la ultima capa del mesh para que sea solida, se pede comentar en cualquier momento
                        //se utiliza como superficie rigida final 
                        if(l == num_layers && freezeLastLayer == true)
                        {
                            rb.constraints = RigidbodyConstraints.FreezeAll; //hace que la ultima capa sea rigida
                        }

                        // Cambia la masa del Rigidbody a 5
                        rb.mass = node_mass;  //masa manta 0.01
                        rb.isKinematic = false;
                        rb.interpolation = RigidbodyInterpolation.None; // Opciones: None, Interpolate, Extrapolate
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                        //se le quita la gravedad al muelle POR VER SI ES CORRECTO
                        rb.useGravity = enableGravityMesh;
                        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                        
                        //añade un SphereCollider a la primera capa
                        //if (l == 1)
                        //{
                            SphereCollider sphereCollider = emptyObject.AddComponent<SphereCollider>();
                            // Ajustar el radio del SphereCollider
                            sphereCollider.radius = colliderRadius;
                            // Hacer el collider un trigger si es necesario
                            sphereCollider.isTrigger = false;  // Cambia a true si necesitas que sea un trigger

                            // Asignar el material al collider
                            sphereCollider.material = ElasticMeshMaterial;
                        //}

                        

                        //MODIFICAR MAS TARDE
                        if (l == 1)
                        {
                            if (y > maxTerrainHeight)
                                maxTerrainHeight = y;
                            if (y < minTerrainHeight)
                                minTerrainHeight = y;
                        }
                       
                        
                        i++;
                    }
                }

            }
            // Si deseas actualizar la malla en el diccionario, crea una nueva malla
            LayerMesh = new Mesh();
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles; // Mantener los triángulos originales
            LayerMesh.colors = LayerColors; // Mantener los colores originales

            // Actualizar el diccionario con la nueva malla
            meshDataDict[meshName] = (LayerVertices, meshData.Item2, meshData.Item3, LayerObjectVector, LayerMesh);

        }//fin for multilayer
    }//fin create vertices


    //avoid collision para todas las capas suponiendo que todas tengan colliders
    void AvoidCollision()
    {
        AvoidSameLayerCollisionFunction(); //llama al la funcion para ignorar las colisiones entre colliders de la misma capa

        GameObject[] LayerObjectVector1;
        GameObject[] LayerObjectVector2;

        for (int l = 1; l < num_layers; l++)
        {

            for(int subl = l + 1; subl <= num_layers; subl++)
            {

                string meshName1 = "Mesh_" + l;
                string meshName2 = "Mesh_" + subl;

                //Debug.Log("se ignora capa " + l + "mas capa " + subl);
                // Intentar encontrar el GameObject correpsondiente
                GameObject meshLayer1 = GameObject.Find(meshName1);
                GameObject meshLayer2 = GameObject.Find(meshName2);

                // Intentar acceder a los datos de la malla en el diccionario
                if (meshDataDict.TryGetValue(meshName1, out var meshData1) && meshDataDict.TryGetValue(meshName2, out var meshData2))
                {
                    

                    // Acceder al array de vértices del layer superior
                    LayerObjectVector1 = meshData1.Item4;

                    LayerObjectVector2 = meshData2.Item4;

                    IgnoreCollisionsBetweenGroups(LayerObjectVector1, LayerObjectVector2); //llama a la funcion para que se ignoren las colisiones entre capas

                }//end if

            } //end for subl

        }//end for l


    }//end ignore collision



    //utilizo esta función para que los colliders de la malla se ignoren entre si, para que el funcionamiento de la malla sea mas optimo y
    //no se malgaste tanta potencia computacional en calcular los choques entre los empty objects de la malla
    void AvoidSameLayerCollisionFunction()
    {
        for (int l = 1; l <= num_layers; l++)
        {

            string meshName = "Mesh_" + l; //de momento solo para la capa 1
                                           // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {
                for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
                {
                    // Acceder al array de vértices
                    LayerVertices = meshData.Item1;
                    LayerTriangles = meshData.Item2;
                    LayerColors = meshData.Item3;
                    LayerObjectVector = meshData.Item4;

                    //cojo el primer empty object para vincularlo al resto
                    GameObject fistObject = LayerObjectVector[i];
                    Collider colliderA = fistObject.GetComponent<Collider>();

                    for (int j = i + 1; j < (xSize + 1) * (zSize + 1); j++)
                    {
                        //cojo el segundo empty object para completar la funcion ignore collision
                        GameObject secondObject = LayerObjectVector[j];
                        Collider colliderB = LayerObjectVector[j].GetComponent<Collider>();

                        // Asegúrate de que ambos objetos tienen un collider
                        if (colliderA != null && colliderB != null)
                        {
                            Physics.IgnoreCollision(colliderA, colliderB);  //hago que 2 colliders se ignoren entre si
                                                                            //Debug.Log("Se cumple funcion avoid");
                        }
                    }
                } //fin for i
            }//fin if
            LayerMesh = new Mesh();
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles; // Mantener los triángulos originales
            LayerMesh.colors = LayerColors; // Mantener los colores originales
                                            //if (LayerColors == null) Debug.Log("error");
                                            // Actualizar el diccionario con la nueva malla
            meshDataDict[meshName] = (meshData.Item1, meshData.Item2, meshData.Item3, LayerObjectVector, LayerMesh);
        }//end for l
    }//end avoid colliders de la misma capa


    //ignora las colisiones entre grupos diferentes
    void IgnoreCollisionsBetweenGroups(GameObject[] groupA, GameObject[] groupB)
    {
        foreach (GameObject objA in groupA)
        {
            
            Collider colliderA = objA.GetComponent<Collider>();
            if (colliderA == null) continue; // Saltar si el objeto no tiene collider
            
            foreach (GameObject objB in groupB)
            {
                Collider colliderB = objB.GetComponent<Collider>();
                if (colliderB == null) continue; // Saltar si el objeto no tiene collider
                //Debug.Log("Llega hasta aqui " + colliderA + " col b" + colliderB);
                Physics.IgnoreCollision(colliderA, colliderB);
                
            }
        }
    }



    //me crea los triangulos del mesh en función de donde estan situados los vertices
    void CreateLayerTriangles()
    {
        if (AllowMesh0 == true) Mesh0Triangles = new int[xSize * zSize * 6]; //permitir triangles de la capa fantasma

        for (int l = 1; l <= num_layers; l++)
        {
            //maxTerrainHeight = initPosY;  //me soluciona el problema de colores con máx. y min. preestablecidos, MODIFICAR MAS TARDE
            string meshName = "Mesh_" + l;
            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {
                // Acceder al array de vértices
                LayerVertices = meshData.Item1;
                LayerTriangles = meshData.Item2;
                LayerColors = meshData.Item3;
                LayerObjectVector = meshData.Item4;


                int vert = 0;
                int tris = 0;
                for (int z = 0 + initPosZ; z < zSize + initPosZ; z++)
                {
                    for (int x = 0 + initPosX; x < xSize + initPosX; x++)
                    {
                        LayerTriangles[tris + 0] = vert + 0;
                        LayerTriangles[tris + 1] = vert + xSize + 1;
                        LayerTriangles[tris + 2] = vert + 1;
                        LayerTriangles[tris + 3] = vert + 1;
                        LayerTriangles[tris + 4] = vert + xSize + 1;
                        LayerTriangles[tris + 5] = vert + xSize + 2;

                        vert++;
                        tris += 6;
                    }
                    vert++;
                }
                if (AllowMesh0 == true && l == 1) Mesh0Triangles = LayerTriangles;  //copiar los trinagulos de la capa fantasma
                // Si deseas actualizar la malla en el diccionario, crea una nueva malla
            }//fin if


            LayerMesh = new Mesh();
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles; // Mantener los triángulos originales
            LayerMesh.colors = LayerColors; // Mantener los colores originales

            // Actualizar el diccionario con la nueva malla
            meshDataDict[meshName] = (meshData.Item1, LayerTriangles, meshData.Item3, meshData.Item4, LayerMesh);
            
        }
    } // fin create layer triangles

    //Me asigna un gradiente de colores en el mesh en fucnión de su deformación
    void AssignLayerColors()
    {
        float height = 0;
        Mesh0Colors = new Color[LayerVertices.Length];  //permitir colores en la capa fantasma
        for (int l = 1; l <= num_layers; l++)
        {
            
            string meshName = "Mesh_" + l;
            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {

                // Acceder al array de vértices
                LayerVertices = meshData.Item1;
                LayerTriangles = meshData.Item2;
                LayerColors = meshData.Item3;
                LayerObjectVector = meshData.Item4;


                for (int i = 0, z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++)
                    {
                        height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, LayerVertices[i].y);
                        LayerColors[i] = gradient.Evaluate(height);

                        if (AllowMesh0 == true && l == 1)
                        {
                            Mesh0Colors[i] = gradient.Evaluate(height);
                        }

                        i++;
                    }
                }
            }//fin if

            LayerMesh = new Mesh();
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles; // Mantener los triángulos originales
            LayerMesh.colors = LayerColors; // Mantener los colores originales
            //if (LayerColors == null) Debug.Log("error");
            // Actualizar el diccionario con la nueva malla
            meshDataDict[meshName] = (meshData.Item1, meshData.Item2, LayerColors, meshData.Item4, LayerMesh);

        }


    }


    // Update Mesh, me actualiza el mesh a tiempo real por si hay cambios
    void UpdateLayerMesh()
    {
        for (int l = 1; l <= num_layers; l++)
        {
          
            string meshName = "Mesh_" + l;
            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            MeshFilter meshFilter = meshLayer.GetComponent<MeshFilter>();

            if(meshLayer == null) { Debug.Log("Hay un error"); }

            // Intenta acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {
                // Acceder al array de vértices
                LayerVertices = meshData.Item1;
                LayerTriangles = meshData.Item2;
                LayerColors = meshData.Item3;
                LayerObjectVector = meshData.Item4;
            }
            
            // Obtener el mesh correspondiente desde el diccionario
            LayerMesh = meshFilter.mesh;

            LayerMesh.Clear();

            // Asigna los nuevos datos
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles;
            LayerMesh.colors = LayerColors;
            

            LayerMesh.RecalculateNormals();
            LayerMesh.RecalculateBounds();

            if (AllowMesh0 == true && Mesh0 != null && l == 1)
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


            // Actualizar el diccionario con la nueva malla
            //meshDataDict[meshName] = (meshData.Item1, meshData.Item2, meshData.Item3, LayerMesh);

        }
    }// fin update mesh

    //Actualiza la posición de los nodos del mesh con los empty object creados, para que así, los nodos se
    ////muevan con los gameobjects y en consecuencia se mueva el mesh
    void AssignNewLayerVertices()
    {
        float x;
        float y;
        float z;

        for (int l = 1; l <= num_layers; l++)
        {

            string meshName = "Mesh_" + l;
            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {

                // Acceder al array de vértices
                LayerVertices = meshData.Item1;
                LayerTriangles = meshData.Item2;
                LayerColors = meshData.Item3;
                LayerObjectVector = meshData.Item4;

                //Debug.Log("Actualizando");
                for (int i = 0; i < (xSize + 1) * (zSize + 1); i++)
                {
                    GameObject obj = LayerObjectVector[i];
                    if (obj != null)
                    {
                        x = obj.transform.position.x;
                        y = obj.transform.position.y;
                        z = obj.transform.position.z;

                        // Definir la posición del nodo
                        Vector3 nodePosition = new Vector3(x, y, z);

                        // Guardar la posición en los vértices de la malla
                        LayerVertices[i] = new Vector3(nodePosition.x - transform.position.x * 2, nodePosition.y - transform.position.y * 2, nodePosition.z - transform.position.z * 2);

                        //modificar nodos capa fantasma/false
                        if (AllowMesh0 == true && l == 1 && Mesh0 != null)
                        {
                            Mesh0Vertices[i] = nodePosition;
                            Mesh0Vertices[i].y = y + colliderRadius;
                        }

                        if (y > maxTerrainHeight)
                            maxTerrainHeight = y;
                        if (y < minTerrainHeight)
                            minTerrainHeight = y;

                    }
                }

            } //end for i
            LayerMesh = new Mesh();
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles; // Mantener los triángulos originales
            LayerMesh.colors = LayerColors; // Mantener los colores originales
            
            // Actualizar el diccionario con la nueva malla
            meshDataDict[meshName] = (LayerVertices, meshData.Item2, LayerColors, meshData.Item4, LayerMesh);

        }//end for layer
    }//fin AssignNewLayerVertices


    void Update()
    {
        if (restartMesh == true) //me comprueba si se quiere reiniciar el mesh al cambiar las variables de este, recomendable hacerlo sin objetos en contacto
        {


            //elimina todos los elementos del diccionario y los game objects para crearlos de nuevo al regenerar el mesh. Regenerar sin borrar lo anterior dara
            //muchos problemas y es posible que no funcione, ya que no los gameObjects y sus posiciones no se controlan por codigo, y si se reinicia el codigo sin 
            //borrar hará un duplicado mal hecho
            // Código para limpiar todos los diccionarios y destruir sus GameObjects

            DestroyEmptyObjectsAndClearDictionaries();
            AssignTempVariables();
            Debug.Log("SE REINICIA EL MESH");

            //creacion mesh
            CreateShape();
            UpdateLayerMesh();

            restartMesh = false;
        }
        else
        {
            AssignNewLayerVertices();
            CreateLayerTriangles();
            AssignLayerColors();
            UpdateLayerMesh();
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


    }//fin update



    //me crea los muelles en las posiciones horizontales
    void CreateLayerSpringNodes()
    {
        float SizeVertices = 1 / MeshScale;
        float xSizeVertices = xSize / MeshScale;
        float zSizeVertices = zSize / MeshScale;
        for (int l = 1; l <= num_layers; l++)
        {
            string meshName = "Mesh_" + l;
            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer = GameObject.Find(meshName);
            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName, out var meshData))
            {

                // Acceder al array de vértices
                LayerVertices = meshData.Item1;
                LayerTriangles = meshData.Item2;
                LayerColors = meshData.Item3;
                LayerObjectVector = meshData.Item4;


                for (int i = 0; i < num_nodes_per_layer; i++)
                {
                    GameObject obj = LayerObjectVector[i];
                    if (obj != null)
                    {


                        //condiciones spring joint 1 (X-1, Z+1), posicion vector i + xSize
                        if (i > 0 && i % (xSize + 1) != 0 && i < numNodesLayer - (xSize + 1))

                        {
                            Debug.Log("OK 1, Mesh Layer " + l + " , hay " + (numNodesLayer - xSize - zSize - 1));

                            // Obtiene los dos primeros objetos vacíos
                            GameObject firstObject = LayerObjectVector[i];
                            GameObject secondObject = LayerObjectVector[i + xSize];

                            ConfigureSpringNodes(firstObject, secondObject, springConstantHorizontal * (float)Math.Sqrt(2), dampingConstantHorizontal * (float)Math.Sqrt(2));

                        }

                        //condiciones spring joint 2 (X=, Z+1) , posicion vector i + xSize + 1
                        if (i < numNodesLayer - (xSize + 1))
                        {
                            Debug.Log("OK 2, Mesh Layer " + l + " , hay " + (numNodesLayer - xSize - 1));

                            // Obtiene los dos primeros objetos vacíos
                            GameObject firstObject = LayerObjectVector[i];
                            GameObject secondObject = LayerObjectVector[i + xSize + 1];

                            ConfigureSpringNodes(firstObject, secondObject, springConstantHorizontal, dampingConstantHorizontal);
                        }

                        //condiciones spring joint 3 (X+1, Z+1) , posicion vector i + xSize + 2
                        if (i < numNodesLayer - (xSize + 1) && (i + 1) % (xSize + 1) != 0)
                        {
                            Debug.Log("OK 3, Mesh Layer " + l + " , hay " + (numNodesLayer - xSize - zSize - 1));

                            // Obtiene los dos primeros objetos vacíos
                            GameObject firstObject = LayerObjectVector[i];
                            GameObject secondObject = LayerObjectVector[i + xSize + 2];

                            ConfigureSpringNodes(firstObject, secondObject, springConstantHorizontal * (float)Math.Sqrt(2), dampingConstantHorizontal * (float)Math.Sqrt(2));
                        }

                        //condiciones spring joint 4 (X+1, Z=) , posicion vector i + 1
                        if ((i + 1) % (xSize + 1) != 0)
                        {
                            Debug.Log("OK 4, Mesh Layer " + l + " , hay " + (numNodesLayer - zSize - 1));

                            // Obtiene los dos primeros objetos vacíos
                            GameObject firstObject = LayerObjectVector[i];
                            GameObject secondObject = LayerObjectVector[i + 1];

                            ConfigureSpringNodes(firstObject, secondObject, springConstantHorizontal, dampingConstantHorizontal);
                        }

                    }

                } //end for



            }//end if

            LayerMesh = new Mesh();
            LayerMesh.vertices = LayerVertices;
            LayerMesh.triangles = LayerTriangles; // Mantener los triángulos originales
            LayerMesh.colors = LayerColors; // Mantener los colores originales
            //if (LayerColors == null) Debug.Log("error");
            // Actualizar el diccionario con la nueva malla
            meshDataDict[meshName] = (meshData.Item1, meshData.Item2, meshData.Item3, LayerObjectVector, LayerMesh);

        }//end for layer

    }//fin CreateSpringNodes

    //me crea los muelles en las posiciones horizontales
    //SUGERENCIAS : HACER QUE EL VALOR DEL MUELLE SE INCREMENTE CONFORME SE VA A CAPAS MAS PROFUNDAS
    void CreateInterLayerSpringNodes()
    {
        Vector3[] LayerVertices1;
        int[] LayerTriangles1;
        Color[] LayerColors1;
        GameObject[] LayerObjectVector1;

        Vector3[] LayerVertices2;
        int[] LayerTriangles2;
        Color[] LayerColors2;
        GameObject[] LayerObjectVector2;

        for (int l = 2; l <= num_layers; l++)
        {
            string meshName1 = "Mesh_" + (l - 1);
            string meshName2 = "Mesh_" + l;

            // Intentar encontrar el GameObject correpsondiente
            GameObject meshLayer1 = GameObject.Find(meshName1);
            GameObject meshLayer2 = GameObject.Find(meshName2);

            // Intentar acceder a los datos de la malla en el diccionario
            if (meshDataDict.TryGetValue(meshName1, out var meshData1) && meshDataDict.TryGetValue(meshName2, out var meshData2))
            {

                // Acceder al array de vértices del layer superior
                LayerVertices1 = meshData1.Item1;
                LayerTriangles1 = meshData1.Item2;
                LayerColors1 = meshData1.Item3;
                LayerObjectVector1 = meshData1.Item4;

                // Acceder al array de vértices del layer inferior
                LayerVertices2 = meshData2.Item1;
                LayerTriangles2 = meshData2.Item2;
                LayerColors2 = meshData2.Item3;
                LayerObjectVector2 = meshData2.Item4;

                for(int i = 0; i < num_nodes_per_layer; i ++)
                {
                    // Obtiene los dos primeros objetos vacíos
                    GameObject firstObject = LayerObjectVector1[i];
                    GameObject secondObject = LayerObjectVector2[i];

                    ConfigureSpringNodes(firstObject, secondObject, (springConstantVertical + (l-2)*springIncrementation), (dampingConstantVertical + (l-2)*dampingIncrementation));
                }

            }//fin if 
        }//fin for l
    }//fin 


    //crear los muelles horizontales, los datos del muelle basicamente
    void ConfigureSpringNodes(GameObject firstObject, GameObject secondObject, float SpringConstant, float dampingConstant)
    {
        // Crea un Spring Joint en el primer objeto
        SpringJoint springJoint = firstObject.AddComponent<SpringJoint>();
        springJoint.connectedBody = secondObject.GetComponent<Rigidbody>(); // Conecta el segundo objeto

        // Configura los parámetros del Spring Joint
        springJoint.autoConfigureConnectedAnchor = true; //Unity configura automaticamente los puntos de anclaje para mantener la distancia inicial entre ellos


        springJoint.spring = SpringConstant; // Fuerza del resorte
        springJoint.damper = dampingConstant; // Amortiguación
        springJoint.minDistance = minDistanceSpring; // Distancia mínima
        springJoint.maxDistance = maxDistanceSpring; // Distancia máxima
        springJoint.tolerance = 0.025f;

        springJoint.enableCollision = true;
        springJoint.enablePreprocessing = true;
        

    }


    //funcion para destruir todos los elementos del diccionario para regenerar el mesh despues, se encarga de destruir principalemnte los gameObjects y luego
    //destruir sus referencias
    void DestroyEmptyObjectsAndClearDictionaries()
    {
        for (int l = 1; l <= num_layers; l++)
        {
            //maxTerrainHeight = initPosY;  //me soluciona el problema de colores con máx. y min. preestablecidos, MODIFICAR MAS TARDE
            string meshName = "Mesh_" + l;
            // Referencias a los EmptyObjects que contienen los GameObjects de cada diccionario
            var layer = GameObject.Find(meshName);
            DestroyImmediate(layer);


            Debug.Log("Clear dict");
            // Limpia cada diccionario después de destruir los objetos
        }
        meshDataDict.Clear();
    }



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


    //CANVAS
    //MODIFICACIONES A LAS VARIABLES EN FUNCIÓN DEL CANVAS   

    //variables temporales del canvas

    private int tempInitPosX;
    private int tempInitPosY;
    private int tempInitPosZ;

    private int tempSizeX;
    private int tempSizeZ;
    private float tempMeshScale;

    private float tempHorSpringConstant;
    private float tempVertSpringConstant;
    private float tempIncrSpringConstant;

    private float tempHorDampingConstant;
    private float tempVertDampingConstant;
    private float tempIncrDampingConstant;

    private float tempMeshStaticFriction;
    private float tempMeshDynamicFriction;
    private float tempMeshBounciness;

    private float tempColliderRadius;

    private int tempNumLayers;

    private bool tempManualDistLayer;
    private float tempDistanceLayers;

    private bool tempFreezeLastLayer;

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
        Debug.Log("Se ha cambiado la variable initPosY exitosamente. Nuevo valor : " + tempInitPosY);
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


    public void UpdateHorSpringConst(float newValue)
    {
        tempHorSpringConstant = newValue;
        Debug.Log("Se ha cambiado la variable springConstantHorizontal exitosamente. Nuevo valor : " + tempHorSpringConstant);
    }

    public void UpdateVertSpringConst(float newValue)
    {
        tempVertSpringConstant = newValue;
        Debug.Log("Se ha cambiado la variable springConstantVertical exitosamente. Nuevo valor : " + tempVertSpringConstant);
    }

    public void UpdateIncrSpringConst(float newValue)
    {
        tempIncrSpringConstant = newValue;
        Debug.Log("Se ha cambiado la variable springIncrementation exitosamente. Nuevo valor : " + tempIncrSpringConstant);
    }



    public void UpdateHorDampConst(float newValue)
    {
        tempHorDampingConstant = newValue;
        Debug.Log("Se ha cambiado la variable dampingConstantHorizontal exitosamente. Nuevo valor : " + tempHorDampingConstant);
    }

    public void UpdateVertDampConst(float newValue)
    {
        tempVertDampingConstant = newValue;
        Debug.Log("Se ha cambiado la variable dampingConstantVertical exitosamente. Nuevo valor : " + tempVertDampingConstant);
    }

    public void UpdateIncrDampConst(float newValue)
    {
        tempIncrDampingConstant = newValue;
        Debug.Log("Se ha cambiado la variable dampingIncrementation exitosamente. Nuevo valor : " + tempIncrDampingConstant);
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
            canvas_3.CollRadSlider.value = newValue; // Actualiza el Slider visualmente
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

    public void UpdateNumLayers(int newValue)
    {
        tempNumLayers = newValue;
        Debug.Log("Se ha cambiado la variable num_layers exitosamente. Nuevo valor : " + tempNumLayers);
    }


    public void UpdateManualDistance(bool isOn)
    {
        tempManualDistLayer = isOn;
        Debug.Log("Se ha cambiado el modo distancia entre capas manual exitosamente. Nuevo valor : " + tempManualDistLayer);
    }


    public void UpdateDistanceLayers(float newValue)
    {
        tempDistanceLayers = newValue;
        Debug.Log("Se ha cambiado la variable distanceLayers exitosamente. Nuevo valor : " + tempDistanceLayers);
    }

    public void UpdateFreezeLayer(bool isOn)
    {
        tempFreezeLastLayer = isOn;
        Debug.Log("Se ha congelado la útlima capa de la malla exitosamente. Nuevo valor : " + tempFreezeLastLayer); //cambiar más tarde el contenido en función bool
    }

    //activa/desactiva la capa falsa Mesh0
    public void UpdateAllowMesh0(bool isOn)
    {
        AllowMesh0 = isOn;
        Debug.Log("Se ha activado/desactivado la capa false de la malla exitosamente. Nuevo valor : " + AllowMesh0); //cambiar más tarde el contenido en función bool
    }

    //boton
    public void RestartMesh()
    {
        restartMesh = true;

    }

    //se asignan todos los nuevos valores temporales del canvas a los valores del programa para recrear el Mesh
    public void AssignTempVariables()
    {


        initPosX = tempInitPosX;
        initPosY = tempInitPosY;
        initPosZ = tempInitPosZ;

        xSize = tempSizeX;
        zSize = tempSizeZ;
        MeshScale = tempMeshScale;

        springConstantHorizontal = tempHorSpringConstant;
        springConstantVertical = tempVertSpringConstant;
        springIncrementation = tempIncrSpringConstant;

        dampingConstantHorizontal = tempHorDampingConstant;
        dampingConstantVertical = tempVertDampingConstant;
        dampingIncrementation = tempIncrDampingConstant;

        MeshStaticFriction = tempMeshStaticFriction;
        MeshDynamicFriction = tempMeshDynamicFriction;
        MeshBounciness = tempMeshBounciness;

        colliderRadius = tempColliderRadius;

        num_layers = tempNumLayers;

        ManualDistLayer = tempManualDistLayer;
        DistanceLayer = tempDistanceLayers;

        freezeLastLayer = tempFreezeLastLayer;


        float rad = MeshScale / 2;
        minColliderRadius = rad / 3;
        maxColliderRadius = rad * 4;

        // Inicializa el rango y el valor inicial del slider
        canvas_3.UpdateSliderRange(minColliderRadius, maxColliderRadius);
        //si el valor del radio del colisionador no entra dendtro de las especificaciones, coje la mitad del valor de la distancia entre nodos como referencia
        if (colliderRadius > maxColliderRadius || colliderRadius < minColliderRadius)
        {

            canvas_3.UpdateCurrentValue(colliderRadius);
            colliderRadius = rad * 2;
            canvas_3.CollRadSlider.value = colliderRadius; // Actualiza el Slider visualmente
            canvas_3.ColliderRad.text = rad.ToString("F2");
        }

        //te dice que tamaño y numero de nodos hay
        int numNodes = (xSize + 1) * (zSize + 1) * num_layers;
        canvas_3.GridInfo.text = $"(Size Mesh {xSize * MeshScale}X{zSize * MeshScale}, number of nodes {numNodes})";

    }




}//FIN DEL PROGRAMA

