using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using XCharts;
using XCharts.Runtime;

[DisallowMultipleComponent]
public class Chart_Script_2_config_joint : MonoBehaviour
{
    public MeshDeformation_1_3_cloth_config_joint MeshData = new MeshDeformation_1_3_cloth_config_joint();

    private LineChart chart;
    private Serie serie;

    public float scaleFactor = 1;

    private Vector3 nodesPoseMain;   //me guarda las posiciones de los nodos del main script
    private float[] nodesHeight;      //me guarda la altura de los nodos en un vector a parte para que sea más fácil de operar
    private float nodesHeightAvarage;       //media altura nodos de una vuelta
    private List<float> chartedData = new List<float>();        //data que saldra en la gráfica
    public int numData = 10;        //cantidad de datos en la gráfica
    public float sample_time = 0.25f;

    //grafica deformacion 0 / fuerza 1
    public bool deform0_force1 = false;

    public GameObject test_object;      //objeto con el que la malla va a colisionar

    public string titulo;
    public string sub_titulo;

    //velocidades esfera para hacer diferencial
    private float prev_vel = 0.0f;
    private float actual_vel = 0.0f;

    public float min_max_val_data = 0.001f;

    //para permitir facilmente ver o no los debug
    public bool allow_debug = false;


    private void Start()
    {
        Rigidbody rb_test_object = test_object.GetComponent<Rigidbody>();

        chart = gameObject.GetComponent<LineChart>();
        if (chart == null)
        {
            chart = gameObject.AddComponent<LineChart>();
            chart.Init();
        }
        chart.GetChartComponent<Title>().text = titulo;
        chart.GetChartComponent<Title>().subText = sub_titulo;

        var yAxis = chart.GetChartComponent<YAxis>(); //me permite confirgurar el eje Y
        yAxis.minMaxType = Axis.AxisMinMaxType.Custom;

        yAxis.min = -1 * scaleFactor;
        yAxis.max = 1 * scaleFactor;



        chart.RemoveData();
        serie = chart.AddSerie<Line>("Line");


        for (int i = 0; i < numData; i++)
        {
            chartedData.Add(0f);
        }

        Vector3[] nodesPosition = new Vector3[MeshData.vertices.Length];
        float[] nodesHeight = new float[MeshData.vertices.Length];

        for (int i = 0; i < MeshData.vertices.Length; i++)
        {
            nodesPosition[i] = Vector3.zero; // O cualquier otro valor predeterminado
            nodesHeight[i] = 0f; // O cualquier otro color predeterminado
        }

        for (int i = numData - 1; i >= 0; i--)
        {
            //chart.AddXAxisData("x" + (i + 1));  //otra forma de configurar el eje X
            chart.AddData(0, chartedData[i]);
        }

        prev_vel = rb_test_object.velocity.y;
        actual_vel = rb_test_object.velocity.y;

        StartCoroutine(PieDemo()); //CAMBIAR A UPDATE


    }

    IEnumerator PieDemo()
    {
        while (true)
        {

            if (MeshData.vertices != null)
            {
                //Debug.Log("Se han cargando datos");
                StartCoroutine(AddSimpleLine());

            }
            else
            {
                Debug.Log("No hay datos. Cargando datos");
            }


            //StartCoroutine(LineLabelSettings());
            yield return new WaitForSeconds(sample_time);


        }
    }

    //AÑADE UNA LINEA SIMPLE, SERVIRÁ PARA AÑADIR LINEA DE FUERZA
    IEnumerator AddSimpleLine()
    {
        var yAxis = chart.GetChartComponent<YAxis>();
        Rigidbody rb_test_object = test_object.GetComponent<Rigidbody>();


        actual_vel = rb_test_object.velocity.y;

        //asigno variables del main a variables locales
        Vector3[] nodesPoseMain = new Vector3[MeshData.vertices.Length];
        // Asegúrate de inicializar nodesHeight
        nodesHeight = new float[MeshData.vertices.Length];

        int valid_data = 0;
        for (int i = 0; i < MeshData.vertices.Length; i++)
        {
            nodesPoseMain[i] = new Vector3(MeshData.vertices[i].x, MeshData.vertices[i].y, MeshData.vertices[i].z);

            if (nodesPoseMain[i].y >= min_max_val_data || nodesPoseMain[i].y <= -min_max_val_data)
            {
                nodesHeight[i] = nodesPoseMain[i].y;
                valid_data++;
            }
        }
        nodesHeightAvarage = nodesHeight.Sum(); //calculo la media de la altura
        //calculo la media de la altura
        nodesHeightAvarage = nodesHeightAvarage / valid_data;




        if (deform0_force1 == false)
        {
            chartedData.RemoveAt(0); //se quita el primer elemento de la gráfica

            if (float.IsNaN(nodesHeightAvarage)) chartedData.Add(0);
            else chartedData.Add(nodesHeightAvarage);

            if (allow_debug == true) Debug.Log("Media de la deformación de la malla: " + nodesHeightAvarage);  //para la deformación de la malla
        }
        else //la fuerza aplicada por la esfera en la malla
        {
            float aplied_force = 0f;
            float dif_vel = 0f;
            dif_vel = (actual_vel - prev_vel);

            float accel = dif_vel / sample_time; //calculo la aceleracion
            prev_vel = actual_vel;

            aplied_force = accel * rb_test_object.mass + rb_test_object.mass * 9.8f;
            if (aplied_force >= yAxis.max || aplied_force <= yAxis.min)
            {
                if (allow_debug == true) Debug.Log("Ha habido un error en la gráfica, valor error " + aplied_force);
            }
            else
            {
                chartedData.RemoveAt(0);//se quita el primer elemento de la gráfica
                if (allow_debug == true) Debug.Log("Fuerza aplicada por la esfera " + aplied_force);
                if (float.IsNaN(aplied_force)) chartedData.Add(0);
                else chartedData.Add(aplied_force);

            }


        }




        serie.RemoveData(0);

        for (int i = numData - 1; i >= 0; i--)
        {
            //chart.AddXAxisData("x" + (i + 1));  //otra forma de configurar el eje X
            chart.AddData((numData - 1) - i, chartedData[i]);
        }

        serie.symbol.type = SymbolType.Diamond; //los puntos tendrás forma de diamente
        serie.itemStyle.color = Color.red; //diamantes rojos
        serie.lineStyle.color = Color.blue; //linea azul

        serie.lineType = LineType.Smooth; //hace que la linea del gráfico sea curva
        chart.RefreshChart();
        yield return new WaitForSeconds(0.1f);
    }











    //CAMBIAR COLOR

    //    // Cambiar el color del símbolo
    //    serie.itemStyle.color = Color.red;

    //    // Cambiar el color del borde del símbolo
    //    serie.symbol.borderColor = Color.black;
    //    serie.symbol.borderWidth = 2;

    //    // Cambiar el color de la línea
    //    serie.lineStyle.color = Color.blue;


    //AÑADIR ETIQUETAS A LOS PUNTOS, IDEAL PARA SABER EL VALOR, más tarde
    IEnumerator LineLabelSettings()
    {
        serie.EnsureComponent<LabelStyle>();
        chart.RefreshChart();
        while (serie.label.offset[1] < 20)
        {
            serie.label.offset = new Vector3(serie.label.offset.x, serie.label.offset.y + 20f * Time.deltaTime);
            chart.RefreshChart();
            yield return null;
        }
        yield return new WaitForSeconds(1);

        chart.RefreshChart();
        yield return new WaitForSeconds(1);

        serie.label.textStyle.color = Color.white;
        serie.label.background.color = Color.grey;
        serie.labelDirty = true;
        chart.RefreshChart();
        yield return new WaitForSeconds(1);

        serie.label.show = false;
        chart.RefreshChart();

    }//FIN AÑADIR ETIQUETAS



}   //FIN PROGRAMA