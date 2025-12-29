using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canvas : MonoBehaviour
{

    private MeshGeneratorPhysics meshDeformationConfig;

    public TMP_InputField InitialPosX;
    public TMP_InputField InitialPosY;
    public TMP_InputField InitialPosZ;

    public TMP_InputField MeshSizeX;
    public TMP_InputField MeshSizeZ;
    public TMP_InputField Scale;

    public TMP_InputField SphereWeight;
    public TMP_InputField SprindConst;
    public TMP_InputField DampConst;


    void Start()
    {
        // Encuentra el script de deformación en el objeto correspondiente
        meshDeformationConfig = FindObjectOfType<MeshGeneratorPhysics>();

        // Opcional: Inicializar los InputFields con los valores actuales, SOLO VARIABLES POSICION Y TAMAÑO, MAS TARDE
        /*InitPosX.text = meshDeformationConfig.InitPosXValue.ToString();
        InitPosY.text = meshDeformationConfig.InitPosYValue.ToString();
        InitPosz.text = meshDeformationConfig.InitPosZValue.ToString();

        MeshSizeX.text = meshDeformationConfig.MeshSizeX.ToString();
        MeshSizeZ.text = meshDeformationConfig.MeshSizeZ.ToString();
        
        SphereWeight.text = meshDeformationConfig.SphereWeightValue.ToString();
        SprindConst.text = meshDeformationConfig.SprindConstValue.ToString();
        DampConst.text = meshDeformationConfig.DampConstValue.ToString();
         
         */
        InitialPosX.text = meshDeformationConfig.initPosX.ToString();
        InitialPosZ.text = meshDeformationConfig.initPosZ.ToString();
        InitialPosY.text = meshDeformationConfig.initPosY.ToString();

        MeshSizeX.text = meshDeformationConfig.xSize.ToString();
        MeshSizeZ.text = meshDeformationConfig.zSize.ToString();
        Scale.text = meshDeformationConfig.MeshScale.ToString();

        SphereWeight.text = meshDeformationConfig.sphereMass.ToString();
        SprindConst.text = meshDeformationConfig.springConstant.ToString();
        DampConst.text = meshDeformationConfig.dampingConstant.ToString();

        // Vincular los métodos a los eventos onValueChanged de los InputFields
        InitialPosX.onValueChanged.AddListener(OnInitialPosXChanged);
        InitialPosZ.onValueChanged.AddListener(OnInitialPosZChanged);
        InitialPosY.onValueChanged.AddListener(OnInitialPosYChanged);

        MeshSizeX.onValueChanged.AddListener(OnMeshSizeXChanged);
        MeshSizeZ.onValueChanged.AddListener(OnMeshSizeZChanged);
        Scale.onValueChanged.AddListener(OnScaleChanged);

        SphereWeight.onValueChanged.AddListener(OnSphereWeightChanged);
        SprindConst.onValueChanged.AddListener(OnSprindConstChanged);
        DampConst.onValueChanged.AddListener(OnDampConstChanged);
    }


    // Este método será llamado en tiempo real cuando se modifique el texto en el InputField

    public void OnInitialPosXChanged(string input) //se cambia el peso de la esfera
    {
        int value;
        if (int.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateInitialPosX(value);
        }
    }

    public void OnInitialPosZChanged(string input) //se cambia el peso de la esfera
    {
        int value;
        if (int.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateInitialPosZ(value);
        }
    }

    public void OnInitialPosYChanged(string input) //se cambia el peso de la esfera
    {
        int value;
        if (int.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateInitialPosY(value);
        }
    }




    public void OnSphereWeightChanged(string input) //se cambia el peso de la esfera
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateSphereWeight(value);
        }
    }


    public void OnSprindConstChanged(string input)  //se cambia la constante del muelle
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateSprindConst(value);
        }
    }


    public void OnDampConstChanged(string input)  //se cambia la constante de amortiguamiento
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateDampConst(value);
        }
    }


    //Se cambia el tamaño del grid    (gridX x gridZ)
    public void OnMeshSizeXChanged(string input)
    {
        int value;
        if (int.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateXsizeConst(value);
        }
    }

    public void OnMeshSizeZChanged(string input)
    {
        int value;
        if (int.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateZsizeConst(value);
        }
    }

    //se cambia la escala/tamaño del grid
    public void OnScaleChanged(string input)
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateScaleConst(value);
        }
    }
}
