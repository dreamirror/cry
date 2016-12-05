using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;

public class SH_BuildPostProcessor : MonoBehaviour {
    [PostProcessScene]
    public static void OnPostprocessScene()
    {
        if (EditorApplication.isPlaying)
            return;

        var system_infos = UnityEngine.Object.FindObjectsOfType<SH_SystemInfo>();
        if (system_infos.Length > 1)
            throw new System.InvalidOperationException("More than one SH_SystemInfo in the scene " + EditorSceneManager.GetActiveScene().name);

        if (system_infos.Length == 0)
            return;

        var system_info = system_infos[0];

        system_info.bundle_version = PlayerSettings.bundleVersion;
        system_info.bundle_identifier = PlayerSettings.bundleIdentifier;
    }
}
