// Copyright (C) 2014 - 2015 Stephan Bouchard - All Rights Reserved
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;



public class SH_CharacterMaterial_Editor : MaterialEditor
{
    readonly private string ShadowAlphaKeyword = "_ShadowAlpha";
    readonly private string AlphaKeyword = "_Alpha";
    readonly private string CullKeyword = "_Cull";
    readonly private string UseAlphaKeyword = "USE_ALPHA";
    readonly private string GlossKeyword = "_GlossTex";

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

        float shadow_alpha = targetMaterial.GetFloat(ShadowAlphaKeyword);
        float new_shadow_alpha = EditorGUILayout.Slider("Shadow Alpha", shadow_alpha, 0f, 1f);
        if (new_shadow_alpha != shadow_alpha)
        {
            targetMaterial.SetFloat(ShadowAlphaKeyword, new_shadow_alpha);
            EditorUtility.SetDirty(target);
        }

        if (targetMaterial.HasProperty(AlphaKeyword) == true)
        {
            float alpha = targetMaterial.GetFloat(AlphaKeyword);
            float new_alpha = EditorGUILayout.Slider("Alpha", alpha, 0f, 1f);
            if (new_alpha != alpha)
            {
                targetMaterial.SetFloat(AlphaKeyword, new_alpha);
                EditorUtility.SetDirty(target);
            }
        }

        bool use_cull = targetMaterial.GetInt(CullKeyword) == 2;
        bool new_use_cull = EditorGUILayout.Toggle("Use Cull", use_cull);
        if (new_use_cull != use_cull)
        {
            targetMaterial.SetInt(CullKeyword, new_use_cull ? 2 : 0);
            EditorUtility.SetDirty(target);
        }

        bool use_alpha = targetMaterial.IsKeywordEnabled(UseAlphaKeyword);
        bool new_use_alpha = EditorGUILayout.Toggle("Use Alpha", use_alpha);
        if (new_use_alpha != use_alpha)
        {
            if (new_use_alpha)
                targetMaterial.EnableKeyword(UseAlphaKeyword);
            else
                targetMaterial.DisableKeyword(UseAlphaKeyword);
            EditorUtility.SetDirty(target);
        }

        if (targetMaterial.HasProperty(GlossKeyword) == true)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUIUtility.fieldWidth = 64;
            MaterialProperty gloss_tex = GetMaterialProperty(objs, "_GlossTex");
            TextureProperty(gloss_tex, "Gloss Texture", false);
            EditorGUIUtility.fieldWidth = 0;

            MaterialProperty gloss_color = GetMaterialProperty(objs, "_GlossColor");
            ColorProperty(gloss_color, "Gloss Color");

            MaterialProperty gloss_min = GetMaterialProperty(objs, "_GlossMin");
            RangeProperty(gloss_min, "Gloss Min");

            MaterialProperty gloss_max = GetMaterialProperty(objs, "_GlossMax");
            RangeProperty(gloss_max, "Gloss Max");

            MaterialProperty gloss_speed = GetMaterialProperty(objs, "_GlossSpeed");
            FloatProperty(gloss_speed, "Gloss Speed");

            if (EditorGUI.EndChangeCheck()) PropertiesChanged();
        }
    }
}
