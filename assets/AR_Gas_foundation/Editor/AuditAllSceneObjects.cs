#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;

public class AuditAllSceneObjects
{
    [MenuItem("SurakhshaAR/Audit All Scene Objects")]
    public static void Audit()
    {
        StringBuilder sb = new StringBuilder();
        string scenePath = "Assets/AR_Gas_foundation/scenes/AR_gas_Foundation.unity";
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject[] rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        sb.AppendLine($"Total Root Objects in Scene: {rootObjs.Length}");

        foreach (var root in rootObjs)
        {
            sb.AppendLine($"Root Object: '{root.name}'");
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.GetComponent<Renderer>() != null)
                {
                    sb.AppendLine($"  - '{t.name}' (Parent: {t.parent?.name}), Pos={t.position.ToString("F3")}");
                }
            }
        }

        File.WriteAllText(@"C:\SurakhshaAR\SceneAuditObjects.txt", sb.ToString());
    }
}
#endif
