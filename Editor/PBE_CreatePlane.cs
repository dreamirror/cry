//////////////////////////////////////////////////////////////////////////
//
// Create custom plane and export
// 
// Created by LCY.
//
// Copyright 2010 UpaRupa.Inc
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PBE_CreatePlane : ScriptableWizard
{
    public enum Orientation { Horizontal, Vertical }
    public int widthSegments = 1;
    public int lengthSegments = 1;
    public float width = 1.0f;
    public float length = 1.0f;
    public Orientation orientation = Orientation.Horizontal;
    public bool addCollider = false;
    public bool createAtOrigin = false;
    public string optionalName;
    static Camera cam;
    static Camera lastUsedCam;

    [MenuItem("PBEditor/Create Custom Plane...")]
    static void CreateWizard()
    {
        cam = Camera.current;
        // Hack because camera.current doesn't return editor camera if scene view doesn't have focus
        if (!cam)
            cam = lastUsedCam;
        else
            lastUsedCam = cam;
        ScriptableWizard.DisplayWizard("Create Plane", typeof(PBE_CreatePlane));
    }

    [MenuItem("PBEditor/Create Split Image...")]
    static void CreateSplitImage()
    {
        if (Selection.objects == null || Selection.objects.Length == 0)
            return;

        Color32[] delete_colors = new Color32[1024];
        Color32 delete_color = new Color32(0, 0, 0, 0);
        for (int i = 0; i < 1024; ++i)
        {
            delete_colors[i] = delete_color;
        }

        foreach (Object obj in Selection.objects)
        {
            if (obj != null && obj is Texture2D)
            {
                string texture_path = AssetDatabase.GetAssetPath(obj);

                TextureImporter importer = AssetImporter.GetAtPath(texture_path) as TextureImporter;

                importer.textureType = TextureImporterType.Advanced;
                importer.isReadable = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.mipmapEnabled = false;
                importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                importer.filterMode = FilterMode.Bilinear;
                importer.spriteImportMode = SpriteImportMode.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                AssetDatabase.ImportAsset(texture_path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(texture_path);

                if (image.width != 1280 || image.height != 720)
                {
                    Debug.LogErrorFormat("Image({0}) should be 1280x720", image.name);
                    return;
                }
                Color[] colors_main = image.GetPixels(0, 0, 1001, 720, 0);
                Color[] colors_sub = image.GetPixels(999, 0, 281, 720, 0);
                image.Resize(1024, 1024, importer.DoesSourceTextureHaveAlpha() ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
                for (int i = 0; i < 1024; ++i)
                {
                    image.SetPixels32(0, i, 1024, 1, delete_colors);
                }
                image.SetPixels(0, 1024 - 720, 1001, 720, colors_main);
                for (int y = 0; y < 720; ++y)
                {
                    for (int x = 0; x < 281; ++x)
                    {
                        image.SetPixel(y, x, colors_sub[y * 281 + 280 - x]);
                    }
                }
                image.Apply(false, false);

                System.IO.File.WriteAllBytes(texture_path, image.EncodeToPNG());

                EditorUtility.SetDirty(image);

                importer.isReadable = false;
                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                AssetDatabase.ImportAsset(texture_path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                Debug.LogFormat("[{0}] Converted", image.name);
            }
        }
    }

    [MenuItem("PBEditor/Create Split Plane...")]
    static void CreateSplit()
    {
        GameObject plane = new GameObject();
        plane.name = "SplitPlane";
        plane.transform.position = Vector3.zero;
        string meshPrefabPath = "Assets/Editor/" + plane.name + ".asset";
        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        plane.AddComponent(typeof(MeshRenderer));

        Mesh m = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));

        if (m == null)
        {
            m = new Mesh();
            m.name = plane.name;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            int triangle_index = 0;
            vertices.Add(new Vector3(-1280 / 2, 720 / 2, 0));
            vertices.Add(new Vector3(-1280 / 2 + 1000, 720 / 2, 0));
            vertices.Add(new Vector3(-1280 / 2, -720 / 2, 0));
            vertices.Add(new Vector3(-1280 / 2 + 1000, -720 / 2, 0));

            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1000f / 1024f, 1));
            uvs.Add(new Vector2(0, 1f - 720f / 1024f));
            uvs.Add(new Vector2(1000f / 1024f, 1f - 720f / 1024f));

            triangles.Add(triangle_index + 0);
            triangles.Add(triangle_index + 1);
            triangles.Add(triangle_index + 2);
            triangles.Add(triangle_index + 3);
            triangles.Add(triangle_index + 2);
            triangles.Add(triangle_index + 1);

            triangle_index = 4;
            vertices.Add(new Vector3(-1280 / 2 + 1000, 720 / 2, 0));
            vertices.Add(new Vector3(1280 / 2, 720 / 2, 0));
            vertices.Add(new Vector3(-1280 / 2 + 1000, -720 / 2, 0));
            vertices.Add(new Vector3(1280 / 2, -720 / 2, 0));

            uvs.Add(new Vector2(720f / 1024f, 280f /1024f));
            uvs.Add(new Vector2(720f / 1024f, 0f));
            uvs.Add(new Vector2(0f, 280f / 1024f));
            uvs.Add(new Vector2(0f, 0f));

            triangles.Add(triangle_index + 0);
            triangles.Add(triangle_index + 1);
            triangles.Add(triangle_index + 2);
            triangles.Add(triangle_index + 3);
            triangles.Add(triangle_index + 2);
            triangles.Add(triangle_index + 1);

            //             for (float y = 0.0f; y < vCount2; y++)
            //             {
            //                 for (float x = 0.0f; x < hCount2; x++)
            //                 {
            //                     if (orientation == Orientation.Horizontal)
            //                     {
            //                         vertices[index] = new Vector3(x * scaleX + realX, 0.0f, y * scaleY + realY);
            //                     }
            //                     else
            //                     {
            //                         vertices[index] = new Vector3(x * scaleX + realX, y * scaleY + realY, 0.0f);
            //                     }
            // 
            //                     uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
            //                 }
            //             }
            // 
            //             index = 0;
            //             for (int y = 0; y < lengthSegments; y++)
            //             {
            //                 for (int x = 0; x < widthSegments; x++)
            //                 {
            //                     triangles[index] = (y * hCount2) + x;
            //                     triangles[index + 1] = ((y + 1) * hCount2) + x;
            //                     triangles[index + 2] = (y * hCount2) + x + 1;
            // 
            //                     triangles[index + 3] = ((y + 1) * hCount2) + x;
            //                     triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
            //                     triangles[index + 5] = (y * hCount2) + x + 1;
            //                     index += 6;
            //                 }
            //             }

            m.vertices = vertices.ToArray();
            m.uv = uvs.ToArray();
            m.triangles = triangles.ToArray();
            //            m.RecalculateNormals();

            AssetDatabase.CreateAsset(m, meshPrefabPath);
            AssetDatabase.SaveAssets();
        }

        meshFilter.sharedMesh = m;
        m.RecalculateBounds();

        Selection.activeObject = plane;
    }

    void OnWizardUpdate()
    {
        widthSegments = Mathf.Clamp(widthSegments, 1, 254);
        lengthSegments = Mathf.Clamp(lengthSegments, 1, 254);
    }

    void OnWizardCreate()
    {
        GameObject plane = new GameObject();

        if (!string.IsNullOrEmpty(optionalName))
            plane.name = optionalName;
        else
            plane.name = "Plane";

        if (!createAtOrigin && cam)
            plane.transform.position = cam.transform.position + cam.transform.forward * 5.0f;
        else
            plane.transform.position = Vector3.zero;

        string meshPrefabPath = "Assets/Editor/" + plane.name + widthSegments + "x" + lengthSegments + "W" + width + "L" + length + (orientation == Orientation.Horizontal ? "H" : "V") + ".asset";

        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        plane.AddComponent(typeof(MeshRenderer));

        Mesh m = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));

        if (m == null)
        {
            m = new Mesh();
            m.name = plane.name;

            int hCount2 = widthSegments + 1;
            int vCount2 = lengthSegments + 1;
            int numTriangles = widthSegments * lengthSegments * 6;
            int numVertices = hCount2 * vCount2;

            Vector3[] vertices = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] triangles = new int[numTriangles];

            int index = 0;
            float uvFactorX = 1.0f / widthSegments;
            float uvFactorY = 1.0f / lengthSegments;
            float scaleX = width / widthSegments;
            float scaleY = length / lengthSegments;

            float halfWidth = width / 2.0f;
            float halfHeight = length / 2.0f;

            float realX = -halfWidth;
            float realY = -halfHeight;

            for (float y = 0.0f; y < vCount2; y++)
            {
                for (float x = 0.0f; x < hCount2; x++)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        vertices[index] = new Vector3(x * scaleX + realX, 0.0f, y * scaleY + realY);
                    }
                    else
                    {
                        vertices[index] = new Vector3(x * scaleX + realX, y * scaleY + realY, 0.0f);
                    }

                    uvs[index++] = new Vector2(x * uvFactorX, y * uvFactorY);
                }
            }

            index = 0;
            for (int y = 0; y < lengthSegments; y++)
            {
                for (int x = 0; x < widthSegments; x++)
                {
                    triangles[index] = (y * hCount2) + x;
                    triangles[index + 1] = ((y + 1) * hCount2) + x;
                    triangles[index + 2] = (y * hCount2) + x + 1;

                    triangles[index + 3] = ((y + 1) * hCount2) + x;
                    triangles[index + 4] = ((y + 1) * hCount2) + x + 1;
                    triangles[index + 5] = (y * hCount2) + x + 1;
                    index += 6;
                }
            }

            m.vertices = vertices;
            m.uv = uvs;
            m.triangles = triangles;
            m.RecalculateNormals();

            AssetDatabase.CreateAsset(m, meshPrefabPath);
            AssetDatabase.SaveAssets();
        }

        meshFilter.sharedMesh = m;
        m.RecalculateBounds();

        if (addCollider)
            plane.AddComponent(typeof(BoxCollider));

        Selection.activeObject = plane;
    }
}