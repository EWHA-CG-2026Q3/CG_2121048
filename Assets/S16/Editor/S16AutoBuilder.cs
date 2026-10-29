#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// This script edits ONLY new S16 / S16_Abs scenes and assets.
// Original S15_Landscape is opened read-only as a template; it is never saved.
public static class S16AutoBuilder
{
    private const string Source = "Assets/S15/S15_Landscape.unity";
    private const string NormalScene = "Assets/S16/S16_Landscape.unity";
    private const string AbsScene = "Assets/S16_Abs/S16_Abs_Landscape.unity";

    [MenuItem("Tools/CG S16/Build S16 + S16_Abs")]
    public static void BuildBoth()
    {
        if (!File.Exists(Source))
            throw new Exception("S15_Landscape.unity was not found. Aborting.");
        if (File.Exists(NormalScene) || File.Exists(AbsScene))
            throw new Exception("Existing S16 scene found. Refusing to overwrite it.");

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        try
        {
            MakeNormal();
            MakeAbs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[S16 AUTO SUCCESS] S16 and S16_Abs scenes, shaders, materials, lighting profiles and cameras were created.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[S16 AUTO FAILURE] " + ex);
            throw;
        }
    }

    private static Material MakeMaterial(string path, string shaderPath, Color baseColor, Color emissionColor)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        if (shader == null || shader.isSupported == false)
            throw new Exception("Shader missing or unsupported: " + shaderPath);

        Material mat = new Material(shader);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", emissionColor);
        mat.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static GameObject FindSceneObject(string name)
    {
        foreach (GameObject go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (go.scene.IsValid() && go.name == name) return go;
        }
        return null;
    }

    private static void ApplyToMesh(GameObject go, Material mat)
    {
        if (go == null) throw new Exception("Required custom mesh not found in S15 scene.");
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        // S13_KeepMaterial overwrites runtime material in Start/LateUpdate.
        // Update that component's serialized reference as well.
        foreach (MonoBehaviour behaviour in go.GetComponents<MonoBehaviour>())
        {
            if (behaviour == null || behaviour.GetType().Name != "S13_KeepMaterial") continue;
            SerializedObject serialized = new SerializedObject(behaviour);
            SerializedProperty p = serialized.FindProperty("materialToKeep");
            if (p != null)
            {
                p.objectReferenceValue = mat;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
        EditorUtility.SetDirty(go);
        EditorUtility.SetDirty(mr);
    }

    private static GameObject MakeSphere(string name, Vector3 position, float diameter, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = Vector3.one * diameter;
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        return go;
    }

    private static void ConfigureBloomAndLighting(string folder, string prefix)
    {
        Volume volume = UnityEngine.Object.FindFirstObjectByType<Volume>(FindObjectsInactive.Include);
        if (volume != null)
        {
            VolumeProfile existing = volume.sharedProfile;
            string profilePath = folder + "/" + prefix + "_GlobalVolumeProfile.asset";
            VolumeProfile profile;
            string oldPath = existing != null ? AssetDatabase.GetAssetPath(existing) : "";
            if (!string.IsNullOrEmpty(oldPath))
            {
                // Clone the profile asset INCLUDING Bloom sub-assets; do not edit S15's profile.
                if (!AssetDatabase.CopyAsset(oldPath, profilePath))
                    throw new Exception("Cannot copy volume profile to " + profilePath);
                AssetDatabase.ImportAsset(profilePath, ImportAssetOptions.ForceSynchronousImport);
                profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
                if (profile == null) throw new Exception("Copied VolumeProfile failed to load.");
            }
            else
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = prefix + "_GlobalVolumeProfile";
                AssetDatabase.CreateAsset(profile, profilePath);
            }
            volume.sharedProfile = profile;
            volume.isGlobal = true;
            volume.weight = 1f;

            if (!profile.TryGet<Bloom>(out Bloom bloom)) bloom = profile.Add<Bloom>(true);
            bloom.active = true;
            bloom.threshold.overrideState = true;
            bloom.threshold.value = 0.9f;
            bloom.intensity.overrideState = true;
            bloom.intensity.value = 2.0f;
            EditorUtility.SetDirty(profile);
            EditorUtility.SetDirty(volume);
        }

        // Separate lighting settings for each new scene; no edits to S15 asset.
        LightingSettings settings = null;
        try { settings = Lightmapping.lightingSettings; } catch { }
        LightingSettings fresh = settings != null
            ? UnityEngine.Object.Instantiate(settings)
            : new LightingSettings();
        fresh.name = prefix + "_LightingSettings";
        AssetDatabase.CreateAsset(fresh, folder + "/" + fresh.name + ".lighting");
        Lightmapping.lightingSettings = fresh;
    }

    private static void CreateSubmissionCamera(string name, Vector3 position, Vector3 focus, float fov)
    {
        foreach (Camera cam in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            cam.enabled = false;

        GameObject cameraGo = new GameObject(name);
        Camera camera = cameraGo.AddComponent<Camera>();
        cameraGo.tag = "MainCamera";
        camera.transform.position = position;
        camera.transform.LookAt(focus, Vector3.up);
        camera.fieldOfView = fov;
        camera.nearClipPlane = 0.03f;
        camera.farClipPlane = 1000f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.06f, 0.13f, 1f);
        camera.depth = 50f;

        UniversalAdditionalCameraData extra = cameraGo.AddComponent<UniversalAdditionalCameraData>();
        extra.renderPostProcessing = true;
        // Do not add a second AudioListener: the original Player camera owns it.
    }

    private static Scene StartFromS15(string dest)
    {
        Scene scene = EditorSceneManager.OpenScene(Source, OpenSceneMode.Single);
        if (!scene.IsValid()) throw new Exception("Could not open S15 scene.");
        // Save As, not Save Copy: the opened Scene becomes the NEW file.
        if (!EditorSceneManager.SaveScene(scene, dest))
            throw new Exception("Could not save new scene: " + dest);
        return scene;
    }

    private static void SaveAndBake(Scene scene, string label)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        try
        {
            bool baked = Lightmapping.Bake();
            Debug.Log("[S16 AUTO] Generate Lighting " + label + ": " + (baked ? "OK" : "returned false"));
            EditorSceneManager.SaveScene(scene);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[S16 AUTO] Lighting bake could not finish for " + label + ". Open Lighting > Generate Lighting manually if needed. " + ex.Message);
            EditorSceneManager.SaveScene(scene);
        }
    }

    private static void MakeNormal()
    {
        Material minimal = MakeMaterial("Assets/S16/Mat_S16_MyShader.mat", "Assets/S16/S16_MyShader.shader",
            Color.black, Color.black);
        Material amber = MakeMaterial("Assets/S16/Mat_S16_MyCoal.mat", "Assets/S16/S16_MyCoal.shader",
            new Color(0.025f, 0.01f, 0.01f, 1f), new Color(2.0f, 0.55f, 0.09f, 1f));
        Material violet = MakeMaterial("Assets/S16/Mat_S16_MyCoal_Violet.mat", "Assets/S16/S16_MyCoal.shader",
            new Color(0.04f, 0.01f, 0.05f, 1f), new Color(1.15f, 0.30f, 2.75f, 1f));

        Scene scene = StartFromS15(NormalScene);
        ConfigureBloomAndLighting("Assets/S16", "S16");

        Material referenceCoal = AssetDatabase.LoadAssetAtPath<Material>("Assets/S13/Mat_S13_Coal.mat");
        if (referenceCoal == null)
            referenceCoal = AssetDatabase.LoadAssetAtPath<Material>("Assets/S15/Mat_S15_Coal.mat");
        if (referenceCoal == null) throw new Exception("Reference coal material from S13/S15 not found.");

        // Three distinct spheres: reference Lit coal, simple hard-coded shader, editable HDR coal.
        MakeSphere("S16_GoalCoal_Lit", new Vector3(-0.53f, 1.75f, 2.65f), 0.31f, referenceCoal);
        MakeSphere("S16_SimpleShader_Red", new Vector3(0f, 1.75f, 2.65f), 0.31f, minimal);
        MakeSphere("S16_EditableCoal_Amber", new Vector3(0.53f, 1.75f, 2.65f), 0.31f, amber);

        GameObject polygon = FindSceneObject("S03_CustomPolygonMesh_Square");
        GameObject diamond = FindSceneObject("CustomDiamondMesh");
        if (polygon == null || diamond == null) throw new Exception("S03 polygon or S04 diamond is missing from the S15 scene.");
        polygon.transform.position = new Vector3(-1.02f, 1.72f, 2.52f);
        diamond.transform.position = new Vector3(1.02f, 1.72f, 2.52f);
        ApplyToMesh(polygon, amber);
        ApplyToMesh(diamond, violet);

        CreateSubmissionCamera("S16_ScreenshotCamera",
            new Vector3(-0.15f, 2.27f, 0.65f), new Vector3(0f, 1.78f, 2.80f), 55f);
        SaveAndBake(scene, "S16");
    }

    private static void MakeAbs()
    {
        Material glow = MakeMaterial("Assets/S16_Abs/Mat_S16_MyGlow.mat", "Assets/S16_Abs/S16_MyGlow.shader",
            new Color(0.005f, 0.015f, 0.10f, 1f), new Color(0.20f, 0.85f, 4.5f, 1f));

        Scene scene = StartFromS15(AbsScene);
        ConfigureBloomAndLighting("Assets/S16_Abs", "S16_Abs");

        GameObject diamond = FindSceneObject("CustomDiamondMesh");
        if (diamond == null) throw new Exception("CustomDiamondMesh is missing from S15.");
        diamond.transform.position = new Vector3(0.20f, 1.83f, 2.55f);
        diamond.transform.localScale = Vector3.one * 0.32f;
        ApplyToMesh(diamond, glow);

        CreateSubmissionCamera("S16_Abs_ScreenshotCamera",
            new Vector3(0.18f, 2.15f, 0.90f), new Vector3(0.20f, 1.80f, 2.60f), 48f);
        SaveAndBake(scene, "S16_Abs");
    }
}
#endif
