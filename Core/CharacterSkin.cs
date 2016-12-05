using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class CharacterSkin : MonoBehaviour, IAssetObject
{
    [HideInInspector]
    public SkinnedMeshRenderer Renderer;
    [HideInInspector]
    public string[] Bones;

#if UNITY_EDITOR
    public void ImportRenderer(string model_name, string skin_name, Transform[] bones)
    {
        Renderer = GetComponent<SkinnedMeshRenderer>();
        if (Renderer == null)
            throw new System.Exception("Renderer is null");

        SetImportDefault();
        Bones = Renderer.bones.Select(b => b.name).ToArray();
//        CreateMesh(bones, model_name, skin_name);

        CoreUtility.SetRecursiveLayer(gameObject, "Character");

        string prefab_path = string.Format("Assets/Character_Skin/{0}_skin_{1}.prefab", model_name, skin_name);
        CharacterAnimation prefab = AssetDatabase.LoadAssetAtPath(prefab_path, typeof(CharacterAnimation)) as CharacterAnimation;
        if (prefab == null)
            PrefabUtility.CreatePrefab(prefab_path, gameObject, ReplacePrefabOptions.ConnectToPrefab);
        else
            PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }

    void CreateMesh(Transform[] bones, string model_name, string skin_name)
    {
        List<int> mapping = new List<int>();
        bool error = false;
        foreach (Transform bone in Renderer.bones)
        {
            int index = Array.FindIndex(bones, b => string.Compare(bone.name, b.name, true) == 0);
            if (index < 0)
            {
                Debug.LogErrorFormat("[{0}] can't find bone : {1}", skin_name, bone.name);
                error = true;
            }

            mapping.Add(index);
        }
        if (error)
            return;

        Mesh mesh = new Mesh();
        mesh.name = Renderer.sharedMesh.name;
        Renderer.BakeMesh(mesh);

        int[] tris = mesh.GetTriangles(0);
        Vector3[] vertices = mesh.vertices;

        List<CharacterCutScene.Triangle> new_tris = new List<CharacterCutScene.Triangle>();
        int tri_count = tris.Length;
        for (int i = 0; i < tri_count; i += 3)
        {
            new_tris.Add(new CharacterCutScene.Triangle(tris[i], tris[i + 1], tris[i + 2], ref vertices));
        }

        new_tris = new_tris.OrderBy(t => t.z_min).ToList();
        var set_tris = new_tris.SelectMany(t => t.index).ToArray();
        mesh.SetTriangles(set_tris, 0);
        mesh.bindposes = Renderer.sharedMesh.bindposes;
        mesh.boneWeights = Renderer.sharedMesh.boneWeights;
        mesh.vertices = Renderer.sharedMesh.vertices;
        mesh.uv = Renderer.sharedMesh.uv;
        mesh.colors32 = Renderer.sharedMesh.colors32;
        mesh.normals = Renderer.sharedMesh.normals;
        mesh.UploadMeshData(false);
        Renderer.sharedMesh = mesh;

        string mesh_path = string.Format("Assets/Character_Model/{0}/{0}_skin_{1}_mesh.asset", model_name, skin_name);
        AssetDatabase.CreateAsset(mesh, mesh_path);
        AssetDatabase.SaveAssets();
    }

    void SetImportDefault()
    {
        Renderer.rootBone = null;
        Renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        Renderer.updateWhenOffscreen = true;
        Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Renderer.receiveShadows = false;
        Renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        Renderer.motionVectors = false;
        Renderer.skinnedMotionVectors = false;
    }
#endif

    public void Init(Transform root_bone, Transform[] bones)
    {
        Renderer.rootBone = root_bone;
        Renderer.bones = Bones.Select(b => Array.Find(bones, bt => bt.name == b)).ToArray();
    }

    public void InitEffect()
    {
        if (Renderer != null)
        {
            Material mat = Renderer.sharedMaterial;
            mat.shader = Shader.Find(mat.shader.name);
        }
    }

    public void OnAlloc()
    {
    }

    public void OnFree()
    {
    }

    public void InitPrefab()
    {
        InitEffect();
    }
}
