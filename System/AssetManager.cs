using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HeroFX;
using System.Xml;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum eUICharacterAssetType
{
    character,
    f1,//skill1
    f2,//skill2
}
public enum eShader
{
    character,
    character_gloss,
    character_alpha,
}

public enum eUIAtlasType
{
    ui_skill_icon,
    ui_character,
}

public enum eAssetBundleType
{
    character,
    sound,
    particle,
}

#if UNITY_5


public class AssetData
{
    public GameObject Prefab { get; private set; }
    public List<GameObject> UsingList { get; private set; } //列表
    public Stack<GameObject> FreeList { get; private set; }//栈

    public AssetData(GameObject prefab) //构造函数
    {
        Prefab = prefab;
        UsingList = new List<GameObject>();
        FreeList = new Stack<GameObject>();

        var prefab_component = prefab.GetComponent<IAssetObject>();
        prefab_component.InitPrefab();
    }

    public T Alloc<T>() where T : Component
    {
        GameObject asset = null;
        if (FreeList.Count > 0)
            asset = FreeList.Pop();
        if (asset == null)
            asset = GameObject.Instantiate<GameObject>(Prefab);
        UsingList.Add(asset);
        asset.gameObject.SetActive(true);

        if (AssetManager.OnChanged != null) AssetManager.OnChanged();

        return asset.GetComponent<T>();
    }

    public void Free<T>(T asset) where T : Component
    {
        UsingList.Remove(asset.gameObject);
        FreeList.Push(asset.gameObject);
        asset.gameObject.SetActive(false);
        if (typeof(T).Name == "Character")
        {
            asset.gameObject.transform.localPosition = Vector3.zero;
            asset.gameObject.transform.localRotation = Quaternion.identity;
            asset.gameObject.transform.localScale = Vector3.one;
        }

        if (AssetManager.OnChanged != null) AssetManager.OnChanged();
    }
}

public class AssetManager {

    static Dictionary<string, AssetData> m_Characters = new Dictionary<string, AssetData>();
    static Dictionary<string, AssetData> m_CharacterSkins = new Dictionary<string, AssetData>();
    static Dictionary<string, AssetData> m_CharacterCutScenes = new Dictionary<string, AssetData>();
    static public Dictionary<string, AssetData> Characters { get { return m_Characters; } }
    static public Dictionary<string, AssetData> CharacterSkins { get { return m_CharacterSkins; } }
    static public Dictionary<string, AssetData> CharacterCutScenes { get { return m_CharacterCutScenes; } }

    static Dictionary<string, AudioClip> m_Sounds = new Dictionary<string, AudioClip>();

    static public Action OnChanged = null;

    //static AssetBundle CharacterAssetBundle = null;
    //static AssetBundle SoundAssetBundle = null;
    //static AssetBundle ParticleAssetBundle = null;
#if UNITY_EDITOR
    public static string AssetURL { get { return Directory.GetParent(Application.dataPath).Parent.FullName + "/Bundle/DEV/StandaloneWindows/"; } }
    public static string AssetPath { get { return Directory.GetParent(Application.dataPath).Parent.FullName + "/Bundle/Download/"; } }

#else
    public static string AssetPath { get { return string.Format("{0}/",Application.temporaryCachePath); } }
#endif

    //------------------------------------------------------------------------------------------
    static public string GetPlatformString()
    {
        
#if UNITY_IOS
        return "iOS";
#elif UNITY_ANDROID
        return "Android";
#else
        return "StandaloneWindows";
#endif
    }
    //------------------------------------------------------------------------------------------
    //------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------


    //private
    //------------------------------------------------------------------------------------------
    static GameObject LoadAsset(AssetBundle bundle, string name)
    {
        GameObject obj = null;
        if (bundle != null)
        {
            obj = bundle.LoadAsset(name, typeof(GameObject)) as GameObject;
            if (obj == null)
                Debug.LogFormat("Load Asset failed! - {0}, {1}", bundle, name);
        }
        return obj;
    }
    //---------------------------------------------------------------------------
    //---------------------------------------------------------------------------
    //---------------------------------------------------------------------------
    //static Dictionary<string, AssetBundle> Bundles = new Dictionary<string, AssetBundle>();

    static AssetBundle AllAssetBundle = null;
    static void LoadFromFile()
    {
#if UNITY_EDITOR
        string strPath = string.Format("{0}{1}", AssetURL, AssetBundleFilename);
#else
        string strPath = string.Format("{0}{1}", AssetPath, AssetBundleFilename);
#endif
        if (AllAssetBundle == null)
        {
            AllAssetBundle = AssetBundle.LoadFromFile(strPath);
        }
        if (AllAssetBundle == null)
            throw new Exception(string.Format("Failed to CreateFromFile() : {0}", strPath));
    }
    static T LoadInternal<T>(string name)
    {
        if (AllAssetBundle == null)
            LoadFromFile();

        GameObject obj = LoadAsset(AllAssetBundle, name);
        return obj.GetComponent<T>();
    }

    public static string AssetBundleFilename = "sh_assetbundle.unity3d";
    static public UIAtlas LoadUIAtlas()
    {
        return LoadInternal<UIAtlas>("SHUIAtlas");
    }

    static public UIAtlas LoadUIAtlas_Sub1()
    {
        return LoadInternal<UIAtlas>("SHUIAtlas_Sub1");
    }

    static public UIAtlas LoadUIBattleAtlas()
    {
        return LoadInternal<UIAtlas>("SHBattle");
    }
    static public UIAtlas LoadGuildEmblem()
    {
        return LoadInternal<UIAtlas>("SHGuildEmblem");
    }
    static public UIAtlas LoadCreatureAtlas()
    {
        return LoadInternal<UIAtlas>("SHCreature");
    }

    static public UIAtlas LoadTitleAtlas()
    {
        return LoadInternal<UIAtlas>("SHTitle");
    }

    static public UIAtlas LoadStoreAtlas()
    {
        return LoadInternal<UIAtlas>("SHStore");
    }

    static public UIAtlas LoadMainMenuAtlas()
    {
        return LoadInternal<UIAtlas>("SHMainMenu");
    }

    static public UIAtlas LoadStuffAtlas(bool is_gray = false)
    {
        if (is_gray)
            return LoadInternal<UIAtlas>("SHStuff_gray");
        return LoadInternal<UIAtlas>("SHStuff");
    }

    static public UIAtlas LoadEquipAtlas()
    {
        return LoadInternal<UIAtlas>("SHEquip");
    }
    static public UIAtlas LoadSkillAtlas()
    {
        return LoadInternal<UIAtlas>("SHSkill");
    }
    static public UIAtlas LoadTutorialAtlas()
    {
        return LoadInternal<UIAtlas>("SHTutorial");
    }
    static public UIAtlas LoadProfileAtlas()
    {
        return LoadInternal<UIAtlas>("SHPFCreature");
    }
    static public UIAtlas LoadMissionAtlas()
    {
        return LoadInternal<UIAtlas>("SHMission");
    }
    static public UIAtlas LoadCharacterShotsAtlas()
    {
        return LoadInternal<UIAtlas>("SHCharacterShots");
    }

    //////////////////////////////////////////////////////////////////////////////
//    static AssetBundle BGBundle = null;
    static public Texture2D LoadBG(string name)
    {
        LoadFromFile();
        return AllAssetBundle.LoadAsset<Texture2D>(name);
    }

    //---------------------------------------------------------------------------

    static public GameObject GetAssetBundle(eAssetBundleType type, string name)
    {
        GameObject res = null;
        switch(type)
        {
            case eAssetBundleType.character:
                res = GetCharacterPrefab(name).gameObject;
                break;
        }

        return res;
    }

    //---------------------------------------------------------------------------
    // character

    static public bool ContainsCharacterData(string name)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_Characters.TryGetValue(name, out data) == false)
        {
            LoadFromFile();
            return AllAssetBundle.Contains(name);
        }
        return true;
    }

    static public void LoadCharacterPrefab(string name)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_Characters.ContainsKey(name) == false)
        {
            LoadFromFile();

            if (AllAssetBundle.Contains(name) == false)
            {
                Debug.LogErrorFormat("Not exists character in LoadCharacterPrefab : {0}", name);
            }
            else
            {
                GameObject prefab = AllAssetBundle.LoadAsset<GameObject>(name + ".prefab");
                data = new AssetData(prefab);
                m_Characters.Add(name, data);

                Debug.LogFormat("LoadCharacterPrefab : {0}", name);
            }
        }
    }


    static public AssetData GetCharacterData(string name, bool load = false)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_Characters.TryGetValue(name, out data) == false)
        {
            if (load == false)
                Debug.LogErrorFormat("Not exists : {0}", name);

            LoadFromFile();

            if (AllAssetBundle.Contains(name) == false)
            {
                Debug.LogErrorFormat("Not exists character in GetCharacterData : {0}", name);
            }
            else
            {
                GameObject prefab = AllAssetBundle.LoadAsset<GameObject>(name + ".prefab");
                data = new AssetData(prefab);
                m_Characters.Add(name, data);
                Debug.LogFormat("GetCharacterData : {0}", name);
            }
        }
        return data;
    }

    //---------------------------------------------------------------------------
    // character skin
    static public bool ContainsCharacterSkinData(string name)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_CharacterSkins.TryGetValue(name, out data) == false)
        {
            LoadFromFile();
            return AllAssetBundle.Contains(name);
        }
        return true;
    }

    static public void LoadCharacterSkinPrefab(string name)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_CharacterSkins.ContainsKey(name) == false)
        {
            LoadFromFile();

            if (AllAssetBundle.Contains(name) == false)
            {
                Debug.LogErrorFormat("Not exists character skin in LoadCharacterSkinPrefab : {0}", name);
            }
            else
            {
                GameObject prefab = AllAssetBundle.LoadAsset<GameObject>(name + ".prefab");
                data = new AssetData(prefab);
                m_CharacterSkins.Add(name, data);

                Debug.LogFormat("LoadCharacterSkinPrefab : {0}", name);
            }
        }
    }


    static public AssetData GetCharacterSkinData(string name, bool load = false)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_CharacterSkins.TryGetValue(name, out data) == false)
        {
            if (load == false)
                Debug.LogErrorFormat("Not exists : {0}", name);

            LoadFromFile();

            if (AllAssetBundle.Contains(name) == false)
            {
                Debug.LogErrorFormat("Not exists character skin in GetCharacterSkinData : {0}", name);
            }
            else
            {
                GameObject prefab = AllAssetBundle.LoadAsset<GameObject>(name + ".prefab");
                data = new AssetData(prefab);
                m_CharacterSkins.Add(name, data);
                Debug.LogFormat("GetCharacterSkinData : {0}", name);
            }
        }
        return data;
    }

    //---------------------------------------------------------------------------
    // character cutscene

    static public bool ContainsCharacterCutSceneData(string name)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_CharacterCutScenes.TryGetValue(name, out data) == false)
        {
            LoadFromFile();
            return AllAssetBundle.Contains(name);
        }
        return true;
    }

    static public AssetData GetCharacterCutSceneData(string name, bool load = false)
    {
        name = name.ToLower();

        AssetData data = null;
        if (m_CharacterCutScenes.TryGetValue(name, out data) == false)
        {
            if (load == false)
                Debug.LogErrorFormat("Not exists : {0}", name);

            LoadFromFile();

            if (AllAssetBundle.Contains(name) == false)
            {
                Debug.LogErrorFormat("Not exists character cutscene in GetCharacterCutSceneData : {0}", name);
            }
            else
            {
                GameObject prefab = AllAssetBundle.LoadAsset<GameObject>(name + ".prefab");
                data = new AssetData(prefab);
                m_CharacterCutScenes.Add(name, data);
                Debug.LogFormat("GetCharacterCutSceneData : {0}", name);
            }
        }
        return data;
    }

    static public GameObject GetCharacterPrefab(string name)
    {
        AssetData data = GetCharacterData(name, true);
        return data.Prefab;
    }

    static public CharacterAssetContainer GetCharacterAsset(string name, string skin_name)
    {
        name = name.ToLower();

        AssetData data = GetCharacterData(name, true);
        return new CharacterAssetContainer(data, skin_name);
    }

    static public AssetContainer<CharacterSkin> GetCharacterSkinAsset(string model_name, string skin_name)
    {
        string name = string.Format("{0}_skin_{1}", model_name, skin_name);

        AssetData data = GetCharacterSkinData(name, true);
        return new AssetContainer<CharacterSkin>(data);
    }

    static public AssetContainer<CharacterCutScene> GetCharacterCutSceneAsset(string name)
    {
        name = name.ToLower();

        AssetData data = GetCharacterCutSceneData(name, true);
        return new AssetContainer<CharacterCutScene>(data);
    }

    static public AudioClip GetSound(string name)
    {
        name = name.ToLower();

        AudioClip audioClip = null;
        if (m_Sounds.TryGetValue(name, out audioClip) == false)
        {
            LoadFromFile();

            audioClip = AllAssetBundle.LoadAsset<AudioClip>(name);
            m_Sounds.Add(name, audioClip);
        }
        return audioClip;
    }

    static public HFX_ParticleSystem GetParticleSystem(string name)
    {
        name = name.ToLower();

        LoadFromFile();

        HFX_ParticleSystem res = null;
        {
            res = AllAssetBundle.LoadAsset<GameObject>(name).GetComponent<HFX_ParticleSystem>();
//             res.RefreshShader();
        }

        return res;
    }

    static public int bundleVersion = -1;
}
#endif
