using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("SmallHeroes/CharacterCutScene")]
public class CharacterCutScene : MonoBehaviour, IAssetObject
{
    AnimationState[] m_AnimationStates;
    public AnimationState[] AnimationStates
    {
        get
        {
            if (m_AnimationStates == null)
            {
                List<AnimationState> animations = new List<AnimationState>();
                foreach (AnimationState state in Animation)
                {
                    animations.Add(state);
                }
                m_AnimationStates = animations.ToArray();
            }
            return m_AnimationStates;
        }
    }

    SkinnedMeshRenderer m_Renderer = null;
    SkinnedMeshRenderer Renderer
    {
        get
        {
            if (m_Renderer == null)
                m_Renderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
            return m_Renderer;
        }
    }

    [NonSerialized]
    public float PlaybackSpeed = 1f;
    [NonSerialized]
    public float PlaybackTime = 0f;

    public bool IsPlaying { get; private set; }
    public bool IsPlayingAction { get { return IsPlaying && m_CurrentState != null && PlaybackTime < m_CurrentState.length; } }
    public bool IsPause { get; set; }

    public bool CheckPlaying(string name)
    {
        if (CurrentState == null)
            return false;

        if (CurrentState.name == "die")
            return true;

        if (CurrentState.name != name)
            return false;

        if (PlaybackTime >= CurrentStateLength && !(CurrentState.wrapMode == WrapMode.Loop || CurrentState.wrapMode == WrapMode.ClampForever))
            return false;

        return true;
    }

    public void SetRenderQueue(int renderQueue)
    {
        if (Application.isPlaying == false)
            return;

        Material.renderQueue = renderQueue;
        foreach (var ps in GetComponentsInChildren<HeroFX.HFX_ParticleSystem>())
        {
            ps.SetRenderQueue(renderQueue + 1);
        }

    }

    Animation m_Animation;
    public Animation Animation
    {
        get
        {
            if (m_Animation == null && transform.childCount > 0 && transform.GetChild(0).childCount > 0)
            {
                m_Animation = transform.GetChild(0).GetChild(0).GetComponent<Animation>();
            }
            return m_Animation;
        }
    }

    AnimationState m_CurrentState;
    public AnimationState CurrentState
    {
        get { return m_CurrentState; }
#if UNITY_EDITOR
        set
        {
            m_CurrentState = value;
            Sample();
        }
#endif
    }

    public float CurrentStateLength
    {
        get
        {
            if (m_CurrentState == null)
                return 0f;

            return m_CurrentState.length;
        }
    }

    Material m_Material = null;
    public Material Material
    {
        get
        {
            if (m_Material == null)
            {
                SetMaterialDefault();
            }
            return m_Material;
        }
    }

    void SetMaterialDefault()
    {
        SkinnedMeshRenderer skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer)
        {
            if (Application.isPlaying == true)
                m_Material = skinnedMeshRenderer.material;
            else
                m_Material = skinnedMeshRenderer.sharedMaterial;
        }
    }

#if UNITY_EDITOR
    public void CreateImport()
    {
        while (transform.childCount > 0)
        {
            Transform childTransform = transform.GetChild(0);
            GameObject.DestroyImmediate(childTransform.gameObject, true);
        }

        GameObject tween_object = new GameObject("Tween");
        tween_object.transform.SetParent(gameObject.transform, false);

        string model_path = "Assets/CharacterCutScene_Model/" + gameObject.name + "/" + gameObject.name + ".FBX";
        Animation model_prefab = AssetDatabase.LoadAssetAtPath(model_path, typeof(Animation)) as Animation;
        if (model_prefab == null)
            return;

        m_Animation = GameObject.Instantiate<Animation>(model_prefab);
        m_Animation.name = this.name;
        m_Animation.transform.SetParent(tween_object.transform, false);

        SetImportDefault();
    }

    public class Triangle
    {
        public Triangle(int index1, int index2, int index3, ref Vector3[] vertices)
        {
            z_min = Mathf.Min(Mathf.Min(vertices[index1].y, vertices[index2].y), vertices[index3].y);
            z_max = Mathf.Max(Mathf.Max(vertices[index1].y, vertices[index2].y), vertices[index3].y);
            index = new int[3];
            index[0] = index1;
            index[1] = index2;
            index[2] = index3;
        }
        public float z_max;
        public float z_min;
        public int[] index;
    }

    void MeshSort(SkinnedMeshRenderer mesh_renderer)
    {
        if (mesh_renderer.sharedMesh.isReadable == false)
        {
            throw new System.Exception(string.Format("[{0}] mesh isReadable == false", gameObject.name));
        }

        Mesh mesh = new Mesh();
        mesh.name = mesh_renderer.sharedMesh.name;
        mesh_renderer.BakeMesh(mesh);

        int[] tris = mesh.GetTriangles(0);
        Vector3[] vertices = mesh.vertices;

        List<Triangle> new_tris = new List<Triangle>();
        int tri_count = tris.Length;
        for (int i = 0; i < tri_count; i += 3)
        {
            new_tris.Add(new Triangle(tris[i], tris[i + 1], tris[i + 2], ref vertices));
        }

        new_tris = new_tris.OrderByDescending(t => t.z_max).ToList();
        var set_tris = new_tris.SelectMany(t => t.index).ToArray();
        mesh.SetTriangles(set_tris, 0);
        mesh.bindposes = mesh_renderer.sharedMesh.bindposes;
        mesh.boneWeights = mesh_renderer.sharedMesh.boneWeights;
        mesh.vertices = mesh_renderer.sharedMesh.vertices;
        mesh.uv = mesh_renderer.sharedMesh.uv;
        mesh.colors32 = mesh_renderer.sharedMesh.colors32;
        mesh.UploadMeshData(false);
        mesh_renderer.sharedMesh = mesh;

        string mesh_path = "Assets/CharacterCutScene_Model/" + gameObject.name + "/" + gameObject.name + "_mesh.asset";
        AssetDatabase.CreateAsset(mesh, mesh_path);
        AssetDatabase.SaveAssets();
    }

    public void SetImportDefault()
    {
        if (transform.localPosition.y == 0f)
            transform.localPosition = new Vector3(0f, -1.5f, 0f);
        transform.localRotation = Quaternion.identity;
        if (transform.localScale.x == 1f)
            transform.localScale = Vector3.one*0.2f;

        Animation model = Animation;

        if (model.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 1)
        {
            Debug.LogError("SkinnedMesh count error");
        }

        m_AnimationStates = null;

        model.transform.localRotation = CharacterAnimation.rotationCharacter;
        model.GetComponent<Animation>().clip = null;
        model.GetComponent<Animation>().playAutomatically = false;
        model.GetComponent<Animation>().cullingType = AnimationCullingType.AlwaysAnimate;

        CoreUtility.SetRecursiveLayer(gameObject, "UI");

        SkinnedMeshRenderer skinnedMeshRenderer = model.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer)
        {
//            skinnedMeshRenderer.rootBone = RootBone;
            skinnedMeshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            skinnedMeshRenderer.updateWhenOffscreen = true;
            skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            skinnedMeshRenderer.receiveShadows = false;
            skinnedMeshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            skinnedMeshRenderer.motionVectors = false;

            MeshSort(skinnedMeshRenderer);
        }
        else
        {
            Debug.LogWarning("can't find SkinnedMeshRenderer");
        }

        string prefab_path = "Assets/CharacterCutScene/" + gameObject.name + ".prefab";
        CharacterCutScene prefab = AssetDatabase.LoadAssetAtPath(prefab_path, typeof(CharacterCutScene)) as CharacterCutScene;
        if (prefab == null)
            PrefabUtility.CreatePrefab(prefab_path, gameObject, ReplacePrefabOptions.ConnectToPrefab);
        else
            PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
#endif

    public bool ContainsAnimation(string name)
    {
        return Array.Exists(AnimationStates, a => a.name == name);
    }

    public void Awake()
    {
        SetMaterialDefault();
    }

    public void Start()
    {
        Reset();
    }

    public void Update()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            UpdateEditor();
            return;
        }
#endif

        if (IsPlaying == false)
            return;

        if (IsPause == false)
            UpdatePlay(PlaybackTime + RealTime.deltaTime * PlaybackSpeed);
    }

    public void Reset()
    {
        PlaybackTime = 0f;
    }

    public void InitEffect()
    {
        if (Renderer != null)
        {
            Material mat = Renderer.sharedMaterial;
            mat.shader = Shader.Find(mat.shader.name);
        }
    }

    public void Stop()
    {
#if UNITY_EDITOR
        EditorApplication.update -= s_UpdateEditor;
        s_CharacterCutScene = null;
#endif
        IsPlaying = false;

        Reset();
    }

    public void CancelAnimation()
    {
        m_CurrentState = null;
        PlaybackTime = 0f;
    }

    public void SetShadow(bool is_shadow)
    {
        if (is_shadow)
            Material.EnableKeyword("USE_SHADOW");
        else
            Material.DisableKeyword("USE_SHADOW");
    }

    public void Sample()
    {
        if (IsPlaying == false)
            return;

        float playback_time = PlaybackTime;
        AnimationState currentState = m_CurrentState;

//        float current_state_length = CurrentStateLength;

        float animation_time = playback_time;
        if (currentState != null)
        {
            currentState.enabled = true;
            currentState.time = animation_time;
            currentState.weight = 1f;
        }

        Animation.Sample();

        if (currentState != null)
        {
            currentState.enabled = false;

            if (currentState.clip.wrapMode == WrapMode.Once && currentState.length < playback_time)
            {
                m_CurrentState = Animation["default"];
                PlaybackTime = 0f;
            }
        }
    }

    public void UpdatePlay(float time)
    {
        PlaybackTime = time;
        Sample();
    }

    public void Play(bool paused)
    {
        IsPause = paused;
        Reset();

#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            m_RealTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= s_UpdateEditor;
            EditorApplication.update += s_UpdateEditor;
            s_CharacterCutScene = this;
        }

#endif

        IsPlaying = true;
    }

    public AnimationState PlayAnimation(string name)
    {
        if (Play(false, name) == true)
            return m_CurrentState;
        return null;
    }

    public AnimationState Play(bool paused, string name)
    {
        IsPause = paused;

        AnimationState state = Animation[name];
        if (state != null)
        {
            CancelAnimation();

            m_CurrentState = state;

#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                m_RealTime = EditorApplication.timeSinceStartup;
                EditorApplication.update -= s_UpdateEditor;
                EditorApplication.update += s_UpdateEditor;
                s_CharacterCutScene = this;
            }
#endif

            IsPlaying = true;
            return state;
        }
        return null;
    }

    #region edit mode functions
#if UNITY_EDITOR
    double m_RealTime = 0f;
    public float UpdateEditorTime()
    {
        float deltaTime = 0f;
        double newRealTime = EditorApplication.timeSinceStartup;
        if (m_RealTime != 0f)
            deltaTime = (float)(newRealTime - m_RealTime);
        m_RealTime = newRealTime;

        return deltaTime;
    }

    public void UpdateEditor()
    {
        float deltaTime = UpdateEditorTime();
        if (IsPlaying == false)
            return;

        if (IsPause == false)
            PlaybackTime += deltaTime * PlaybackSpeed;

        Sample();
    }

    static CharacterCutScene s_CharacterCutScene = null;
    static void s_UpdateEditor()
    {
        if (s_CharacterCutScene != null && s_CharacterCutScene.IsPlaying && s_CharacterCutScene.IsPause == false)
        {
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.Repaint();
        }
    }
#endif
    #endregion

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
