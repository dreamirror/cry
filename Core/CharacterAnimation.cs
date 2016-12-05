using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CharacterAnimation : MonoBehaviour, HeroFX.IHFX_AttachRoot
{
#if SH_ASSETBUNDLE && UNITY_EDITOR
    public bool PrefabState = true;
#endif

    public float JumpScaleValue { get; set; }
    public Color32 StateColor { get; set; }

    static public Color MaterialColorDefault = new Color(0.55f, 0.55f, 0.55f, 1f);
    static public Color MaterialGrayColorDefault = new Color(0f, 0f, 0f, 0f);

    public static readonly Quaternion rotationCharacter = Quaternion.Euler(0f, 180f, 0f);
    static readonly Matrix4x4 matRotationCharacter = Matrix4x4.TRS(Vector3.zero, rotationCharacter, Vector3.one);

    [SerializeField]
    Transform[] m_Bones;

    public Transform GetAttachRoot() { return Animation.transform; }
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

    static float s_BlendTime = 0.1f;
    float BlendTime { get { return s_BlendTime * PlaybackSpeed; } }

    [NonSerialized]
    public eCharacterDummyMode DummyMode = eCharacterDummyMode.None;

    [NonSerialized]
    public float PlaybackSpeed = 1f;
    [NonSerialized]
    public float PlaybackTime = 0f;

    public bool IsPlaying { get; private set; }
    public bool IsPlayingAction { get { return IsPlaying && m_CurrentState != null && PlaybackTime < m_CurrentState.length; } }
    public bool IsPause { get; set; }

    public Transform RootBone { get; private set; }
    public Transform FxBone { get; private set; }
    public Transform BipBone { get; private set; }
    public Transform MoveBone { get; private set; }
    public Vector3 RootBonePos { get; private set; }
    public Vector3 FxBonePos { get; private set; }

    public Transform BipNode { get { return BipBone; } }

    public Transform HeadBone { get; private set; }
    float m_HeadTime = 0f;
    bool isHead = false;

    float BipZ = 0f;
    [SerializeField]
    public float IdleRotation = 40f;
    public bool IsHeadReverse { get { return IdleRotation < -90f; } }
    public float GetIdleRotation()
    {
        if (IsHeadReverse)
            return IdleRotation + 180f;
        return IdleRotation;
    }

    public List<Transform> ChildBones;

    public bool IsDeadEnd
    {
        get
        {
            return CurrentState != null && CurrentState.name == "die" && PlaybackTime >= CurrentStateLength;
        }
    }

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
            ps.SetRenderQueue(renderQueue+1);
        }

    }

    void CheckBones()
    {
        ChildBones = new List<Transform>();
        RootBone = m_Animation.transform.FindChild("Root");
        BipBone = FindTransform(m_Animation.transform, "Bip");
        if (BipBone != null)
            BipZ = BipBone.localPosition.z;
        MoveBone = FindTransform(m_Animation.transform, "Move");
        FxBone = m_Animation.transform.FindChild("fx_bone");
        HeadBone = FindTransformEndsWith(m_Animation.transform, "Head");

        for (int i = 0; i < m_Animation.transform.childCount; ++i)
        {
            Transform bone = m_Animation.transform.GetChild(i);
            if (bone == RootBone || bone.GetComponent<SkinnedMeshRenderer>() != null)
                continue;

            ChildBones.Add(bone);
        }
    }

    public static Transform FindTransformEndsWith(Transform transform, string name)
    {
        if (transform.name.EndsWith(name))
            return transform;

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = FindTransformEndsWith(transform.GetChild(i), name);
            if (child != null)
                return child;
        }
        return null;
    }

    static Transform FindTransform(Transform transform, string name)
    {
        if (string.Compare(transform.name, name, true) == 0)
            return transform;

        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform child = FindTransform(transform.GetChild(i), name);
            if (child != null)
                return child;
        }
        return null;
    }

    Animation m_Animation;
    public Animation Animation
    {
        get
        {
            if (m_Animation == null && transform.childCount > 0 && transform.GetChild(0).childCount > 0)
            {
                m_Animation = transform.GetChild(0).GetChild(0).GetComponent<Animation>();
                if (m_Animation != null)
                {
                    CheckBones();
                }
            }
            return m_Animation;
        }
    }

    [SerializeField]
    string m_DefaultStateName;
    AnimationState m_DefaultState;
    public AnimationState DefaultState
    {
        get { return m_DefaultState; }
        set { m_DefaultState = value; if (m_DefaultState != null) m_DefaultStateName = m_DefaultState.name; Sample(); }
    }

    AnimationState m_CurrentState;
    public AnimationState CurrentState
    {
        get { return m_CurrentState; }
#if UNITY_EDITOR
        set
        {
            m_CurrentState = value;
            if (m_CurrentState == DamageState)
            {
                CheckDamageState();
                DamageTime = 0f;
                isDamage = true;
            }
            else
                isDamage = false;
            SetMoveState(true);
            SetFxState(true);
            SetLoopState();
            Sample();
        }
#endif
    }

    public bool IsDefaultState
    {
        get
        {
            return CurrentState == null || PlaybackTime > CurrentStateLength;
        }
    }

    public float CurrentStateLength
    {
        get
        {
            if (m_CurrentState == null)
                return 0f;

            if (CurrentLoopState == null)
                return m_CurrentState.length;

            int fps = Mathf.FloorToInt(m_CurrentState.clip.frameRate);
            int frame_length = Mathf.FloorToInt(m_CurrentState.length * fps);
            int loop_frame = CurrentLoopState.range_max - CurrentLoopState.range_min;

            return ((frame_length - loop_frame) + loop_frame * CurrentLoopState.loop_count) / (float)fps;
        }
    }

    AnimationState m_PrevState = null;
    public AnimationState PrevState
    {
        get { return m_PrevState; }
        set { m_PrevState = value; if (m_PrevState != null) m_PrevTime = m_PrevState.length; Sample(); }
    }
    public AnimationState DamageState { get; private set; }
    float m_PrevTime = 0f;
    bool isDamage = false, isShield = false;
    Material m_Material = null;
    public Material Material
    {
        get
        {
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

        if (m_Material != null)
        {
            m_Material.color = MaterialColorDefault;
            m_Material.SetColor("_GrayColor", new Color(0f, 0f, 0f, 0f));
            SetRimColor(new Color(1f, 1f, 1f, 0f));
            if (Material.HasProperty("_GlossTex"))
            {
#if UNITY_EDITOR
                if (EditorApplication.isPlaying == false)
                    m_Material.SetFloat("_CurTime", Mathf.Repeat((float)EditorApplication.timeSinceStartup, 20f));
                else
#endif
                    Material.SetFloat("_CurTime", Mathf.Repeat(Time.time, 20f));
            }
            m_Material.SetFloat("_YPos", 0f);
            m_Material.SetFloat("_Scale", 1f);
        }
    }

    public float SetShadowAlpha(float alpha)
    {
        float shadow_alpha = m_Material.GetFloat("_ShadowAlpha");
        m_Material.SetFloat("_ShadowAlpha", alpha);
        return shadow_alpha;
    }

    public void SetAlpha(float alpha)
    {
        m_Material.SetFloat("_Alpha", alpha);
    }

    public float DamageTime { get; set; }
    public float ShieldTime { get; set; }

    public void SetShield()
    {
        isShield = true;
        ShieldTime = PlaybackTime;
    }

    [Serializable]
    public class MoveState
    {
        public MoveState() { }
        public MoveState(string name, float length) { this.name = name; this.length = length; this.fixed_length = length; }
        public bool enabled = true;
        public string name;
        public float length;
        public float fixed_length;
    }

    [Serializable]
    public class FxState
    {
        public FxState() { }
        public FxState(string name, float length) { this.name = name; this.length = length; this.fixed_length = length; }
        public bool enabled = false;
        public string name;
        public float length;
        public float fixed_length;

        public string[] SubBoneNames = new string[0];

        public void CheckBone(Transform root)
        {
            SubBones = new List<Transform>();
            for (int i = 0; i < SubBoneNames.Length; ++i)
            {
                Transform sub_bone = root.FindChild(SubBoneNames[i]);
                if (sub_bone == null)
                    sub_bone = FixSubBone(ref SubBoneNames[i], root);
                SubBones.Add(sub_bone);
            }
        }

        public Transform FixSubBone(ref string name, Transform root)
        {
            string new_name = name;
            string node_name = name.Substring(name.LastIndexOf("/") + 1);
            Transform new_transform = CharacterAnimation.FindTransformEndsWith(root, node_name);
            Transform sub_bone = null;
            if (new_transform != null)
            {
                new_name = CoreUtility.GetHierachy(new_transform, root);
                sub_bone = root.FindChild(new_name);
            }
            if (sub_bone == null)
            {
                Debug.LogFormat("Fix Failed : {0}", name);
            }
            else
                Debug.LogFormat("Fixed : {0} -> {1}", name, new_name);
            return sub_bone;
        }

        public List<Transform> SubBones { get; private set; }
    }

    public MoveState[] MoveStates;
    public MoveState CurrentMoveState { get; private set; }

    public FxState[] FxStates;
    public FxState CurrentFxState { get; private set; }

    [NonSerialized]
    public bool UseMove = true;
    public bool IsUIMode { get; private set; }
    [NonSerialized]
    public Vector3 MoveValue = new Vector3(11f, 0f, 0f);
    [NonSerialized]
    public Vector3 MoveValueFx = new Vector3(11f, 0f, 0f);
    [NonSerialized]
    public Vector3 MoveValueUnscaled = new Vector3(0f, 0f, 0f);
    [NonSerialized]
    public float MoveDistance = 0f;

    public string[] Skins = new string[0];
    public AssetContainer<CharacterSkin> CurrentSkin = null;

    [Serializable]
    public class LoopState
    {
        public LoopState() { }
        public LoopState(string name, int range_min, int range_max, int loop_count) { this.name = name; this.range_min = range_min; this.range_max = range_max; this.loop_count = loop_count; }
        public string name;
        public int range_min, range_max;
        public int loop_count = 1;

        public LoopState Clone()
        {
            return new LoopState(name, range_min, range_max, loop_count);
        }
    }
    public LoopState[] LoopStates;
    public LoopState CurrentLoopState { get; private set; }
    [NonSerialized]
    public bool UseLoopState = true;

#if UNITY_EDITOR
    public void RefreshMove(bool bForce = false)
    {
        if (MoveStates == null && bForce == false)
            return;

        List<CharacterAnimation.MoveState> move_states;
        if (MoveStates == null)
            move_states = new List<MoveState>();
        else
            move_states = new List<CharacterAnimation.MoveState>(MoveStates);
        foreach (AnimationState state in Animation)
        {
            float root_length = GetMoveLength("Root", state);
            if (root_length < -1f)
            {
                CharacterAnimation.MoveState find_state = move_states.Find(s => s.name == state.name);
                if (find_state != null)
                {
                    find_state.length = root_length;
                }
                else
                    move_states.Add(new CharacterAnimation.MoveState(state.name, root_length));
            }
            else
            {
                float move_length = GetMoveLengthAll("Root/Move", state);
                if (move_length > 1f)
                {
                    CharacterAnimation.MoveState find_state = move_states.Find(s => s.name == state.name);
                    if (find_state != null)
                    {
                        find_state.length = 0f;
                    }
                    else
                        move_states.Add(new CharacterAnimation.MoveState(state.name, 0f));
                }
            }
        }
        move_states.RemoveAll(s => Animation[s.name] == null);
        MoveStates = move_states.ToArray();
    }

    public void RefreshFx(bool bForce = false)
    {
        if (FxStates == null && bForce == false)
            return;

        List<CharacterAnimation.FxState> fx_states;
        if (FxStates != null)
            fx_states = new List<CharacterAnimation.FxState>(FxStates);
        else
            fx_states = new List<FxState>();

        foreach (AnimationState state in Animation)
        {
            float move_length = 0f;
            if (Array.Exists(MoveStates, s => s.name == state.name && s.enabled == true))
            {
                move_length = GetMoveLengthAdditive("m_LocalPosition.x", "Root", "fx_bone", state);
            }
            else
                move_length = GetMoveLength("fx_bone", state);

            if (move_length < -1f)
            {
                CharacterAnimation.FxState find_state = fx_states.Find(s => s.name == state.name);
                if (find_state != null)
                {
                    find_state.length = move_length;
                }
                else
                    fx_states.Add(new CharacterAnimation.FxState(state.name, move_length));
            }
        }
        fx_states.RemoveAll(s => Animation[s.name] == null);
        FxStates = fx_states.ToArray();
    }

#if SH_ASSETBUNDLE && UNITY_EDITOR
    public void CreateImport()
    {
        PrefabState = true;
        while (transform.childCount > 0)
        {
            Transform childTransform = transform.GetChild(0);
            GameObject.DestroyImmediate(childTransform.gameObject, true);
        }

        GameObject tween_object = new GameObject("Tween");
        tween_object.transform.SetParent(gameObject.transform, false);

        string model_path = "Assets/Character_Model/" + gameObject.name + "/" + gameObject.name + ".FBX";
        Animation model_prefab = AssetDatabase.LoadAssetAtPath(model_path, typeof(Animation)) as Animation;
        if (model_prefab == null)
            return;

        m_Animation = GameObject.Instantiate<Animation>(model_prefab);
        m_Animation.name = this.name;
        m_Animation.transform.SetParent(tween_object.transform, false);

        List<string> skins = new List<string>();

        List<Transform> bones = new List<Transform>();
        GetAllBones(bones, m_Animation.transform);
        m_Bones = bones.ToArray();

        string skin_name = "default";
        CreateSkin(m_Animation, skin_name);
        skins.Add(skin_name);

        SkinnedMeshRenderer skinnedMeshRenderer = m_Animation.GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMeshRenderer.transform.SetParent(null);
        GameObject.DestroyImmediate(skinnedMeshRenderer.gameObject, true);

        string[] paths = AssetDatabase.GetAllAssetPaths();
        foreach (string path in paths)
        {
            if (path.StartsWith("Assets/Character_Model/"+gameObject.name) == false || path.EndsWith(".FBX") == false || path.Contains("@"))
                continue;

            GameObject go = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
            if (go == null)
                continue;

            Animation ca = go.GetComponent<Animation>();
            if (ca == null)
                continue;

            if (ca.gameObject.name == gameObject.name)
                continue;

            var cao = GameObject.Instantiate<Animation>(ca);
            try
            {
                skin_name = ca.gameObject.name.Substring(gameObject.name.Length + 1);
                CreateSkin(cao, skin_name);
                skins.Add(skin_name);
            }
            finally
            {
                GameObject.DestroyImmediate(cao.gameObject);
            }
        }

        Skins = skins.ToArray();

        SetImportDefault();
        AssetDatabase.SaveAssets();

        PrefabState = false;
    }

    void CreateSkin(Animation animation, string skin_name)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = animation.GetComponentInChildren<SkinnedMeshRenderer>();
        var skin = skinnedMeshRenderer.gameObject.AddComponent<CharacterSkin>();
        skin.ImportRenderer(gameObject.name, skin_name, m_Bones);
    }

    public void SetImportDefault()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        Animation model = Animation;

        if (model.GetComponentsInChildren<SkinnedMeshRenderer>().Length > 1)
        {
            Debug.LogError("SkinnedMesh count error");
        }

        m_AnimationStates = null;
        CheckBones();

        model.transform.localRotation = rotationCharacter;
        model.GetComponent<Animation>().clip = null;
        model.GetComponent<Animation>().playAutomatically = false;
        model.GetComponent<Animation>().cullingType = AnimationCullingType.AlwaysAnimate;
        m_DefaultStateName = "battleidle";

        Transform bip_node = FindTransform(model.transform, "Bip");

        AnimationState idle_state = Animation["idle"];
        if (idle_state != null && HeadBone != null)
        {
            idle_state.enabled = true;
            idle_state.time = 0f;
            idle_state.weight = 1f;
            Animation.Sample();
            idle_state.enabled = false;

            IdleRotation = 90f - GetRotation(HeadBone.transform, transform).eulerAngles.y;
        }

        AnimationState default_state = Animation[m_DefaultStateName];
        if (default_state != null)
        {
            default_state.enabled = true;
            default_state.time = 0f;
            default_state.weight = 1f;
            Animation.Sample();

            default_state.enabled = false;
        }

        {
            BoxCollider temp_col = GetComponent<BoxCollider>();
            if (temp_col != null) DestroyImmediate(temp_col, true);

            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<CapsuleCollider>();
                collider.center = new Vector3(0f, 4f, 0f);
                collider.radius = 3f;
                collider.height = 10f;
            }
        }

        if (bip_node != null)
        {
            BoxCollider temp_col = bip_node.GetComponent<BoxCollider>();
            if (temp_col != null) DestroyImmediate(temp_col, true);

            SphereCollider collider = bip_node.GetComponent<SphereCollider>();
            if (collider != null) DestroyImmediate(collider, true);
        }

        if (HeadBone != null)
        {
            SphereCollider collider = HeadBone.GetComponent<SphereCollider>();
            if (collider == null)
                collider = HeadBone.gameObject.AddComponent<SphereCollider>();
            collider.center = new Vector3(-1.5f, 0f, 0f);
            collider.radius = 3;
        }

        //RootBone.localRotation = Quaternion.Euler(-90f, 180f, 0f);

        CoreUtility.SetRecursiveLayer(gameObject, "Character");

        RefreshMove(true);
        RefreshFx(true);

        string prefab_path = "Assets/Character/" + gameObject.name + ".prefab";
        CharacterAnimation prefab = AssetDatabase.LoadAssetAtPath(prefab_path, typeof(CharacterAnimation)) as CharacterAnimation;
        if (prefab == null)
            PrefabUtility.CreatePrefab(prefab_path, gameObject, ReplacePrefabOptions.ConnectToPrefab);
        else
            PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
    }
#endif
#endif

    public bool ContainsAnimation(string name)
    {
        return Array.Exists(AnimationStates, a => a.name == name);
    }

    public void SetIdleMode()
    {
        transform.localRotation = Quaternion.Euler(0f, GetIdleRotation(), 0f);
        if (ContainsAnimation("idle"))
            m_DefaultStateName = "idle";
        else
            m_DefaultStateName = "battleidle";
        UseMove = false;
        SetDefaultState();
    }

    public void SetBattleMode()
    {
        transform.localRotation = Quaternion.identity;
        m_DefaultStateName = "battleidle";
        UseMove = true;
        SetDefaultState();
    }

    public void SetUIMode(bool value)
    {
        if (Application.isPlaying == false)
            return;

        IsUIMode = value;
    }

    void CheckDamageState()
    {
        if (DamageState != null || Animation == null)
            return;

        DamageState = Animation["damage"];
        if (DamageState != null)
        {
            DamageState.blendMode = AnimationBlendMode.Additive;
            DamageState.layer = 10;
        }
    }

    public void Awake()
    {
#if SH_ASSETBUNDLE && UNITY_EDITOR
        PrefabState = false;
#endif

        CheckDamageState();
        CheckDefaultState();
    }

    public void CheckDefaultState()
    {
        if (Animation == null)
            return;

        if (string.IsNullOrEmpty(m_DefaultStateName) == false)
            m_DefaultState = Animation[m_DefaultStateName];
    }

    public void SetDefaultState()
    {
        CheckDefaultState();

        CancelAnimation();
        Reset();
        PrevState = null;
        Sample();
    }

    public void Start()
    {
        Reset();
        if (Application.isPlaying && m_DefaultState != null)
        {
            IsPlaying = true;
            Update();
        }
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
            UpdatePlay(PlaybackTime + Time.deltaTime * PlaybackSpeed);
    }

    void LateUpdate()
    {
        SetRender();
    }

    public void SetRender()
    {
        if (Application.isPlaying == false)
        {
#if UNITY_EDITOR
            if (Material != null)
            {
                m_Material.SetFloat("_CurTime", Mathf.Repeat((float)EditorApplication.timeSinceStartup, 20f));
                m_Material.SetColor("_RimColor", m_RimColor);
            }
#endif
            return;
        }

        if (Material != null)
        {
            float _YPos = 0f, _Scale = 1f;
            if (transform.parent != null)
            {
                _YPos = transform.parent.position.y;
                _Scale = transform.parent.lossyScale.y;
            }

            m_Material.SetFloat("_YPos", _YPos);
            m_Material.SetFloat("_Scale", _Scale);
            m_Material.SetColor("_RimColor", m_RimColor);
            if (IsUIMode == false)
            {
                m_Material.SetFloat("_LensTilt", 0f);

                if (isDamage && (PlaybackTime - DamageTime) <= 0.17)
                {
                    m_Material.SetFloat("_Intensity", 1f);
                    m_Material.SetColor("_GrayColor", new Color32(255, 255, 255, 120));
                }
                else if (isShield == true && (PlaybackTime - ShieldTime) <= 0.3)
                {
                    m_Material.SetFloat("_Intensity", 1f);
                    m_Material.SetColor("_GrayColor", new Color32(109, 188, 255, 255));
                }
                else
                {
                    isShield = false;
                    m_Material.SetFloat("_Intensity", 0f);
                    if (DummyMode == eCharacterDummyMode.None)
                        m_Material.SetColor("_GrayColor", StateColor);
                    else
                        m_Material.SetColor("_GrayColor", BattleBase.Instance != null ? (Color)BattleBase.Instance.color_container.Colors[0].color : MaterialGrayColorDefault);
                }
            }
            else
            {
                m_Material.SetFloat("_LensTilt", 0.15f);
                m_Material.SetFloat("_Intensity", 0f);
                m_Material.SetColor("_GrayColor", MaterialGrayColorDefault);
            }
            if (m_Material.HasProperty("_GlossTex"))
            {
                m_Material.SetFloat("_CurTime", Mathf.Repeat(Time.time, 20f));
            }
        }
    }

    public void Reset()
    {
        PlaybackTime = 0f;
        StateColor = new Color32(0, 0, 0, 0);
        if (Application.isPlaying == true)
        {
            m_Material.color = MaterialColorDefault;
            m_Material.SetFloat("_Intensity", 0f);
            m_Material.SetColor("_GrayColor", MaterialGrayColorDefault);
        }
        isDamage = false;
        isShield = false;
    }

    public void ResetDefaultColor()
    {
        if (Application.isPlaying == true)
        {
            m_Material.color = MaterialColorDefault;
        }
    }

    public void Stop()
    {
#if UNITY_EDITOR
        EditorApplication.update -= s_UpdateEditor;
        s_CharacterAnimation = null;
#endif
        IsPlaying = false;
        isDamage = false;

        Reset();
    }

    public void CancelAnimation()
    {
        if (m_CurrentState != null)
        {
            if (!(m_CurrentState.wrapMode == WrapMode.Clamp || m_CurrentState.wrapMode == WrapMode.ClampForever || m_CurrentState.wrapMode == WrapMode.Loop))
            {
                m_PrevState = m_DefaultState;
                m_PrevTime = PlaybackTime - m_CurrentState.length;
            }
            else
            {
                m_PrevState = m_CurrentState;
                m_PrevTime = PlaybackTime;
            }
        }
        else
        {
            m_PrevState = m_DefaultState;
            m_PrevTime = PlaybackTime;
        }

        m_CurrentState = null;
        DamageTime -= PlaybackTime;
        ShieldTime -= PlaybackTime;
        m_HeadTime -= PlaybackTime;
        PlaybackTime = 0f;
        MoveDistance = 0f;
    }

    public void Sample()
    {
        if (IsPlaying == false || DummyMode == eCharacterDummyMode.Hidden)
            return;

        if (DummyMode == eCharacterDummyMode.Dummy && (CurrentState == null || CurrentState.name != "die"))
        {
            DefaultState.enabled = true;
            DefaultState.time = PlaybackTime;
            UpdateDamage();
            Animation.Sample();
            DefaultState.enabled = false;
            if (DamageState != null)
                DamageState.enabled = false;
            UpdateMove();
            UpdateFx();
            UpdateHead();
            return;
        }

        float playback_time = PlaybackTime, prev_time = m_PrevTime;
        AnimationState currentState = m_CurrentState, prevState = PrevState;
#if UNITY_EDITOR
        CheckDamageState();
#endif
        if (currentState == DamageState)
            currentState = null;
        float current_state_length = CurrentStateLength;
        bool is_state_over = false;
        if (currentState != null && currentState != m_DefaultState && PlaybackTime >= current_state_length && !(currentState.wrapMode == WrapMode.Loop || currentState.wrapMode == WrapMode.ClampForever))
        {
            is_state_over = true;
            prevState = currentState;
            playback_time -= current_state_length;
            prev_time = currentState.length;
            currentState = m_DefaultState;
        }
        else if (currentState == null)
            currentState = m_DefaultState;

        float animation_time = playback_time;
        if (is_state_over == false && currentState != null && currentState.clip != null && CurrentLoopState != null)
        {
            float fps = currentState.clip.frameRate;
            float loop_time = (CurrentLoopState.range_max - CurrentLoopState.range_min) / fps;
            float min_time = CurrentLoopState.range_min / fps, max_time = min_time + loop_time * CurrentLoopState.loop_count;
            if (animation_time > max_time)
                animation_time -= loop_time * (CurrentLoopState.loop_count - 1);
            else if (animation_time > min_time)
                animation_time = min_time + Mathf.Repeat((animation_time - min_time), loop_time);

            //Debug.LogFormat("pt:{0}, at:{1}", playback_time, animation_time);
        }

        if (currentState != null)
        {
            float blendTime = BlendTime;
            if (prevState != null && playback_time < blendTime && prevState != currentState)
            {
                //Debug.Log("blend");
                prevState.enabled = true;
                prevState.time = prev_time;

                currentState.enabled = true;
                currentState.time = animation_time;

                currentState.weight = Mathf.Clamp01(playback_time / blendTime);
                prevState.weight = 1f - currentState.weight;
            }
            else
            {
                currentState.enabled = true;
                currentState.time = animation_time;
                currentState.weight = 1f;
            }
        }
        UpdateDamage();

        Animation.Sample();
        if (currentState != null) currentState.enabled = false;
        if (prevState != null) prevState.enabled = false;
        if (DamageState != null) DamageState.enabled = false;

        UpdateMove();
        UpdateFx();
        UpdateHead();

        //         if (m_DefaultState != null)
        //         {
        //             m_DefaultState.enabled = true;
        //             m_DefaultState.time = PlaybackTime;
        //             m_DefaultState.weight = 1f;
        //             animation.Sample();
        //             m_DefaultState.enabled = false;
        //         }
    }

    void UpdateDamage()
    {
        if (isDamage == false || DamageState == null)
            return;

        float delta_time = PlaybackTime - DamageTime;
        if (delta_time >= DamageState.length)
        {
            if (m_CurrentState != DamageState)
                isDamage = false;
        }
        else
        {
            DamageState.enabled = true;
            DamageState.time = delta_time;
            DamageState.weight = 1f;
        }
    }

    public void PlayHead()
    {
        if (isHead == true)
            return;

        //        if (IsPlayingAction == false || m_CurrentState != null && (m_CurrentState.name == "win" || m_CurrentState.wrapMode == WrapMode.Clamp || m_CurrentState.wrapMode == WrapMode.ClampForever || m_CurrentState.wrapMode == WrapMode.Loop))
        {
            m_HeadTime = PlaybackTime;
            isHead = true;
        }
    }

    readonly float s_HeadTime = 1f;
    readonly float angle_limit = 40f;
    void UpdateHead()
    {
        if (isHead == false)
            return;

        float time = PlaybackTime - m_HeadTime;
        if (time >= s_HeadTime)
            isHead = false;
        else
        {
            Quaternion look;

            float blend = 1f;
            time = Mathf.Clamp01((s_HeadTime - Mathf.Abs(s_HeadTime - Mathf.Repeat(time * 2f, s_HeadTime * 2))) / s_HeadTime * 2f);

            if (HeadBone != null)
            {
                if (IsHeadReverse)
                    look = Quaternion.LookRotation(Vector3.left, Vector3.up);
                else
                    look = Quaternion.LookRotation(Vector3.right, Vector3.back);

                Quaternion quat = Quaternion.Inverse(GetRotation(HeadBone.parent.transform, transform)) * look;

                float angle = Quaternion.Angle(HeadBone.localRotation, quat);
                if (angle > angle_limit)
                    blend = angle_limit / angle;

                quat = Quaternion.Lerp(HeadBone.localRotation, quat, (Mathf.Sin((time * 2f - 1f) * 90f * Mathf.Deg2Rad) * 0.5f + 0.5f) * 0.8f * blend);
                HeadBone.localRotation = quat;
            }
            else
            {
//                 if (IsHeadReverse)
//                     look = Quaternion.LookRotation(Vector3.left, Vector3.up);
//                 else
                    look = Quaternion.LookRotation(Vector3.left, Vector3.forward);

                Quaternion quat = Quaternion.Inverse(GetRotation(BipBone.transform.parent, transform)) * look;

                float angle = Quaternion.Angle(BipBone.localRotation, quat);
                if (angle > angle_limit)
                    blend = angle_limit / angle;

                quat = Quaternion.Lerp(BipBone.localRotation, quat, (Mathf.Sin((time * 2f - 1f) * 90f * Mathf.Deg2Rad) * 0.5f + 0.5f) * 0.8f * blend);
                BipBone.localRotation = quat;
            }
        }
    }

    void UpdateMove()
    {
        RootBonePos = RootBone.localPosition;
        MoveDistance = 0f;

        if (JumpScaleValue != 0f && BipBone != null && JumpScaleValue + 1f != 0f)
        {
            Vector3 bip_pos = BipBone.localPosition;
            float bip_gap = bip_pos.z - BipZ;
            bip_pos.z = BipZ + bip_gap / (JumpScaleValue + 1f);
            BipBone.localPosition = bip_pos;
        }

        if (UseMove == false)
        {
            RootBone.localPosition = Vector3.zero;
            if (MoveBone != null)
            {
                Vector3 move_pos = Vector3.zero;
                if (MoveBone != null)
                {
                    move_pos = Matrix4x4.TRS(Vector3.zero, RootBone.localRotation, Vector3.one).MultiplyPoint3x4(MoveBone.transform.localPosition);
                    RootBonePos += move_pos;
                    move_pos.x = 0f;
                    move_pos.z = 0f;
                    MoveBone.localPosition = Matrix4x4.TRS(Vector3.zero, Quaternion.Inverse(RootBone.localRotation), Vector3.one).MultiplyPoint3x4(move_pos);
                }
            }
            foreach (Transform bone in ChildBones)
            {
                bone.localPosition = bone.localPosition - RootBonePos;
            }

            return;
        }

        if (CurrentMoveState == null || CurrentState == null || PlaybackTime > CurrentStateLength || DummyMode == eCharacterDummyMode.Dummy)
        {
            transform.localPosition = Vector3.zero;
            RootBone.localPosition = Vector3.zero;
            if (MoveBone != null)
                MoveBone.localPosition = Vector3.zero;
            return;
        }

        if (UseMove == true)
        {
            Vector3 move_pos = Vector3.zero;
            if (MoveBone != null)
            {
                move_pos = Matrix4x4.TRS(Vector3.zero, RootBone.localRotation, Vector3.one).MultiplyPoint3x4(MoveBone.transform.localPosition);
                RootBonePos += move_pos;

                move_pos = matRotationCharacter.MultiplyPoint3x4(move_pos);
            }

            if (CurrentMoveState.length != 0f)
            {
                float percent = RootBone.localPosition.x / CurrentMoveState.length;
                transform.localPosition = MoveValue * percent + move_pos;

                MoveDistance = Vector3.Magnitude(MoveValueUnscaled * percent + move_pos);
            }
            else
                transform.localPosition = move_pos;
        }

        foreach (Transform bone in ChildBones)
        {
            bone.localPosition = bone.localPosition - RootBonePos;
        }

        RootBone.localPosition = Vector3.zero;
        if (MoveBone != null)
            MoveBone.localPosition = Vector3.zero;
    }

    void UpdateFx()
    {
        if (FxBone == null || CurrentFxState == null)
            return;

        if (UseMove == false)
        {
//            FxBone.localPosition = Vector3.zero;
            return;
        }

        //RootBone.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        if (CurrentFxState == null || CurrentState == null || PlaybackTime > CurrentStateLength || DummyMode == eCharacterDummyMode.Dummy)
        {
//            transform.localPosition = Vector3.zero;
//             FxBone.localPosition = Vector3.zero;
            return;
        }

        if (UseMove == true)
        {
            FxBonePos = FxBone.localPosition + RootBonePos;
            Vector3 fx_pos_original = FxBone.localPosition;

            float state_length = CurrentFxState.length;

            float percent = fx_pos_original.x / state_length;
            Vector3 scale = transform.localScale;

            Vector3 move_vec = -MoveValueFx * percent;
            move_vec.x /= scale.x;

            Vector3 fx_pos = fx_pos_original + RootBonePos;
            fx_pos.x = move_vec.x;
            fx_pos.y += move_vec.y - RootBonePos.y;
            fx_pos.z += move_vec.z;
            FxBone.localPosition = fx_pos;

            if (CurrentFxState.SubBoneNames != null && CurrentFxState.SubBoneNames.Length > 0)
            {
                if (CurrentFxState.SubBones == null)
                    CurrentFxState.CheckBone(transform);

                foreach (var bone in CurrentFxState.SubBones)
                {
                    if (bone != null)
                        bone.localPosition += -fx_pos_original + fx_pos;
                }
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
            s_CharacterAnimation = this;
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

    public void SetMoveState(bool set_move_value)
    {
        if (m_CurrentState == null || m_CurrentState == m_DefaultState)
        {
            CurrentMoveState = null;
            return;
        }

        CurrentMoveState = Array.Find(MoveStates, s => s.name == m_CurrentState.name && s.enabled == true);
        if (set_move_value == true && CurrentMoveState != null)
            MoveValue = new Vector3(-CurrentMoveState.fixed_length, 0f, 0f);
    }

    public bool IsMoveState(string name)
    {
        return Array.Exists(MoveStates, s => s.name == name && s.enabled == true);
    }

    public void SetFxState(bool set_move_value)
    {
        if (m_CurrentState == null || m_CurrentState == m_DefaultState)
        {
            CurrentFxState = null;
            return;
        }

        CurrentFxState = Array.Find(FxStates, s => s.name == m_CurrentState.name && s.enabled == true);
        if (set_move_value == true && CurrentFxState != null)
            MoveValueFx = new Vector3(-CurrentFxState.fixed_length, 0f, 0f);
    }

    void SetLoopState()
    {
        if (m_CurrentState == null || m_CurrentState == m_DefaultState)
        {
            CurrentLoopState = null;
            return;
        }

        CurrentLoopState = Array.Find(LoopStates, s => s.name == m_CurrentState.name);
    }

    public AnimationState Play(bool paused, string name)
    {
        IsPause = paused;

        AnimationState state = Animation[name];
        if (state != null)
        {
            if (state == DamageState)
            {
                isDamage = true;
                DamageTime = PlaybackTime;
                return state;
            }

            CancelAnimation();

            m_CurrentState = state;
            SetMoveState(false);
            SetFxState(false);
            SetLoopState();

#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                m_RealTime = EditorApplication.timeSinceStartup;
                EditorApplication.update -= s_UpdateEditor;
                EditorApplication.update += s_UpdateEditor;
                s_CharacterAnimation = this;
            }
#endif

            IsPlaying = true;
            return state;
        }
        return null;
    }

    Color m_RimColor;
    public void SetRimColor(Color color)
    {
        m_RimColor = color;
    }

    Quaternion GetRotation(Transform transform, Transform root)
    {
        if (transform == root)
            return Quaternion.identity;

        return GetRotation(transform.parent, root) * transform.localRotation;
    }

    public void SetSkin(string skin_name)
    {
        string modelname = Animation.gameObject.name;

        if (Array.Exists(Skins, s => string.Compare(s, skin_name, true) == 0) == false)
            throw new System.Exception(string.Format("[{0}] skin failed : {1}", modelname, skin_name));

        if (CurrentSkin != null && CurrentSkin.IsInit == true)
            CurrentSkin.Free();

#if UNITY_EDITOR
#if SH_ASSETBUNDLE
        CharacterSkin prefab = null;
        string prefab_path = string.Format("Assets/Character_Skin/{0}_skin_{1}.prefab", modelname, skin_name);
        prefab = AssetDatabase.LoadAssetAtPath<CharacterSkin>(prefab_path);
        if (EditorApplication.isPlaying == false)
            EditorUtility.SetDirty(gameObject);
        CurrentSkin = new AssetContainer<CharacterSkin>(new AssetData(prefab.gameObject));
#else
        CurrentSkin = AssetManager.GetCharacterSkinAsset(modelname, skin_name);
#endif
#endif

        CurrentSkin.Alloc();
        CurrentSkin.Asset.transform.SetParent(Animation.transform, false);
        CurrentSkin.Asset.gameObject.name = skin_name;
        CurrentSkin.Asset.Init(RootBone, m_Bones);

        SetMaterialDefault();
    }

    public void FreeSkin()
    {
        CurrentSkin.Free();
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

    static CharacterAnimation s_CharacterAnimation = null;
    static void s_UpdateEditor()
    {
        if (s_CharacterAnimation != null && s_CharacterAnimation.IsPlaying && s_CharacterAnimation.IsPause == false)
        {
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.Repaint();
        }
    }

    public float GetMoveLength(string path, AnimationState state)
    {
        return GetMoveLength("m_LocalPosition.x", path, state, false);
    }

    public float GetMoveLengthAll(string path, AnimationState state)
    {
        float x = GetMoveLength("m_LocalPosition.x", path, state, true);
        float y = GetMoveLength("m_LocalPosition.y", path, state, true);
        float z = GetMoveLength("m_LocalPosition.z", path, state, true);
        return (new Vector3(x, y, z)).magnitude;
    }

    public float GetMoveLength(string property_name, string path, AnimationState state, bool abs)
    {
        if (state == null)
            return 0f;

        float length = 0f;
        AnimationCurve data = GetEditorCurve(property_name, path, state);
        if (data != null)
        {
            foreach (var key in data.keys)
            {
                if (abs == true)
                    length = Mathf.Max(length, Mathf.Abs(key.value));
                else
                    length = Mathf.Min(key.value, length);
            }
        }
        return length;
    }

    public float GetMoveLengthAdditive(string property_name, string path, string additive_path, AnimationState state)
    {
        if (state == null)
            return 0f;

        float length = 0f;
        AnimationCurve data = GetEditorCurve(property_name, path, state);
        AnimationCurve additive_data = GetEditorCurve(property_name, additive_path, state);

        if (data != null && additive_data != null)
        {
            float frame_multi = 1 / state.clip.frameRate;
            int key_length = (int)(data.keys[data.length - 1].time * state.clip.frameRate);
            for (int frame = 0; frame < key_length; ++frame)
            {
                float time = frame * frame_multi;
                float value = data.Evaluate(time);
                float value_additive = additive_data.Evaluate(time);
                length = Mathf.Min(length, value_additive - value);
            }
        }
        return length;
    }

    AnimationCurve GetEditorCurve(string property_name, string path, AnimationState state)
    {
        EditorCurveBinding bind = new EditorCurveBinding();
        bind.path = path;
        bind.type = typeof(Transform);
        bind.propertyName = property_name;

        return AnimationUtility.GetEditorCurve(state.clip, bind);
    }

    void GetAllBones(List<Transform> bones, Transform transform)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            Transform bone = transform.GetChild(i);
            if (bone.GetComponent<SkinnedMeshRenderer>() != null)
                continue;

            bones.Add(bone);

            GetAllBones(bones, bone);
        }
    }
#endif
#endregion
}
