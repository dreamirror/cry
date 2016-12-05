using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using HeroFX;

public class SH_EditorUtility : ScriptableObject
{
    [MenuItem("Utility/Take Screenshot")]
    static void Screenshot()
    {
        string filename = string.Format("ScreenShot/ScreenShot_{0:yyyy_MM_dd HH.MM.ss}.png", System.DateTime.Now);
        Application.CaptureScreenshot(filename);
        Debug.LogFormat("[ScreenShot] {0}", filename);
    }

    [MenuItem("Utility/Show World Transform")]
    static void ShowWorldTransform()
    {
        Debug.LogFormat("Position : {0}", Selection.activeTransform.position);
        Debug.LogFormat("Rotation : {0}", Selection.activeTransform.rotation.eulerAngles);
        Debug.LogFormat("Scale : {0}", Selection.activeTransform.lossyScale);
    }

    [MenuItem("Utility/Show UI World Position")]
    static void ShowUIWorldPosition()
    {
        Transform trans = Selection.activeTransform;
        Vector3 pos = Vector3.zero;
        while(trans != null)
        {
            pos += trans.localPosition;
            trans = trans.parent;
        }
        Debug.Log(pos);
    }

    [MenuItem("Utility/Show RenderQueue")]
    static void ShowRenderQueue()
    {
        Material material = Selection.activeObject as Material;
        if (material != null)
            Debug.Log(material.renderQueue);
    }

    [MenuItem("Utility/Show InstanceID")]
    static void ShowInstanceID()
    {
        var emitter = Selection.activeGameObject.GetComponent<HFX_Emitter>();
        if (emitter != null)
            Debug.Log(string.Format("m: {0}, t: {1}, s:{2}", emitter.GetComponent<MeshRenderer>().sharedMaterial.GetInstanceID(), emitter.GetComponent<MeshRenderer>().sharedMaterial.mainTexture.GetInstanceID(), emitter.GetComponent<MeshRenderer>().sharedMaterial.shader.GetInstanceID()));
    }

    [MenuItem("Utility/Show UIPanel RenderQueue")]
    static void ShowUIPanelRenderQueue()
    {
        UIPanel panel = Selection.activeGameObject.GetComponent<UIPanel>();
        Debug.Log(panel.startingRenderQueue);
    }

    [MenuItem("Utility/Show Hierachy")]
    static void ShowHierachy()
    {
        Debug.Log(CoreUtility.GetHierachy(Selection.activeTransform));
    }

    [MenuItem("Utility/Temp2")]
    static void CheckHFX_PS()
    {
        List<HFX_ParticleSystem> prefabs = GetAllPrefabs<HFX_ParticleSystem>();

        foreach (var ps in prefabs)
        {
            foreach (var em in ps.Emitters)
            {
                if (em.UseTextureAnimation == true && (em.TextureAnimation.AnimationType != eAnimationType.Single && em.RandomStart == true))
                {
                    Debug.LogFormat("PE : {0}/{1}", AssetDatabase.GetAssetPath(ps.GetInstanceID()), em.name);
                }
            }
        }
    }

    static List<T> GetAllPrefabs<T>()
    {
        string[] files;
        GameObject obj;

        List<T> objs = new List<T>();

        // Stack of folders:
        Stack stack = new Stack();

        // Add root directory:
        stack.Push(Application.dataPath);

        // Continue while there are folders to process
        while (stack.Count > 0)
        {
            // Get top folder:
            string dir = (string)stack.Pop();

            try
            {
                // Get a list of all prefabs in this folder:
                files = Directory.GetFiles(dir, "*.prefab");

                // Process all prefabs:
                for (int i = 0; i < files.Length; ++i)
                {
                    // Make the file path relative to the assets folder:
                    files[i] = files[i].Substring(Application.dataPath.Length - 6);

                    obj = (GameObject)AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject));
//                    bool bDirty = false;

                    if (obj != null)
                    {
                        T component = obj.GetComponent<T>();
                        if (component != null)
                            objs.Add(component);
                    }
                }

                // Add all subfolders in this folder:
                foreach (string dn in Directory.GetDirectories(dir))
                {
                    stack.Push(dn);
                }
            }
            catch
            {
                // Error
                Debug.LogError("Could not access folder: \"" + dir + "\"");
            }
        }

        return objs;
    }

    [MenuItem("Utility/DungeonLocation")]
    static void DungeonLocation()
    {
        if (Selection.activeTransform == null)
            return;

        string pos = "";
        Transform transform = Selection.activeTransform;
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = transform.GetChild(i);
            Vector3 vec = child.localPosition;
            vec.x = Mathf.RoundToInt(vec.x);
            vec.y = Mathf.RoundToInt(vec.y);

            if (string.IsNullOrEmpty(pos) == false)
                pos += "\r\n";

            pos += string.Format("{0}\t{1}", vec.x, vec.y);
        }
        EditorGUIUtility.systemCopyBuffer = pos;
        //Debug.Log(GetHierachy(Selection.activeTransform));
    }

    [MenuItem("Assets/Find References")]
    static void FindReferences()
    {
        Object obj = Selection.activeObject;
        if (obj == null)
            return;

        string path = AssetDatabase.GetAssetPath(obj);
        Debug.LogWarningFormat("[{0}] Find References Start", path);

        string title = string.Format("Find References : {0}", path);
        var paths = AssetDatabase.GetAllAssetPaths();
        int progress = 0;
        foreach (string asset in paths)
        {
            if (EditorUtility.DisplayCancelableProgressBar(title, asset, (++progress) / (float)paths.Length))
            {
                Debug.LogWarningFormat("Canceled");
                EditorUtility.ClearProgressBar();
                return;
            }
            if (AssetDatabase.GetDependencies(asset).Contains(path))
                Debug.LogFormat("{0}", asset);
        }
        Debug.LogWarningFormat("[{0}] Find References Finish", path);
        EditorUtility.ClearProgressBar();
    }

#if SH_ASSETBUNDLE
    [MenuItem("Utility/Convert HFX_Particle")]
    static void ConvertHFX_Particle()
    {
        var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/Particle/") && p.EndsWith(".prefab")).ToList();

        int progress = 0;
        foreach (string asset in paths)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Convert HFX_Particle", asset, (++progress) / (float)paths.Count))
            {
                Debug.LogWarningFormat("Canceled");
                return;
            }

            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject));
            GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            HFX_ParticleSystem ps = obj.GetComponent<HFX_ParticleSystem>();
            ps.CheckConvert();
            PrefabUtility.ReplacePrefab(obj, prefab);
            GameObject.DestroyImmediate(obj);
            
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Utility/Character Import")]
    static void CharacterImport()
    {
        var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/Character/") && p.EndsWith(".prefab")).ToList();

        int progress = 0;
        foreach (string asset in paths)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Character Import", asset, (++progress) / (float)paths.Count))
            {
                Debug.LogWarningFormat("Canceled");
                return;
            }

            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject));
            GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            CharacterAnimation ps = obj.GetComponent<CharacterAnimation>();

            if (ps == null)
            {
                GameObject.DestroyImmediate(obj);
                continue;
            }

            ps.CreateImport();
            GameObject.DestroyImmediate(obj);

        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Utility/Character CutScene Import")]
    static void CharacterCutSceneImport()
    {
        var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/CharacterCutScene/") && p.EndsWith(".prefab")).ToList();

        int progress = 0;
        foreach (string asset in paths)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Character CutScene Import", asset, (++progress) / (float)paths.Count))
            {
                Debug.LogWarningFormat("Canceled");
                return;
            }

            GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath(asset, typeof(GameObject));
            GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            CharacterCutScene ps = obj.GetComponent<CharacterCutScene>();

            if (ps == null)
            {
                GameObject.DestroyImmediate(obj);
                continue;
            }

            ps.CreateImport();
            GameObject.DestroyImmediate(obj);

        }
        EditorUtility.ClearProgressBar();
    }
#endif
}
