using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Canvas_1_2 : MonoBehaviour
{

    private MeshDeformation_1_2_cloth meshDeformationConfig;
    

    public TMP_InputField InitialPosX;
    public TMP_InputField InitialPosY;
    public TMP_InputField InitialPosZ;

    public TMP_InputField MeshSizeX;
    public TMP_InputField MeshSizeZ;
    public TMP_InputField Scale;
    public TMP_Text GridInfo;

    public TMP_InputField SprindConst;
    public TMP_InputField DampConst;

    public TMP_InputField StaticFrict;
    public TMP_InputField DynamicFrict;
    public TMP_InputField Bounc;

 
    public TMP_InputField ColliderRad; 
    public TMP_Text MaxColliderRad;
    public TMP_Text MinColliderRad;
    public Slider CollRadSlider;

    public Toggle AllowMesh0;

    public Button GenerateMesh; 
    private bool Restart = false;

    void Start()
    {
        // Encuentra el script de deformación en el objeto correspondiente

        meshDeformationConfig = FindObjectOfType<MeshDeformation_1_2_cloth>();

        InitialPosX.text = meshDeformationConfig.initPosX.ToString();
        InitialPosZ.text = meshDeformationConfig.initPosZ.ToString();
        InitialPosY.text = meshDeformationConfig.initPosY.ToString();

        MeshSizeX.text = meshDeformationConfig.xSize.ToString();
        MeshSizeZ.text = meshDeformationConfig.zSize.ToString();
        Scale.text = meshDeformationConfig.MeshScale.ToString();

        SprindConst.text = meshDeformationConfig.springConstant.ToString();
        DampConst.text = meshDeformationConfig.dampingConstant.ToString();

        StaticFrict.text = meshDeformationConfig.MeshStaticFriction.ToString();
        DynamicFrict.text = meshDeformationConfig.MeshDynamicFriction.ToString();
        Bounc.text = meshDeformationConfig.MeshBounciness.ToString();

        ColliderRad.text = meshDeformationConfig.colliderRadius.ToString();

        // Vincular los métodos a los eventos onValueChanged de los InputFields
        InitialPosX.onValueChanged.AddListener(OnInitialPosXChanged);
        InitialPosZ.onValueChanged.AddListener(OnInitialPosZChanged);
        InitialPosY.onValueChanged.AddListener(OnInitialPosYChanged);

        MeshSizeX.onValueChanged.AddListener(OnMeshSizeXChanged);
        MeshSizeZ.onValueChanged.AddListener(OnMeshSizeZChanged);
        Scale.onValueChanged.AddListener(OnScaleChanged);

        SprindConst.onValueChanged.AddListener(OnSpringConstChanged);
        DampConst.onValueChanged.AddListener(OnDampConstChanged);

        StaticFrict.onValueChanged.AddListener(OnStaticFrictChanged);
        DynamicFrict.onValueChanged.AddListener(OnDynamicFrictChanged);
        Bounc.onValueChanged.AddListener(OnBouncChanged);

        ColliderRad.onValueChanged.AddListener(OnColliderRadChanged);
        CollRadSlider.onValueChanged.AddListener(OnSliderValueChanged);

        AllowMesh0.isOn = AllowMesh0.isOn;
        AllowMesh0.onValueChanged.AddListener(OnAllowMesh0Changed);

        GenerateMesh.onClick.AddListener(GenerateMeshBool); //boton para regenerar el mesh
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


    public void OnSpringConstChanged(string input)  //se cambia la constante del muelle
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateSpringConst(value);
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


    //propidades del material
    public void OnStaticFrictChanged(string input)
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateStaticConstant(value);
        }
    }

    public void OnDynamicFrictChanged(string input)
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateDynamicConstant(value);
        }
    }

    public void OnBouncChanged(string input)
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateBounciness(value);
        }
    }

    //funciones para controlar radio colisionador y su slider

    //modifico el input de collider
    public void OnColliderRadChanged(string input)
    {
        float value;
        if (float.TryParse(input, out value))
        {
            meshDeformationConfig.UpdateColliderRadius(value);
            
            //meshDeformationConfig.Instance.SetColliderValue(newValue); // Notifica al programa principal
        }
    
    }

    //modifico el slider
    public void OnSliderValueChanged(float value)
    {
        // Actualizar el texto del collider radius con 2 decimales

        CollRadSlider.value = value; // Asegura que el slider esté sincronizado
        ColliderRad.text = value.ToString("F2");

        meshDeformationConfig.UpdateSliderValue(CollRadSlider.value);

    }

    // Actualiza el rango del slider y los textos asociados.
    public void UpdateSliderRange(float minValue, float maxValue)
    {
        CollRadSlider.minValue = minValue;
        CollRadSlider.maxValue = maxValue;

        // Actualizar los textos
        MinColliderRad.text = $"{minValue:F2}";
        MaxColliderRad.text = $"{maxValue:F2}";
    }

    // Actualiza el texto del input y la posicion del slider al inicio.
    public void UpdateCurrentValue(float value)
    {
        CollRadSlider.value = value; // Asegura que el slider esté sincronizado
        ColliderRad.text = value.ToString("F2");
    }

    //-------------------

    public void OnAllowMesh0Changed(bool isOn)
    {
        if (meshDeformationConfig != null)
        {
            meshDeformationConfig.UpdateAllowMesh0(isOn); // Llama a la función en el script principal
        }
    }

    private void GenerateMeshBool()
    {
        if (meshDeformationConfig != null)
        {
            meshDeformationConfig.RestartMesh(); // Llama a la función en el script principal
        }
    }

}//fin canvas
