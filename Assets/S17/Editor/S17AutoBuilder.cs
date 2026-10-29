#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// S17 homework only. Never save or modify the S16 source scene/assets.
public static class S17AutoBuilder
{
    private const string Folder = "Assets/S17";
    private const string Source = "Assets/S16/S16_Landscape.unity";
    private const string FinalScene = Folder + "/S17_Landscape.unity";
    private const string B2Scene = Folder + "/S17_Mission_B2.unity";
    private const string QuarterScene = Folder + "/S17_Mission_Quarter.unity";

    [MenuItem("Tools/CG S17/Build S17 Homework")]
    public static void Build()
    {
        if (!File.Exists(Source)) throw new Exception("S16 source scene missing: " + Source);
        if (File.Exists(FinalScene) || File.Exists(B2Scene) || File.Exists(QuarterScene))
            throw new Exception("S17 scene already exists. Aborting to preserve previous homework.");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        try
        {
            Texture2D grid = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/S17_Grid_PPT_512.png");
            if (grid == null) throw new Exception("Grid image missing from S17 folder.");
            string texPath = AssetDatabase.GetAssetPath(grid);
            TextureImporter ti = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Default;
                ti.sRGBTexture = true;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.filterMode = FilterMode.Point; // keep each labeled square sharp
                ti.mipmapEnabled = false;
                ti.SaveAndReimport();
                grid = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            }

            Material original = MakeTextureMaterial("Mat_S17_Texture_Reference", "S17_Texture", grid);
            Material d3 = MakeTextureMaterial("Mat_S17_MyTexture", "S17_MyTexture", grid);
            Material b2 = MakeTextureMaterial("Mat_S17_MyTexture_B2", "S17_MyTexture_B2", grid);
            Material quarter = MakeTextureMaterial("Mat_S17_MyTexture_Quarter", "S17_MyTexture_Quarter", grid);

            Scene scene = EditorSceneManager.OpenScene(Source, OpenSceneMode.Single);
            if (!scene.IsValid()) throw new Exception("Cannot open S16 source scene.");
            if (!EditorSceneManager.SaveScene(scene, FinalScene))
                throw new Exception("Cannot Save As into S17 folder.");

            // Hide old S16-only demonstration objects. Preserve the original S16 scene.
            Hide("S16_SimpleShader_Red");
            Hide("S16_EditableCoal_Amber");
            Hide("S03_CustomPolygonMesh_Square");
            Hide("CustomCubeMesh");
            Hide("Primitive_Capsule");
            Hide("S13_KeepMaterial");
            // The fluid props are S15 exercises, not required for the S17 texture comparison.
            foreach (GameObject o in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (o.scene.IsValid() && o.name.StartsWith("S15_LavaPool_", StringComparison.Ordinal))
                    o.SetActive(false);

            // Existing URP-Lit coal is our visual reference; it still has a lava texture.
            GameObject goal = Find("S16_GoalCoal_Lit");
            if (goal == null) throw new Exception("Goal Lit coal missing from S16 scene.");
            goal.SetActive(true);
            goal.transform.position = new Vector3(-1.15f, 1.80f, 2.7f);
            goal.transform.localScale = Vector3.one * 0.43f;

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "S17_Quad_D3";
            quad.transform.position = new Vector3(-0.35f, 1.83f, 2.7f);
            quad.transform.localScale = Vector3.one * 0.54f;
            quad.GetComponent<MeshRenderer>().sharedMaterial = d3;

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "S17_Sphere_D3";
            sphere.transform.position = new Vector3(0.37f, 1.80f, 2.7f);
            sphere.transform.localScale = Vector3.one * 0.43f;
            sphere.GetComponent<MeshRenderer>().sharedMaterial = d3;

            GameObject diamond = Find("CustomDiamondMesh");
            if (diamond == null) throw new Exception("CustomDiamondMesh missing from S16 scene.");
            diamond.SetActive(true);
            diamond.name = "S17_CustomDiamondMesh_D3";
            diamond.transform.position = new Vector3(1.13f, 1.80f, 2.7f);
            diamond.transform.localScale = Vector3.one * 0.31f;
            ApplyToRuntimeMesh(diamond, d3);

            ConfigureLightingProfile();
            CreateCamera();
            ApplyToThree(quad, sphere, diamond, d3);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            // Generate Lighting once, separated from the prior scene.
            try
            {
                bool bake = Lightmapping.Bake();
                Debug.Log("[S17] Generate Lighting: " + bake);
                EditorSceneManager.SaveScene(scene);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[S17] Light bake skipped; use Window > Rendering > Lighting > Generate Lighting manually: " + ex.Message);
                EditorSceneManager.SaveScene(scene);
            }

            // Save TWO additional runnable scenes as evidence that both PPT missions were redone.
            // saveAsCopy:true means the active scene remains S17_Landscape.
            ApplyToThree(quad, sphere, diamond, b2);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, B2Scene, true))
                throw new Exception("Cannot save B2 mission scene.");
            ApplyToThree(quad, sphere, diamond, quarter);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, QuarterScene, true))
                throw new Exception("Cannot save Quarter mission scene.");

            ApplyToThree(quad, sphere, diamond, d3);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[S17 AUTO SUCCESS] Final D3 scene + B2 mission + quarter mission + materials + shaders generated.");
            Debug.Log("[S17] Screenshot: Assets/S17/S17_Landscape.unity in Game view. Text: Assets/S17/S17_서술_3-5문장.txt");
        }
        catch (Exception ex)
        {
            Debug.LogError("[S17 AUTO FAILURE] " + ex);
            throw;
        }
    }

    private static Material MakeTextureMaterial(string materialName, string shaderFile, Texture2D tex)
    {
        string shaderPath = Folder + "/" + shaderFile + ".shader";
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        if (shader == null) throw new Exception("Shader not imported: " + shaderPath);
        Material mat = new Material(shader) { name = materialName };
        mat.SetTexture("_BaseMap", tex);
        string path = Folder + "/" + materialName + ".mat";
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static GameObject Find(string name)
    {
        foreach (GameObject obj in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (obj.scene.IsValid() && obj.name == name) return obj;
        return null;
    }

    private static void Hide(string name)
    {
        GameObject obj = Find(name);
        if (obj != null) obj.SetActive(false);
    }

    private static void ApplyToRuntimeMesh(GameObject go, Material mat)
    {
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();
        if (renderer == null) throw new Exception("Custom diamond MeshRenderer missing.");
        renderer.sharedMaterial = mat;
        // Old S13_KeepMaterial script can replace the material in Play mode.
        foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
        {
            if (behaviour == null || behaviour.GetType().Name != "S13_KeepMaterial") continue;
            SerializedObject so = new SerializedObject(behaviour);
            SerializedProperty prop = so.FindProperty("materialToKeep");
            if (prop != null)
            {
                prop.objectReferenceValue = mat;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        EditorUtility.SetDirty(go);
    }

    private static void ApplyToThree(GameObject quad, GameObject sphere, GameObject diamond, Material mat)
    {
        quad.GetComponent<MeshRenderer>().sharedMaterial = mat;
        sphere.GetComponent<MeshRenderer>().sharedMaterial = mat;
        ApplyToRuntimeMesh(diamond, mat);
    }

    private static void ConfigureLightingProfile()
    {
        LightingSettings old = null;
        try { old = Lightmapping.lightingSettings; } catch { }
        LightingSettings own = old != null ? UnityEngine.Object.Instantiate(old) : new LightingSettings();
        own.name = "S17_LightingSettings";
        AssetDatabase.CreateAsset(own, Folder + "/S17_LightingSettings.lighting");
        Lightmapping.lightingSettings = own;

        Volume volume = UnityEngine.Object.FindFirstObjectByType<Volume>(FindObjectsInactive.Include);
        if (volume != null)
        {
            VolumeProfile from = volume.sharedProfile;
            string path = Folder + "/S17_GlobalVolumeProfile.asset";
            string sourcePath = from != null ? AssetDatabase.GetAssetPath(from) : "";
            if (!string.IsNullOrEmpty(sourcePath) && AssetDatabase.CopyAsset(sourcePath, path))
                volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            else
            {
                VolumeProfile newProfile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(newProfile, path);
                volume.sharedProfile = newProfile;
            }
            volume.isGlobal = true;
            VolumeProfile profile = volume.sharedProfile;
            if (profile != null)
            {
                if (!profile.TryGet<Bloom>(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
                bloom.active = true;
                bloom.threshold.overrideState = true;
                bloom.threshold.value = 0.9f;
                bloom.intensity.overrideState = true;
                bloom.intensity.value = 1.5f;
                EditorUtility.SetDirty(profile);
            }
        }
    }

    private static void CreateCamera()
    {
        foreach (Camera cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            cam.enabled = false;
        GameObject go = new GameObject("S17_ScreenshotCamera");
        go.tag = "MainCamera";
        Camera camera = go.AddComponent<Camera>();
        go.transform.position = new Vector3(0f, 2.30f, 0.05f);
        go.transform.LookAt(new Vector3(0f, 1.80f, 2.72f), Vector3.up);
        camera.fieldOfView = 58f;
        camera.nearClipPlane = 0.02f;
        camera.farClipPlane = 1000f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.06f, 0.13f, 1f);
        camera.depth = 50f;
        UniversalAdditionalCameraData urp = go.AddComponent<UniversalAdditionalCameraData>();
        urp.renderPostProcessing = true;
        // No extra AudioListener: existing Player/Main Camera already contains one.
    }
}
#endif

