# unity_deformable_material_simulation

*SJ Spring Joint
*CJ Configurable Joint

Resumen
La simulación de materiales elásticos deformables es relevante para el desarrollo de aplicaciones robóticas que involucren contacto utilizando motores de videojuegos como Unity. Este trabajo aborda el desarrollo de un modelo de material deformable basado íntegramente en las primitivas físicas nativas de Unity (Rigidbodies y Joints). El modelo propuesto está basado en sistemas Masa-Muelle-Amortiguador (MSD) para mallas planas y multicapa generadas proceduralmente. Este modelo proporciona un método simplificado y computacionalmente más ligero, que permite una simulación adecuada con menos recursos y alto realismo en la deformación. Los resultados demuestran la viabilidad de generar y manipular materiales con propiedades mecánicas configurables en tiempo real, ofreciendo una herramienta eficiente y transparente para la investigación en robótica.

Palabras clave: Deformación elástica, Unity, Sistemas Masa-Muelle-Amortiguador, Robótica, Modelado 3D


Abstract
The simulation of deformable elastic materials is relevant for the development of robotic applications involving contact using game engines like Unity. This paper addresses the development of a deformable material model based entirely on Unity’s native physics primitives (Rigidbodies and Joints). To achieve this, a deformable elastic material model based on Mass-Spring-Damper (MSD) systems is implemented for procedurally generated planar and multilayer meshes. This model provides a simplified and computationally lighter method, enabling adequate simulation with fewer resources and high realism in deformation. The results demonstrate the feasibility of generating and manipulating materials with configurable mechanical properties in real time, offering an efficient and transparent tool for robotics research.

Keywords: Elastic deformation, Unity, Mass-Spring-Damper Systems, Robotics, 3D Modeling


Requirements:
  - Unity 2021.3.37f1
  - XCharts package (necessary to implement UI configuration)

Work's content:

Folder: Assets/Scenes/Spring_Damp_Scenes :
  - Scene_1_one_layer_mesh_SJ : one layer mesh with frozen perimeter, acting as an "elastic bed" with SJ
  - Scene_2_one_layer_mesh_cloth_SJ : one layer mesh with all free nodes, acticng as a piece of cloth with SJ
  - Scene_3_one_layer_mesh_CJ : one layer mesh with all free nodes, acticng as a piece of cloth with CJ
  - Scene_4_multi_layer_V1 : first attempt of multi-layer mesh, only with colliders in the first layer
  - Scene_5_multi_layer_V2 : second attempt of multi-layer mesh, colliders in all layers
  - Scene_6_multi_layer_V3_SJ_CJ : second attempt of multi-layer mesh, colliders in all layers and diagonal srpings, with SJ or CJ
  - Scene_7_one_layer_beehive_mesh_SJ_CJ : one layer mesh, 6-connection based on beehive, with SJ or CJ

Folder: Videos: 
  - video_RESUMEN_3_min_spanish : summary of the work this article is based on. A more basic approach of the work
  - Video_Gripper : sample of the operation of the one layer mesh in basic robotic applications
