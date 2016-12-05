// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;



public class SH_CharacterCutsceneMaterial_Editor : MaterialEditor
{
    readonly private string ShadowKeyword = "_Shadow";
    readonly private string UseShadowKeyword = "USE_SHADOW";

    public override void OnInspectorGUI()
    {
        Material targetMaterial = target as Material;
        if (target == null)
            return;

        Object[] objs = new Object[] { targetMaterial };

        EditorGUI.BeginChangeCheck();
        EditorGUIUtility.fieldWidth = 64;
        MaterialProperty main_tex = GetMaterialProperty(objs, "_MainTex");
        TextureProperty(main_tex, "Main Texture", false);
        EditorGUIUtility.fieldWidth = 0;
        if (EditorGUI.EndChangeCheck()) PropertiesChanged();

        float shadow = targetMaterial.GetFloat(ShadowKeyword);
        float new_shadow = EditorGUILayout.FloatField("Shadow", shadow);
        if (new_shadow != shadow)
        {
            targetMaterial.SetFloat(ShadowKeyword, new_shadow);
            EditorUtility.SetDirty(target);
        }

        bool use_shadow = targetMaterial.IsKeywordEnabled(UseShadowKeyword);
        bool new_use_shadow = EditorGUILayout.Toggle("Use Shadow", use_shadow);
        if (new_use_shadow != use_shadow)
        {
            if (new_use_shadow)
                targetMaterial.EnableKeyword(UseShadowKeyword);
            else
                targetMaterial.DisableKeyword(UseShadowKeyword);
            EditorUtility.SetDirty(target);
        }
    }
}
