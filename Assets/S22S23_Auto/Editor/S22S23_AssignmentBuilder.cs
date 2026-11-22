using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Unity.Mathematics;

// One-click builder for the student's existing CG_2121048 project.
// It never modifies S17/S16/Mid scenes; it works on new S22 and S23 copies.
public static class S22S23_AssignmentBuilder
{
    private const string S22 = "Assets/S22/S22_Landscape.unity";
    private const string S23 = "Assets/S23/S23_Landscape.unity";
    private const string S23Abs = "Assets/S23_Abs/S23_Abs_Landscape.unity";

    [MenuItem("Tools/Chaewon S22-S23/1. Build Both Scenes")]
    public static void BuildBoth()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("S22/S23", "Play 모드를 종료하고 다시 눌러 줘!", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureFolder("Assets/S22");
        EnsureFolder("Assets/S23");
        EnsureFolder("Assets/S23_Abs");

        // On repeated runs, keep the student's existing S22 and S23 edits.
        bool createdS22 = !AssetDatabase.LoadAssetAtPath<SceneAsset>(S22);
        if (createdS22)
        {
            string start = FindBestStartingScene();
            if (!string.IsNullOrEmpty(start))
            {
                if (!AssetDatabase.CopyAsset(start, S22))
                    throw new InvalidOperationException("기존 씬을 S22로 복사하지 못했습니다: " + start);
                AssetDatabase.Refresh();
            }
        }

        Scene scene22 = AssetDatabase.LoadAssetAtPath<SceneAsset>(S22)
            ? EditorSceneManager.OpenScene(S22, OpenSceneMode.Single)
            : EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EnsurePavilion(scene22);

        if (FindByName(scene22, "S22_Generated") == null)
            AddS22Practices(scene22);
        EditorSceneManager.SaveScene(scene22, S22);
        AssetDatabase.Refresh();

        bool createdS23 = !AssetDatabase.LoadAssetAtPath<SceneAsset>(S23);
        if (createdS23)
        {
            if (!AssetDatabase.CopyAsset(S22, S23))
                throw new InvalidOperationException("S22를 S23으로 복사하지 못했습니다.");
            AssetDatabase.Refresh();
        }
        Scene scene23 = EditorSceneManager.OpenScene(S23, OpenSceneMode.Single);
        GameObject earlierS23 = FindByName(scene23, "S23_Generated");
        GameObject earlierLeaf = FindByName(scene23, "Leaf_Bilinear_0");
        bool needsTeacherPatchMigration = earlierLeaf == null ||
            earlierLeaf.GetComponent<S23_BilinearPatch_Finish>() == null;
        if (earlierS23 == null || FindByName(scene23, "Vine_SplineExtrude") == null ||
            needsTeacherPatchMigration)
        {
            // Earlier draft package may have generated a Bezier roof.
            // Preserve it as an inactive backup and build the actual S23 homework.
            if (earlierS23 != null)
            {
                earlierS23.name = "S23_Generated_PreviousDraft_Backup";
                earlierS23.SetActive(false);
            }
            AddS23Regular(scene23);
        }
        EditorSceneManager.SaveScene(scene23, S23);

        bool createdAbs = !AssetDatabase.LoadAssetAtPath<SceneAsset>(S23Abs);
        if (createdAbs)
        {
            if (!AssetDatabase.CopyAsset(S22, S23Abs))
                throw new InvalidOperationException("S22를 S23_Abs로 복사하지 못했습니다.");
            AssetDatabase.Refresh();
        }
        Scene sceneAbs = EditorSceneManager.OpenScene(S23Abs, OpenSceneMode.Single);
        GameObject earlierAbs = FindByName(sceneAbs, "S23_Abs_Generated");
        GameObject earlierBanner = FindByName(sceneAbs, "S23_Abs_Banner_Segments_1");
        if (earlierAbs == null || earlierBanner == null ||
            earlierBanner.GetComponent<S23_BilinearPatch_Finish>() == null)
        {
            if (earlierAbs != null)
            {
                earlierAbs.name = "S23_Abs_Generated_PreviousDraft_Backup";
                earlierAbs.SetActive(false);
            }
            AddS23Abs(sceneAbs);
        }
        EditorSceneManager.SaveScene(sceneAbs, S23Abs);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Scene reviewScene = EditorSceneManager.OpenScene(S23, OpenSceneMode.Single);
        GameObject main = FindByName(reviewScene, "S23_Generated");
        if (main != null) Selection.activeGameObject = main;

        EditorUtility.DisplayDialog("S22/S23 생성 완료!",
            "Assets/S22/S22_Landscape.unity\n" +
            "Assets/S23/S23_Landscape.unity\n\n" +
            "Assets/S23_Abs/S23_Abs_Landscape.unity\n\n" +
            "S22: Splines, 움직이는 큐브와 카메라, 직선 t, 베지어 곡선\n" +
            "S23: Spline Extrude 덩굴/텍스처 잎/네 점 Bilinear 캐노피/2-Pass 셰이더\n" +
            "S23_Abs: 서로 다른 굵기 2개, Segments 1과 10 패치 2개\n\n" +
            "Play와 Scene 뷰를 확인하고 스크린샷을 찍어 줘!", "완료");
    }

    [MenuItem("Tools/Chaewon S22-S23/2. Rebuild Selected Bilinear Mesh")]
    public static void RebuildSelectedPatch()
    {
        S23_BilinearPatch_Finish surface = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<S23_BilinearPatch_Finish>() : null;
        if (surface == null)
        {
            EditorUtility.DisplayDialog("S23", "Hierarchy에서 Bilinear Patch 오브젝트를 먼저 선택해 줘.", "확인");
            return;
        }
        // The professor's S23_BilinearPatch_Finish rebuilds its mesh in Update.
        // Re-enabling it refreshes the temporary mesh without touching a saved asset.
        surface.enabled = false;
        surface.enabled = true;
        EditorApplication.QueuePlayerLoopUpdate();
        SceneView.RepaintAll();
        EditorSceneManager.MarkSceneDirty(surface.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        EditorUtility.DisplayDialog("완료", "교수님 BilinearPatch 코드에 따라 메시를 다시 그리도록 요청했어!", "OK");
    }

    private static string FindBestStartingScene()
    {
        string[] candidates = {
            "Assets/S22/S22_Spline_Start.unity",
            "Assets/S21/S21_Landscape.unity",
            "Assets/S20/S20_Landscape.unity",
            "Assets/S17/S17_Landscape.unity",
            "Assets/S16/S16_Landscape.unity",
            "Assets/S15/S15_Landscape.unity",
            "Assets/ThirdParty/Pavilion/Scenes/Pavilion.unity"
        };
        foreach (string path in candidates)
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                return path;
        return null;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void EnsurePavilion(Scene scene)
    {
        if (FindByName(scene, "Pavilion_Roof") != null)
            return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/ThirdParty/Pavilion/Prefabs/Pavilion.prefab");
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.transform.position = Vector3.zero;
        }
        else
            Debug.LogWarning("S22/S23: 파빌리온 프리팹을 찾지 못했어요. S23 지붕은 예제 위치에 만들어집니다.");
    }

    private static GameObject FindByName(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == objectName) return t.gameObject;
        return null;
    }

    private static Bounds PavilionBounds(Scene scene)
    {
        GameObject roof = FindByName(scene, "Pavilion_Roof");
        if (roof != null)
        {
            GameObject parent = roof.transform.parent != null
                ? roof.transform.parent.gameObject : roof;
            Renderer[] renderers = parent.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length > 0)
            {
                Bounds b = renderers[0].bounds;
                foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
                return b;
            }
        }
        return new Bounds(new Vector3(0f, 2.5f, 0f), new Vector3(8f, 5f, 8f));
    }

    private static SplineContainer MakeSpline(Transform parent, string name,
        Vector3 worldCenter, float radius, float initialY, int knotCount)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(worldCenter.x, initialY, worldCenter.z);
        SplineContainer container = go.AddComponent<SplineContainer>();
        Spline spline = new Spline();
        for (int i = 0; i < knotCount; i++)
        {
            float angle = 2f * Mathf.PI * i / knotCount;
            float rise = (i % 2 == 0 ? 0.15f : 0.85f);
            Vector3 p = new Vector3(Mathf.Cos(angle) * radius,
                                    rise, Mathf.Sin(angle) * radius);
            spline.Add(new BezierKnot(new float3(p.x, p.y, p.z)), TangentMode.AutoSmooth);
        }
        spline.Closed = true;
        container.Spline = spline;
        return container;
    }

    private static void AttachAnimation(GameObject go, SplineContainer spline,
        float seconds, SplineAnimate.AlignmentMode alignment)
    {
        SplineAnimate anim = go.AddComponent<SplineAnimate>();
        anim.Container = spline;
        anim.AnimationMethod = SplineAnimate.Method.Time;
        anim.Duration = seconds;
        anim.Loop = SplineAnimate.LoopMode.Loop;
        anim.Alignment = alignment;
        anim.PlayOnAwake = true;
    }

    private static Material GetMaterial(string path, Color color)
    {
        Material found = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (found != null) return found;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material material = new Material(shader);
        material.name = System.IO.Path.GetFileNameWithoutExtension(path);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject Primitive(PrimitiveType type, string name,
        Transform parent, Vector3 pos, float size, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * size;
        if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
        return go;
    }

    private static void SetLine(GameObject go, Material material, float width)
    {
        LineRenderer line = go.GetComponent<LineRenderer>();
        if (line == null) line = go.AddComponent<LineRenderer>();
        line.sharedMaterial = material;
        line.startWidth = width;
        line.endWidth = width;
        line.numCornerVertices = 4;
        line.numCapVertices = 4;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private static void AddS22Practices(Scene scene)
    {
        Bounds area = PavilionBounds(scene);
        Vector3 center = area.center;
        float radius = Mathf.Clamp(Mathf.Max(area.extents.x, area.extents.z) + 3f, 5f, 13f);
        float baseHeight = area.min.y;
        Transform root = new GameObject("S22_Generated").transform;
        Material purple = GetMaterial("Assets/S22/S22_Mover.mat", new Color(.75f, .38f, 1f));
        Material yellow = GetMaterial("Assets/S22/S22_Line.mat", new Color(1f, .82f, .18f));
        Material cyan = GetMaterial("Assets/S22/S22_Curve.mat", new Color(.12f, .9f, .95f));

        // Practice 1: two spline containers, one animated object and one animated camera.
        SplineContainer path = MakeSpline(root, "Spline_Mover", center,
            radius, baseHeight + 1.4f, 6);
        SplineContainer shotPath = MakeSpline(root, "Spline_Shot", center,
            radius + 2.8f, baseHeight + 3.7f, 6);

        GameObject mover = Primitive(PrimitiveType.Cube, "Mover", root,
            center + Vector3.up * (baseHeight + 1.4f - center.y), .75f, purple);
        AttachAnimation(mover, path, 12f, SplineAnimate.AlignmentMode.SplineElement);

        GameObject shot = new GameObject("ShotCamera");
        shot.transform.SetParent(root);
        Camera cam = shot.AddComponent<Camera>();
        cam.depth = 100f;
        cam.fieldOfView = 54f;
        AttachAnimation(shot, shotPath, 20f, SplineAnimate.AlignmentMode.None);
        shot.AddComponent<S22_LookAtTarget>().target = mover.transform;

        // Practice 2: straight-line interpolation, t = 0.25.
        Transform exercises = new GameObject("S22_Interpolation_And_Bezier").transform;
        exercises.SetParent(root);
        Vector3 offset = new Vector3(center.x + radius + 5f, baseHeight + 2f, center.z);
        GameObject a = Primitive(PrimitiveType.Sphere, "A", exercises,
            offset + new Vector3(-2f, 0f, 0f), .38f, purple);
        GameObject b = Primitive(PrimitiveType.Sphere, "B", exercises,
            offset + new Vector3(2f, 0f, 0f), .38f, purple);
        GameObject m = Primitive(PrimitiveType.Sphere, "Marker", exercises,
            offset, .23f, yellow);
        GameObject lineGo = new GameObject("Line");
        lineGo.transform.SetParent(exercises);
        SetLine(lineGo, yellow, .07f);
        S22_Line solved = lineGo.AddComponent<S22_Line>();
        solved.pointA = a.transform;
        solved.pointB = b.transform;
        solved.marker = m.transform;
        solved.t = .25f;
        solved.Refresh();

        // Practice 3: both segment counts visible; easy to capture each in the Scene view.
        CreateCurve(exercises, offset + new Vector3(0f, .5f, 5f), 4, "Curve_Segments4", yellow, purple);
        CreateCurve(exercises, offset + new Vector3(0f, .5f, 10f), 32, "Curve_Segments32", cyan, purple);
    }

    private static void CreateCurve(Transform parent, Vector3 center, int segments,
        string name, Material lineMaterial, Material pointMaterial)
    {
        Transform group = new GameObject(name + "_ControlPoints").transform;
        group.SetParent(parent);
        Vector3[] coords = {
            new Vector3(-2.2f, 0f, 0f),
            new Vector3(-1.3f, 2.8f, 0f),
            new Vector3(1.3f, -1.8f, 0f),
            new Vector3(2.2f, 0f, 0f)
        };
        Transform[] points = new Transform[4];
        string[] names = { "A", "G1", "G2", "B" };
        for (int i = 0; i < 4; i++)
            points[i] = Primitive(PrimitiveType.Cube, names[i], group,
                center + coords[i], .28f, pointMaterial).transform;
        GameObject marker = Primitive(PrimitiveType.Sphere, "Bezier_Marker", group,
            center, .24f, lineMaterial);
        GameObject curveGo = new GameObject(name);
        curveGo.transform.SetParent(group);
        SetLine(curveGo, lineMaterial, .10f);
        S22_Curve_Finish curve = curveGo.AddComponent<S22_Curve_Finish>();
        curve.points = points;
        curve.marker = marker.transform;
        curve.t = .25f;
        curve.segments = segments;
        curve.Refresh();
    }

    private static S23_BilinearPatch_Finish MakePatch(Transform parent, string name,
        Vector3[] worldCorners, int uSegments, int vSegments, Material material,
        string assetFolder, bool bothSides)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = (worldCorners[0] + worldCorners[1] + worldCorners[2] + worldCorners[3]) * .25f;
        go.AddComponent<MeshRenderer>();
        S23_BilinearPatch_Finish patch = go.AddComponent<S23_BilinearPatch_Finish>();
        Transform[] control = new Transform[4];
        string[] labels = { "Corner00", "Corner10", "Corner01", "Corner11" };
        for (int i = 0; i < 4; i++)
        {
            GameObject corner = new GameObject(labels[i]);
            corner.transform.SetParent(go.transform);
            corner.transform.position = worldCorners[i];
            control[i] = corner.transform;
        }
        patch.corner00 = control[0];
        patch.corner10 = control[1];
        patch.corner01 = control[2];
        patch.corner11 = control[3];
        patch.uSegments = uSegments;
        patch.vSegments = vSegments;
        // The original S23_BilinearPatch_Finish.cs provided by the professor
        // creates and updates its own Mesh in OnEnable/Update. Do not override it.
        // A double-sided URP/Lit material lets the original one-sided mesh
        // render from both above and below without modifying teacher code.
        if (bothSides && material != null && material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", 0f);
            EditorUtility.SetDirty(material);
        }
        go.GetComponent<MeshRenderer>().sharedMaterial = material;
        EditorApplication.QueuePlayerLoopUpdate();
        return patch;
    }

    private static SplineExtrude MakeVine(Transform root, string name,
        Vector3 anchor, float radius, Material stemMaterial)
    {
        GameObject vine = new GameObject(name);
        vine.transform.SetParent(root);
        vine.transform.position = anchor;
        SplineContainer container = vine.AddComponent<SplineContainer>();
        Spline spline = new Spline();
        Vector3[] knots = {
            new Vector3(0f, 0f, 0f),
            new Vector3(.65f, .7f, .22f),
            new Vector3(.1f, 1.55f, .3f),
            new Vector3(-.45f, 2.3f, -.1f),
            new Vector3(.15f, 3.15f, -.3f),
            new Vector3(.45f, 3.9f, .17f)
        };
        foreach (Vector3 p in knots)
            spline.Add(new BezierKnot(new float3(p.x, p.y, p.z)), TangentMode.AutoSmooth);
        spline.Closed = false;
        container.Spline = spline;
        SplineExtrude extrude = vine.AddComponent<SplineExtrude>();
        extrude.Container = container;
        extrude.Radius = radius;
        extrude.Sides = 12;
        extrude.Capped = true;
        extrude.RebuildOnSplineChange = true;
        vine.GetComponent<MeshRenderer>().sharedMaterial = stemMaterial;
        EditorUtility.SetDirty(extrude);
        return extrude;
    }

    private static Vector3[] LeafCorners(Vector3 center, float width, float length, float twist)
    {
        return new[] {
            center + new Vector3(-width * .45f, -length * .5f, -twist),
            center + new Vector3(width * .45f, -length * .5f, twist),
            center + new Vector3(-width * .58f, length * .5f, twist),
            center + new Vector3(width * .58f, length * .5f, -twist)
        };
    }

    private static Material MakeLeafMaterial()
    {
        string dir = "Assets/S23";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/Leaf_BaseColor.png") == null)
            MakeLeafTextures(dir);

        Material leaf = GetMaterial(dir + "/Mat_S23_Leaf_PBR.mat", Color.white);
        Texture2D baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/Leaf_BaseColor.png");
        Texture2D bump = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/Leaf_Normal.png");
        Texture2D ao = AssetDatabase.LoadAssetAtPath<Texture2D>(dir + "/Leaf_Occlusion.png");
        leaf.SetTexture("_BaseMap", baseMap);
        leaf.SetTexture("_BumpMap", bump);
        leaf.SetTexture("_OcclusionMap", ao);
        leaf.SetFloat("_Surface", 0f);
        leaf.SetFloat("_AlphaClip", 1f);
        leaf.SetFloat("_Cutoff", .35f);
        leaf.SetFloat("_Cull", 0f); // Render Face: Both
        leaf.SetFloat("_BumpScale", .7f);
        leaf.SetFloat("_OcclusionStrength", 1f);
        leaf.EnableKeyword("_ALPHATEST_ON");
        leaf.EnableKeyword("_NORMALMAP");
        leaf.EnableKeyword("_OCCLUSIONMAP");
        EditorUtility.SetDirty(leaf);
        return leaf;
    }

    private static void MakeLeafTextures(string assetDir)
    {
        const int size = 256;
        Texture2D baseTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Texture2D normalTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Texture2D aoTex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            float v = (y + .5f) / size;
            float width = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Mathf.PI * v)), .7f) * (.74f + .26f * v);
            for (int x = 0; x < size; x++)
            {
                float u = 2f * (x + .5f) / size - 1f;
                float n = Mathf.PerlinNoise(u * 7f + 14f, v * 13f + 8f);
                float vein = Mathf.Pow(Mathf.Abs(Mathf.Sin(v * 12f * Mathf.PI + Mathf.Abs(u) * 5f)), 10f);
                float mainVein = Mathf.Exp(-Mathf.Abs(u) * 26f);
                float radius = width > 1e-4f ? Mathf.Abs(u) / width : 100f;
                float alpha = Mathf.Clamp01((1f - radius) * 28f);
                float green = .46f + .20f * n - .10f * vein + .15f * mainVein;
                baseTex.SetPixel(x, y, new Color(.12f + .08f * n + .16f * vein,
                    green, .08f + .07f * n, alpha));

                float nx = Mathf.Clamp(-u * .9f / Mathf.Max(.15f, width), -1f, 1f);
                float ny = -.15f * Mathf.Cos(v * Mathf.PI * 2f);
                Vector3 nrm = new Vector3(nx, ny, 1f).normalized;
                normalTex.SetPixel(x, y, new Color(nrm.x * .5f + .5f,
                    nrm.y * .5f + .5f, nrm.z * .5f + .5f, 1f));
                float occlusion = Mathf.Clamp01(.65f + .32f * (1f - radius) - .10f * vein);
                aoTex.SetPixel(x, y, new Color(occlusion, occlusion, occlusion, 1f));
            }
        }
        WriteTexture(baseTex, assetDir + "/Leaf_BaseColor.png", false, true);
        WriteTexture(normalTex, assetDir + "/Leaf_Normal.png", true, false);
        WriteTexture(aoTex, assetDir + "/Leaf_Occlusion.png", false, false);
        UnityEngine.Object.DestroyImmediate(baseTex);
        UnityEngine.Object.DestroyImmediate(normalTex);
        UnityEngine.Object.DestroyImmediate(aoTex);
    }

    private static void WriteTexture(Texture2D tex, string assetPath,
        bool normalMap, bool alpha)
    {
        tex.Apply(false, false);
        System.IO.File.WriteAllBytes(assetPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = normalMap ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !normalMap;
            importer.alphaIsTransparency = alpha;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private static void MakeLeafCluster(Transform root, Vector3 anchor, Material leafMaterial)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = anchor + new Vector3(
                .25f + Mathf.Sin(i * 1.8f) * .65f,
                .52f + i * .60f,
                .19f + Mathf.Cos(i * 1.9f) * .28f);
            MakePatch(root, "Leaf_Bilinear_" + i, LeafCorners(pos, .78f, 1.12f, .12f),
                10, 10, leafMaterial, "Assets/S23", true);
        }
    }

    private static Material ConfigureShaderMaterial(string path, string shaderName, Color tint)
    {
        Material m = GetMaterial(path, tint);
        Shader s = Shader.Find(shaderName);
        if (s != null) m.shader = s;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", tint);
        EditorUtility.SetDirty(m);
        return m;
    }

    private static void MakeShaderComparison(Transform root, Vector3 pos)
    {
        Transform group = new GameObject("S23_ThreeMaterials_vs_OneMaterial").transform;
        group.SetParent(root);
        Material toon = ConfigureShaderMaterial("Assets/S23/Mat_S20_Toon_Comparison.mat",
            "CG/S20_Toon_Finish", new Color(.8f, .95f, 1f));
        Material outline = ConfigureShaderMaterial("Assets/S23/Mat_S20_Outline_Comparison.mat",
            "CG/S20_Outline_Finish", Color.black);
        Material vertex = ConfigureShaderMaterial("Assets/S23/Mat_S20_VertexColor_Comparison.mat",
            "CG/S20_VertexColor_Finish", Color.white);
        if (toon.shader.name != "CG/S20_Toon_Finish") toon.shader = Shader.Find("CG/S23_DemoToon_Only");
        if (outline.shader.name != "CG/S20_Outline_Finish") outline.shader = Shader.Find("CG/S23_DemoOutline_Only");
        if (vertex.shader.name != "CG/S20_VertexColor_Finish") vertex.shader = Shader.Find("CG/S23_DemoVertexColor_Only");

        Material combined = ConfigureShaderMaterial("Assets/S23/Mat_S23_TwoPassCombined.mat",
            "CG/S23_ToonOutlineVertexColor_TwoPass", new Color(.85f, .97f, 1f));
        GameObject stacked = Primitive(PrimitiveType.Sphere, "Three_Materials_Stacked_Unexpected", group,
            pos + new Vector3(-2.0f, 0f, 0f), 2.5f, null);
        GameObject single = Primitive(PrimitiveType.Sphere, "One_Material_TwoPass_Combined", group,
            pos + new Vector3(2.0f, 0f, 0f), 2.5f, null);
        Mesh sphere = UnityEngine.Object.Instantiate(stacked.GetComponent<MeshFilter>().sharedMesh);
        sphere.name = "S23_VertexColors_DemoSphere";
        Vector3[] vertices = sphere.vertices;
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.Lerp(new Color(.24f, .52f, 1f),
                new Color(1f, .73f, .42f), Mathf.InverseLerp(-.5f, .5f, vertices[i].y));
        sphere.colors = colors;
        string path = "Assets/S23/S23_DemoColorSphere_Mesh.asset";
        Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (saved == null) { AssetDatabase.CreateAsset(sphere, path); saved = sphere; }
        else UnityEngine.Object.DestroyImmediate(sphere);
        stacked.GetComponent<MeshFilter>().sharedMesh = saved;
        single.GetComponent<MeshFilter>().sharedMesh = saved;
        stacked.GetComponent<MeshRenderer>().sharedMaterials = new[] { toon, outline, vertex };
        single.GetComponent<MeshRenderer>().sharedMaterials = new[] { combined };
        EditorUtility.SetDirty(toon); EditorUtility.SetDirty(outline); EditorUtility.SetDirty(vertex);
    }

    private static void AddS23Regular(Scene scene)
    {
        Transform root = new GameObject("S23_Generated").transform;
        Bounds bounds = PavilionBounds(scene);
        GameObject roofOld = FindByName(scene, "Pavilion_Roof");
        Bounds roofBounds = roofOld != null && roofOld.GetComponent<Renderer>() != null
            ? roofOld.GetComponent<Renderer>().bounds
            : new Bounds(bounds.center + Vector3.up * 1.5f, new Vector3(9f, 2f, 9f));
        float halfX = Mathf.Max(2.5f, roofBounds.extents.x * 1.06f);
        float halfZ = Mathf.Max(2.5f, roofBounds.extents.z * 1.06f);
        float y = roofBounds.center.y;
        Vector3 c = roofBounds.center;

        // Four corners: a saddle-like canopy, visible from both above and below.
        Vector3[] canopy = {
            new Vector3(c.x - halfX, y - .6f, c.z - halfZ),
            new Vector3(c.x + halfX, y + 1.4f, c.z - halfZ),
            new Vector3(c.x - halfX, y + 1.4f, c.z + halfZ),
            new Vector3(c.x + halfX, y - .6f, c.z + halfZ)
        };
        Material canopyMat = GetMaterial("Assets/S23/Mat_S23_Canopy_BothFaces.mat", new Color(.26f, .52f, .64f));
        if (canopyMat.HasProperty("_Cull")) canopyMat.SetFloat("_Cull", 0f);

        Transform canopyParent = roofOld != null ? roofOld.transform.parent : root;
        S23_BilinearPatch_Finish patch = MakePatch(canopyParent, "Pavilion_Roof",
            canopy, 20, 20, canopyMat, "Assets/S23", true);
        if (roofOld != null)
        {
            roofOld.name = "Pavilion_Roof_Original_Disabled";
            roofOld.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(roofOld);
        }

        Material stem = GetMaterial("Assets/S23/Mat_S23_VineStem.mat", new Color(.2f, .32f, .16f));
        Material leaf = MakeLeafMaterial();
        Vector3 vineAnchor = new Vector3(bounds.center.x + bounds.extents.x - 1f,
            bounds.min.y + .2f, bounds.center.z + bounds.extents.z - 1f);
        MakeVine(root, "Vine_SplineExtrude", vineAnchor, .095f, stem);
        MakeLeafCluster(root, vineAnchor, leaf);
        MakeShaderComparison(root, bounds.center + new Vector3(halfX + 6f, 1.5f, halfZ + 3f));
        MakeOverviewCamera(root, "S23_OverviewCamera", bounds.center,
            Mathf.Max(halfX, halfZ) + 13f, bounds.center + Vector3.up * 1f);
    }

    private static void MakeOverviewCamera(Transform root, string name,
        Vector3 center, float distance, Vector3 lookAt)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(root);
        go.transform.position = center + new Vector3(distance * .76f, distance * .62f, -distance);
        go.transform.LookAt(lookAt);
        Camera cam = go.AddComponent<Camera>();
        cam.depth = 200f;
        cam.fieldOfView = 58f;
        cam.clearFlags = CameraClearFlags.Skybox;
    }

    private static void AddS23Abs(Scene scene)
    {
        Transform root = new GameObject("S23_Abs_Generated").transform;
        Bounds bounds = PavilionBounds(scene);
        Vector3 origin = bounds.center + new Vector3(bounds.extents.x + 8f, 0f, 0f);
        origin.y = bounds.min.y + .3f;
        Material thin = GetMaterial("Assets/S23_Abs/Mat_Vine_Thin.mat", new Color(.18f, .55f, .32f));
        Material thick = GetMaterial("Assets/S23_Abs/Mat_Vine_Thick.mat", new Color(.6f, .36f, .18f));
        MakeVine(root, "S23_Abs_Thin_Radius_0_06", origin + new Vector3(-2.5f, 0f, 0f), .06f, thin);
        MakeVine(root, "S23_Abs_Thick_Radius_0_26", origin + new Vector3(2.5f, 0f, 0f), .26f, thick);

        // Not a leaf: two warped quadrilateral banners, 1x1 versus 10x10.
        Material banner = GetMaterial("Assets/S23_Abs/Mat_Bilinear_Banner.mat", new Color(.82f, .34f, .7f));
        float y = bounds.min.y + 2.8f;
        for (int i = 0; i < 2; i++)
        {
            Vector3 x = origin + new Vector3(i == 0 ? -3.2f : 3.2f, 0f, 7f);
            Vector3[] corners = {
                new Vector3(x.x - 1.7f, y + .6f, x.z - 1.2f),
                new Vector3(x.x + 1.7f, y - .4f, x.z - 1.2f),
                new Vector3(x.x - 1.7f, y - .4f, x.z + 1.2f),
                new Vector3(x.x + 1.7f, y + 1.5f, x.z + 1.2f)
            };
            int seg = i == 0 ? 1 : 10;
            MakePatch(root, "S23_Abs_Banner_Segments_" + seg, corners, seg, seg,
                banner, "Assets/S23_Abs", true);
        }

        MakeOverviewCamera(root, "S23_Abs_OverviewCamera",
            origin + new Vector3(0f, 2f, 3f), 12f, origin + new Vector3(0f, 2f, 3f));
    }
}
