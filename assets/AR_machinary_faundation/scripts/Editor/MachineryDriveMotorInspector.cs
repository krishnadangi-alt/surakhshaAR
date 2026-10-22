#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SurakshaAR.Machinery.Editor
{
    public static class MachineryDriveMotorInspector
    {
        [MenuItem("SurakshaAR/Machinery/Inspect Drive Motor Alignment")]
        public static void InspectAlignment()
        {
            var scene = EditorSceneManager.GetActiveScene();
            Debug.Log($"Active Scene: {scene.path}");

            GameObject conveyor = GameObject.Find("ConveyorAssembly");
            GameObject motor = GameObject.Find("DriveMotor");

            if (conveyor == null) conveyor = GameObject.Find("industrial_conveyor_belt");
            if (motor == null) motor = GameObject.Find("electric_motor");

            if (conveyor != null)
            {
                var renderers = conveyor.GetComponentsInChildren<Renderer>();
                Bounds b = new Bounds();
                bool first = true;
                foreach (var r in renderers)
                {
                    if (first) { b = r.bounds; first = false; }
                    else { b.Encapsulate(r.bounds); }
                    Debug.Log($"Conveyor Child Renderer: {r.gameObject.name}, bounds min: {r.bounds.min}, max: {r.bounds.max}, center: {r.bounds.center}");
                }
                Debug.Log($"OVERALL CONVEYOR BOUNDS: Center={b.center}, Size={b.size}, Min={b.min}, Max={b.max}");
            }
            else
            {
                Debug.LogError("ConveyorAssembly not found!");
            }

            if (motor != null)
            {
                var renderers = motor.GetComponentsInChildren<Renderer>();
                Bounds b = new Bounds();
                bool first = true;
                foreach (var r in renderers)
                {
                    if (first) { b = r.bounds; first = false; }
                    else { b.Encapsulate(r.bounds); }
                    Debug.Log($"Motor Child Renderer: {r.gameObject.name}, bounds min: {r.bounds.min}, max: {r.bounds.max}, center: {r.bounds.center}");
                }
                Debug.Log($"OVERALL MOTOR BOUNDS: Center={b.center}, Size={b.size}, Min={b.min}, Max={b.max}");
                Debug.Log($"Motor Transform: Pos={motor.transform.position}, Rot={motor.transform.eulerAngles}, Scale={motor.transform.lossyScale}");
            }
            else
            {
                Debug.LogError("DriveMotor not found!");
            }
        }
    }
}
#endif
