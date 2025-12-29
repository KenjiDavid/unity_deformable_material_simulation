using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XCharts;
using XCharts.Runtime;

[DisallowMultipleComponent]
public class Chart_Script_EXAMPLE : MonoBehaviour
{
   
        private LineChart chart;
        private Serie serie;
        private int m_DataNum = 8;

        private void Start()
        {
            StartCoroutine(PieDemo());
        }

        IEnumerator PieDemo()
        {
            while (true)
            {
                StartCoroutine(AddSimpleLine());
                yield return new WaitForSeconds(2);


            StartCoroutine(ChangeLineType());
            yield return new WaitForSeconds(8);

            StartCoroutine(LineAreaStyleSettings());
            yield return new WaitForSeconds(5);

            StartCoroutine(LineArrowSettings());
            yield return new WaitForSeconds(2);

            StartCoroutine(LineSymbolSettings());
            yield return new WaitForSeconds(7);

            StartCoroutine(LineLabelSettings());
            yield return new WaitForSeconds(3);

            StartCoroutine(LineMutilSerie());
                yield return new WaitForSeconds(5);
            
            }
        }

    //AÑADE UNA LINEA SIMPLE, SERVIRÁ PARA AÑADIR LINEA DE FUERZA
        IEnumerator AddSimpleLine()
        {
            chart = gameObject.GetComponent<LineChart>();
            if (chart == null)
            {
                chart = gameObject.AddComponent<LineChart>();
                chart.Init();
            }
            chart.GetChartComponent<Title>().text = "LineChart - gráfico de líneas";
            chart.GetChartComponent<Title>().subText = "Gráfico de líneas ordinarias";

            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Custom;
            yAxis.min = 0;
            yAxis.max = 100;

            chart.RemoveData();
            serie = chart.AddSerie<Line>("Line");

            for (int i = 0; i < m_DataNum; i++)
            {
                chart.AddXAxisData("x" + (i + 1));
                chart.AddData(0, UnityEngine.Random.Range(30, 90));
            }
            yield return new WaitForSeconds(1);
        }

        IEnumerator ChangeLineType()
        {
            chart.GetChartComponent<Title>().subText = "LineTyle - Gráfico";
            serie.lineType = LineType.Smooth;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "LineTyle - Gráfico de líneas escalonadas";
            serie.lineType = LineType.StepStart;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            serie.lineType = LineType.StepMiddle;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            serie.lineType = LineType.StepEnd;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "LineTyle - línea de puntos 1";
            serie.lineStyle.type = LineStyle.Type.Dashed;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "LineTyle - línea de puntos 2";
            serie.lineStyle.type = LineStyle.Type.Dotted;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "LineTyle - línea de puntos 3";
            serie.lineStyle.type = LineStyle.Type.DashDot;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "LineTyle - línea de puntos doble";
            serie.lineStyle.type = LineStyle.Type.DashDotDot;
            chart.RefreshChart();

            serie.lineType = LineType.Normal;
            chart.RefreshChart();

        } //FIN CAMBIO ESTILO/FORMA DE LA LINEA



    //PINTAR EL AREA DEBAJO DE LA LINEA
        IEnumerator LineAreaStyleSettings()
        {
            chart.GetChartComponent<Title>().subText = "AreaStyle gráfico de área";

            serie.EnsureComponent<AreaStyle>();
            serie.areaStyle.show = true;
            chart.RefreshChart();
            yield return new WaitForSeconds(1f);

            chart.GetChartComponent<Title>().subText = "AreaStyle gráfico de área";
            serie.lineType = LineType.Smooth;
            serie.areaStyle.show = true;
            chart.RefreshChart();
            yield return new WaitForSeconds(1f);

            chart.GetChartComponent<Title>().subText = "AreaStyle gráfico de área - Ajustar la transparencia";
            while (serie.areaStyle.opacity > 0.4)
            {
                serie.areaStyle.opacity -= 0.6f * Time.deltaTime;
                chart.RefreshChart();
                yield return null;
            }
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "AreaStyle gráfico de área - Gradiente";
            serie.areaStyle.toColor = Color.white;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

        } //FIN ESTILO DEL AREA MARCADA
    



    //PONER FLECHAS AL INICIO Y FINAL DE LA GRAFICA
            IEnumerator LineArrowSettings()
            {
                chart.GetChartComponent<Title>().subText = "LineArrow flecha de cabeza";
                chart.GetSerie(0).EnsureComponent<LineArrow>();
                serie.lineArrow.show = true;
                serie.lineArrow.position = LineArrow.Position.Start;
                chart.RefreshChart();
                yield return new WaitForSeconds(1);

                chart.GetChartComponent<Title>().subText = "LineArrow flecha de cola";
                serie.lineArrow.position = LineArrow.Position.End;
                chart.RefreshChart();
                yield return new WaitForSeconds(1);
                serie.lineArrow.show = false;

            } //FIN PERSONALIZACION DE LAS FLECHAS
    




//ESTILO DEL PUNTO, PURAMENTE ESTETICO
    IEnumerator LineSymbolSettings()
        {
            chart.GetChartComponent<Title>().subText = "SerieSymbol marca grafica";
            while (serie.symbol.size < 5)
            {
                serie.symbol.size += 2.5f * Time.deltaTime;
                chart.RefreshChart();
                yield return null;
            }
            chart.GetChartComponent<Title>().subText = "SerieSymbol Marcador gráfico - Círculo hueco";
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "SerieSymbol Marcador gráfico - Círculo relleno";
            serie.symbol.type = SymbolType.Circle;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "SerieSymbol Marcador de forma - Triángulo";
            serie.symbol.type = SymbolType.Triangle;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "SerieSymbol Marcador gráfico - Cuadrado";
            serie.symbol.type = SymbolType.Rect;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "SerieSymbol Marcador gráfico - Diamante";
            serie.symbol.type = SymbolType.Diamond;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

            chart.GetChartComponent<Title>().subText = "SerieSymbol marca grafica";
            serie.symbol.type = SymbolType.EmptyCircle;
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

        }//FIN EDICION PUNTO

        

    //AÑADIR ETIQUETAS A LOS PUNTOS, IDEAL PARA SABER EL VALOR
        IEnumerator LineLabelSettings()
        {
            chart.GetChartComponent<Title>().subText = "SerieLabel etiqueta de texto";
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

      //MULTILINEA, SE PUEDE HACER EN EL MULTICAPA
        IEnumerator LineMutilSerie()
        {
            chart.GetChartComponent<Title>().subText = "Varias series";
            var serie2 = chart.AddSerie<Line>("Line2");
            serie2.lineType = LineType.Normal;
            for (int i = 0; i < m_DataNum; i++)
            {
                chart.AddData(1, UnityEngine.Random.Range(30, 90));
            }
            yield return new WaitForSeconds(1);

            var serie3 = chart.AddSerie<Line>("Line3");
            serie3.lineType = LineType.Normal;
            for (int i = 0; i < m_DataNum; i++)
            {
                chart.AddData(2, UnityEngine.Random.Range(30, 90));
            }
            yield return new WaitForSeconds(1);

            var yAxis = chart.GetChartComponent<YAxis>();
            yAxis.minMaxType = Axis.AxisMinMaxType.Default;
            chart.GetChartComponent<Title>().subText = "Serie múltiple: apilada";
            serie.stack = "samename";
            serie2.stack = "samename";
            serie3.stack = "samename";
            chart.RefreshChart();
            yield return new WaitForSeconds(1);

        }//FIN MULTILINEA

    }   //FIN PROGRAMA




