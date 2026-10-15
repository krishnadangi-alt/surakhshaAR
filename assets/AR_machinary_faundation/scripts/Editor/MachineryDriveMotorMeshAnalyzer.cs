#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryDriveMotorMeshAnalyzer
    {
        [MenuItem("SurakshaAR/Machinery/Analyze Drive Motor Mesh Geometry")]
        public static void AnalyzeGeometry()
        {
            string scenePath = "Assets/AR_machinary_faundation/scenes/MachineryConveyorJam.unity";
            if (File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }

            var scene = EditorSceneManager.GetActiveScene();
            Debug.Log($"--- ANALYZING SCENE: {scene.path} ---");

            GameObject conveyor = GameObject.Find("ConveyorAssembly");
            if (conveyor == null) conveyor = GameObject.Find("industrial_conveyor_belt");

            GameObject motor = GameObject.Find("DriveMotor");
            if (motor == null) motor = GameObject.Find("electric_motor");

            if (conveyor != null)
            {
                Debug.Log("--- CONVEYOR HIERARCHY & BOUNDS ---");
                Debug.Log($"Conveyor World Pos: {conveyor.transform.position}, Rot: {conveyor.transform.eulerAngles}, Scale: {conveyor.transform.lossyScale}");
                PrintTransformTree(conveyor.transform, 0);
            }
            else
            {
                Debug.LogError("ConveyorAssembly not found!");
            }

            if (motor != null)
            {
                Debug.Log("--- MOTOR HIERARCHY & BOUNDS ---");
                Debug.Log($"Motor World Pos: {motor.transform.position}, Rot: {motor.transform.eulerAngles}, Scale: {motor.transform.lossyScale}");
                PrintTransformTree(motor.transform, 0);
            }
            else
            {
                Debug.LogError("DriveMotor not found!");
            }
        }

        private static void PrintTransformTree(Transform t, int depth)
        {
            string indent = new string('-', depth * 2);
            Renderer r = t.GetComponent<Renderer>();
            MeshFilter mf = t.GetComponent<MeshFilter>();
            string rInfo = r != null ? $" [WorldBounds: Center={r.bounds.center}, Size={r.bounds.size}, Min={r.bounds.min}, Max={r.bounds.max}]" : "";
            string mInfo = (mf != null && mf.sharedMesh != null) ? $" [MeshLocalBounds: Center={mf.sharedMesh.bounds.center}, Size={mf.sharedMesh.bounds.size}]" : "";
            
            Debug.Log($"{indent} Node: '{t.name}' Pos={t.localPosition} Rot={t.localEulerAngles} Scale={t.localScale}{rInfo}{mInfo}");

            for (int i = 0; i < t.childCount; i++)
            {
                PrintTransformTree(t.GetChild(i), depth + 1);
            }
        }
    }
}
#endif
