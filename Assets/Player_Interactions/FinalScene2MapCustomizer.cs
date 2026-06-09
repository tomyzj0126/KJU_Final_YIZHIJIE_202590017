using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScene2MapCustomizer : MonoBehaviour
{
    private const string TargetSceneName = "FINAL_Scene2";
    private const float PlacementTolerance = 0.25f;
    private const string GeneratedPrefix = "FINAL_Scene2_NewLayout_";

    private static bool appliedForCurrentLoad;
    private static Material ceilingSurfaceMaterial;
    private static Material wallSurfaceMaterial;
    private static Material floorSurfaceMaterial;
    private static Material coverMaterial;
    private static Material routeMaterial;
    private static Material finalRouteMaterial;

    private struct Placement
    {
        public readonly string AreaName;
        public readonly string ObjectName;
        public readonly Vector3 From;
        public readonly Vector3 To;

        public Placement(string areaName, string objectName, Vector3 from, Vector3 to)
        {
            AreaName = areaName;
            ObjectName = objectName;
            From = from;
            To = to;
        }
    }

    private static readonly Placement[] LayoutPlacements =
    {
        new Placement("Area2", "Box", new Vector3(2.28f, 1f, 7.51f), new Vector3(3.8f, 1f, 8.7f)),
        new Placement("Area2", "Box (1)", new Vector3(4.67f, 1f, 12.58f), new Vector3(5.9f, 1f, 11.1f)),
        new Placement("Area2", "Box (2)", new Vector3(-4.76f, 1f, 7.77f), new Vector3(-5.8f, 1f, 9.6f)),
        new Placement("Area2", "Box (3)", new Vector3(4.67f, 2.44f, 12.06f), new Vector3(5.9f, 2.44f, 11.1f)),
        new Placement("Area2", "Character_Enemy", new Vector3(6.58f, 1f, 10f), new Vector3(6.7f, 1f, 12.4f)),
        new Placement("Area2", "Character_Enemy (1)", new Vector3(-6.86f, 1f, 8.8f), new Vector3(-7.1f, 1f, 11.6f)),
        new Placement("Area2", "Character_Enemy (2)", new Vector3(-2.13f, 1f, 12.54f), new Vector3(-3.7f, 1f, 13.2f)),

        new Placement("Area3", "Box", new Vector3(-0.37f, 1f, 9.71f), new Vector3(1.9f, 1f, 8.9f)),
        new Placement("Area3", "Box (1)", new Vector3(1.6f, 1f, 12.58f), new Vector3(3.6f, 1f, 12.1f)),
        new Placement("Area3", "Box (2)", new Vector3(-5.85f, 1f, 7.73f), new Vector3(-7.1f, 1f, 9.2f)),
        new Placement("Area3", "Box (3)", new Vector3(-5.85f, 2.57f, 7.73f), new Vector3(-7.1f, 2.57f, 9.2f)),
        new Placement("Area3", "Character_Enemy", new Vector3(-5.44f, 1f, 11.46f), new Vector3(-7.7f, 1f, 12.4f)),
        new Placement("Area3", "Character_Enemy (1)", new Vector3(-8.08f, 1f, 8.55f), new Vector3(-8.4f, 1f, 10.5f)),
        new Placement("Area3", "Character_Enemy (2)", new Vector3(-2.27f, 1f, 7.28f), new Vector3(-4.3f, 1f, 7.4f)),
        new Placement("Area3", "Character_Enemy (3)", new Vector3(-0.59f, 1f, 12.15f), new Vector3(1.1f, 1f, 13.3f)),

        new Placement("Area4", "Box (7)", new Vector3(-15.24f, 5.8f, 0.33f), new Vector3(-13.9f, 5.8f, -1f)),
        new Placement("Area4", "Box (6)", new Vector3(-17.43f, 5.8f, -10.72f), new Vector3(-16.3f, 5.8f, -12.6f)),
        new Placement("Area4", "Box (9)", new Vector3(-17.43f, 7.319f, -10.72f), new Vector3(-16.3f, 7.319f, -12.6f)),
        new Placement("Area4", "Box (5)", new Vector3(-19.39f, 5.8f, -4.98f), new Vector3(-18.4f, 5.8f, -2.4f)),
        new Placement("Area4", "Box (3)", new Vector3(-25.07f, 5.8f, -1.0500011f), new Vector3(-24.7f, 5.8f, 1.9f)),
        new Placement("Area4", "Box (8)", new Vector3(-25.07f, 5.8f, 0.72f), new Vector3(-28f, 5.8f, -2f)),
        new Placement("Area4", "Box (4)", new Vector3(-29.05f, 5.8f, -8.43f), new Vector3(-29f, 5.8f, -12.2f)),
        new Placement("Area4", "Character_Enemy (3)", new Vector3(-27.03f, 5.848311f, -10.93f), new Vector3(-27.7f, 5.848311f, -13.2f)),
        new Placement("Area4", "Character_Enemy (4)", new Vector3(-27.35f, 5.848311f, -2.9121122f), new Vector3(-29.1f, 5.848311f, -3.1f)),
        new Placement("Area4", "Character_Enemy (5)", new Vector3(-19.25f, 5.848311f, -13.26f), new Vector3(-16.4f, 5.848311f, -13.9f)),
        new Placement("Area4", "Character_Enemy (6)", new Vector3(-20.79f, 5.848311f, -5.01f), new Vector3(-21.8f, 5.848311f, -2.6f)),
        new Placement("Area4", "Character_Enemy (7)", new Vector3(-15.07f, 5.848311f, -2.13f), new Vector3(-13.8f, 5.848311f, -3.6f)),

        new Placement("Area5", "Box (7)", new Vector3(-16.84f, 5.8f, 0.33f), new Vector3(-15.1f, 5.8f, -1.3f)),
        new Placement("Area5", "Box (11)", new Vector3(-15.18f, 5.8f, 0.33f), new Vector3(-18f, 5.8f, 2f)),
        new Placement("Area5", "Box (10)", new Vector3(-16f, 7.34f, 0.33f), new Vector3(-18f, 7.34f, 2f)),
        new Placement("Area5", "Box (5)", new Vector3(-22.17f, 5.8f, -2.13f), new Vector3(-22.8f, 5.8f, 1.8f)),
        new Placement("Area5", "Box (9)", new Vector3(-19.93f, 5.8f, -6.89f), new Vector3(-19f, 5.8f, -9.4f)),
        new Placement("Area5", "Box (3)", new Vector3(-25.07f, 5.8f, 1.17f), new Vector3(-25.9f, 5.8f, 3f)),
        new Placement("Area5", "Box (8)", new Vector3(-28.39f, 5.8f, -9.65f), new Vector3(-27.6f, 5.8f, -12.4f)),
        new Placement("Area5", "Box (12)", new Vector3(-28.39f, 7.334f, -10.09f), new Vector3(-27.6f, 7.334f, -12.4f)),
        new Placement("Area5", "Box (4)", new Vector3(-24.63f, 5.8f, -13.360001f), new Vector3(-22.4f, 5.8f, -13.5f)),
        new Placement("Area5", "Box (6)", new Vector3(-15.1f, 5.8f, -11.83f), new Vector3(-15.2f, 5.8f, -13.4f)),
        new Placement("Area5", "Character_Enemy (4)", new Vector3(-27.35f, 5.85f, 3.18f), new Vector3(-28.5f, 5.85f, 1.1f)),
        new Placement("Area5", "Character_Enemy (5)", new Vector3(-17.96f, 5.848311f, 6.13f), new Vector3(-16.2f, 5.848311f, 3.6f)),
        new Placement("Area5", "Character_Enemy (6)", new Vector3(-25.25f, 5.848311f, -3.31f), new Vector3(-26.9f, 5.848311f, -1.5f)),
        new Placement("Area5", "Character_Enemy (7)", new Vector3(-15.07f, 5.848311f, -13.86f), new Vector3(-17.5f, 5.848311f, -13.9f)),
        new Placement("Area5", "Character_Enemy (8)", new Vector3(-15.07f, 5.848311f, 2.29f), new Vector3(-14.1f, 5.848311f, 0.5f)),
        new Placement("Area5", "Character_Enemy (9)", new Vector3(-21.95f, 5.848311f, -9.5f), new Vector3(-23.6f, 5.848311f, -11.6f)),

        new Placement("AreaSafeZone", "Ammo_Box", new Vector3(-12.855f, 2.114106f, -14.356f), new Vector3(-15.7f, 2.114106f, -12.5f)),
        new Placement("AreaSafeZone", "Ammo_Box (1)", new Vector3(-11.34f, 2.114106f, -14.356f), new Vector3(-14.2f, 2.114106f, -3.6f)),
        new Placement("AreaSafeZone", "Ammo_Box (2)", new Vector3(-9.88f, 2.114106f, -14.356f), new Vector3(-11f, 2.114106f, -12.4f)),
        new Placement("AreaSafeZone", "Ammo_Box (3)", new Vector3(-8.5f, 2.114106f, -14.356f), new Vector3(-8.3f, 2.114106f, -3.6f)),
        new Placement("AreaSafeZone", "Ammo_Box (4)", new Vector3(-7.04f, 2.114106f, -14.356f), new Vector3(-5.8f, 2.114106f, -10.8f)),
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        appliedForCurrentLoad = false;
        ApplyForScene(scene);
    }

    private static void ApplyForScene(Scene scene)
    {
        if (appliedForCurrentLoad || !scene.IsValid() || scene.name != TargetSceneName)
        {
            return;
        }

        appliedForCurrentLoad = true;
        GameObject runner = new GameObject("FINAL_Scene2_Map_Customizer");
        runner.AddComponent<FinalScene2MapCustomizer>().ApplyLayout();
    }

    private void ApplyLayout()
    {
        ApplyObjectPlacements();
        ClearImportantRoutes();
        RemoveDeletedGeneratedGeometry();
        AddDistinctMapGeometry();
        AddElevatorWalkPads();
        ApplySurfaceMaterials();
    }

    private static void RemoveDeletedGeneratedGeometry()
    {
        RemoveGeneratedObject("A3_Elevator_Guard_Right");
    }

    private static void ApplySurfaceMaterials()
    {
        MeshRenderer[] renderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer == null || !IsSceneTransform(renderer.transform) || ShouldSkipSurface(renderer.transform))
            {
                continue;
            }

            Material[] materials = renderer.sharedMaterials;
            Material material = GetSurfaceMaterial(renderer.transform, materials);
            if (material == null)
            {
                continue;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static Material GetSurfaceMaterial(Transform transform, Material[] existingMaterials)
    {
        string objectName = transform.name.Trim();
        string lowerName = objectName.ToLowerInvariant();

        if (IsCeilingSurfaceName(lowerName))
        {
            return GetCeilingSurfaceMaterial();
        }

        if (IsWallSurfaceName(lowerName) || UsesWallMaterial(existingMaterials) || HasAncestorStartingWith(transform, "Walls"))
        {
            return GetWallSurfaceMaterial();
        }

        if (IsFloorSurfaceName(lowerName) || UsesFloorMaterial(existingMaterials))
        {
            return GetFloorSurfaceMaterial();
        }

        return null;
    }

    private static bool ShouldSkipSurface(Transform transform)
    {
        string lowerName = transform.name.ToLowerInvariant();
        return lowerName.Contains("text")
            || lowerName.Contains("button")
            || lowerName.Contains("crosshair")
            || lowerName.Contains("route_marker")
            || lowerName.StartsWith("box")
            || lowerName.StartsWith("ammo")
            || lowerName.StartsWith("character")
            || HasAncestor(transform, "Canvas")
            || HasAncestor(transform, "Enemys")
            || HasAncestor(transform, "Boxs");
    }

    private static bool IsCeilingSurfaceName(string lowerName)
    {
        return lowerName.StartsWith("roof") || lowerName.Contains("ceiling");
    }

    private static bool IsWallSurfaceName(string lowerName)
    {
        return lowerName.StartsWith("wall")
            || lowerName.Contains("_wall")
            || lowerName.Contains("wall_")
            || lowerName.Contains("broken_wall")
            || lowerName.Contains("baffle")
            || lowerName.Contains("rail");
    }

    private static bool IsFloorSurfaceName(string lowerName)
    {
        return lowerName.StartsWith("plane")
            || lowerName.Contains("floor")
            || lowerName.Contains("ground");
    }

    private static bool UsesWallMaterial(Material[] materials)
    {
        return HasMaterialName(materials, "mat_wall")
            || HasMaterialName(materials, "mat_wall2")
            || HasMaterialName(materials, "red_brick")
            || HasMaterialName(materials, "brick_wall")
            || HasMaterialName(materials, "dark_brick")
            || HasMaterialName(materials, "grey_plaster");
    }

    private static bool UsesFloorMaterial(Material[] materials)
    {
        return HasMaterialName(materials, "mat_floor")
            || HasMaterialName(materials, "whtplank")
            || HasMaterialName(materials, "wood_floor")
            || HasMaterialName(materials, "wood_table_worn");
    }

    private static bool HasMaterialName(Material[] materials, string expectedNamePart)
    {
        if (materials == null)
        {
            return false;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material != null && material.name.ToLowerInvariant().Contains(expectedNamePart))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddDistinctMapGeometry()
    {
        // Area2: turn the first combat room into a visible S-shaped push.
        CreateBlock("Area2", "A2_Left_Channel_Wall", new Vector3(-3.35f, 1.25f, 8.4f), new Vector3(0.35f, 2.5f, 4.4f), GetBarrierMaterial());
        CreateBlock("Area2", "A2_Right_Channel_Wall", new Vector3(3.35f, 1.25f, 12.1f), new Vector3(0.35f, 2.5f, 4.2f), GetBarrierMaterial());
        CreateBlock("Area2", "A2_Center_Cover_Island", new Vector3(0f, 0.65f, 10.2f), new Vector3(1.4f, 1.3f, 1.4f), GetCoverMaterial());
        CreateFloorMarker("Area2", "A2_Route_Marker_01", new Vector3(-1.7f, 0.045f, 8.0f), new Vector3(2.2f, 0.06f, 1.35f), GetRouteMaterial());
        CreateFloorMarker("Area2", "A2_Route_Marker_02", new Vector3(1.6f, 0.045f, 10.4f), new Vector3(2.3f, 0.06f, 1.35f), GetRouteMaterial());
        CreateFloorMarker("Area2", "A2_Route_Marker_03", new Vector3(-1.55f, 0.045f, 12.7f), new Vector3(2.4f, 0.06f, 1.35f), GetRouteMaterial());
        CreateGuideLight("Area2", "A2_Guide_Light_01", new Vector3(-4.1f, 3.0f, 8.0f), new Color(0.1f, 0.7f, 1f), 4.5f, 1.1f);
        CreateGuideLight("Area2", "A2_Guide_Light_02", new Vector3(4.2f, 3.0f, 12.3f), new Color(0.1f, 0.7f, 1f), 4.5f, 1.1f);

        // Area3: make the elevator approach feel like a new intermediate room.
        CreateBlock("Area3", "A3_Broken_Wall_Left", new Vector3(-4.2f, 1.2f, 8.0f), new Vector3(4.2f, 2.4f, 0.35f), GetBarrierMaterial());
        CreateBlock("Area3", "A3_Broken_Wall_Right", new Vector3(2.8f, 1.2f, 11.9f), new Vector3(0.35f, 2.4f, 3.4f), GetBarrierMaterial());
        CreateBlock("Area3", "A3_Elevator_Guard_Left", new Vector3(5.15f, 0.85f, 10.0f), new Vector3(0.55f, 1.7f, 2.3f), GetCoverMaterial());
        CreateFloorMarker("Area3", "A3_Route_Marker_01", new Vector3(-1.2f, 0.045f, 9.3f), new Vector3(2.6f, 0.06f, 1.25f), GetRouteMaterial());
        CreateFloorMarker("Area3", "A3_Route_Marker_02", new Vector3(1.4f, 0.045f, 12.5f), new Vector3(2.6f, 0.06f, 1.25f), GetRouteMaterial());
        CreateGuideLight("Area3", "A3_Elevator_Guide_Light", new Vector3(5.9f, 2.6f, 12.4f), new Color(1f, 0.45f, 0.12f), 5f, 1.3f);

        // Area4: replace the open upper floor with a central choke and two side lanes.
        CreateBlock("Area4", "A4_Left_Baffle", new Vector3(-25.4f, 6.2f, -6.9f), new Vector3(4.8f, 2.4f, 0.35f), GetBarrierMaterial());
        CreateBlock("Area4", "A4_Right_Baffle", new Vector3(-18.4f, 6.2f, -3.0f), new Vector3(4.4f, 2.4f, 0.35f), GetBarrierMaterial());
        CreateBlock("Area4", "A4_Center_Block", new Vector3(-22.0f, 5.8f, -4.9f), new Vector3(1.5f, 1.6f, 1.5f), GetCoverMaterial());
        CreateBlock("Area4", "A4_Back_Side_Cover", new Vector3(-28.2f, 5.75f, -12.2f), new Vector3(1.6f, 1.5f, 2.2f), GetCoverMaterial());
        CreateFloorMarker("Area4", "A4_Route_Marker_01", new Vector3(-25.0f, 5.08f, -4.8f), new Vector3(2.7f, 0.06f, 1.15f), GetFinalRouteMaterial());
        CreateFloorMarker("Area4", "A4_Route_Marker_02", new Vector3(-20.0f, 5.08f, -6.0f), new Vector3(2.7f, 0.06f, 1.15f), GetFinalRouteMaterial());
        CreateGuideLight("Area4", "A4_Guide_Light_01", new Vector3(-25.5f, 8.0f, -5.2f), new Color(1f, 0.35f, 0.08f), 5.5f, 1.35f);

        // Area5: build a more distinct final arena before the safe-zone/elevator path.
        CreateBlock("Area5", "A5_Final_Left_Rail", new Vector3(-28.1f, 6.1f, -5.6f), new Vector3(0.35f, 2.2f, 4.8f), GetBarrierMaterial());
        CreateBlock("Area5", "A5_Final_Right_Rail", new Vector3(-17.2f, 6.1f, -5.6f), new Vector3(0.35f, 2.2f, 4.8f), GetBarrierMaterial());
        CreateBlock("Area5", "A5_Final_Back_Baffle", new Vector3(-23.0f, 6.15f, -11.2f), new Vector3(6.4f, 2.3f, 0.35f), GetBarrierMaterial());
        CreateBlock("Area5", "A5_Final_Center_Cover", new Vector3(-22.7f, 5.78f, -6.0f), new Vector3(1.8f, 1.55f, 1.8f), GetCoverMaterial());
        CreateBlock("Area5", "A5_Final_Side_Cover", new Vector3(-18.2f, 5.75f, -1.8f), new Vector3(1.4f, 1.5f, 2.2f), GetCoverMaterial());
        CreateFloorMarker("Area5", "A5_Route_Marker_01", new Vector3(-26.0f, 5.08f, -5.1f), new Vector3(2.6f, 0.06f, 1.15f), GetFinalRouteMaterial());
        CreateFloorMarker("Area5", "A5_Route_Marker_02", new Vector3(-21.6f, 5.08f, -7.2f), new Vector3(2.6f, 0.06f, 1.15f), GetFinalRouteMaterial());
        CreateFloorMarker("Area5", "A5_Route_Marker_03", new Vector3(-18.0f, 5.08f, -3.5f), new Vector3(2.6f, 0.06f, 1.15f), GetFinalRouteMaterial());
        CreateGuideLight("Area5", "A5_Final_Guide_Light_01", new Vector3(-27.5f, 8.1f, -4.7f), new Color(1f, 0.2f, 0.08f), 6f, 1.45f);
        CreateGuideLight("Area5", "A5_Final_Guide_Light_02", new Vector3(-17.5f, 8.1f, -4.7f), new Color(1f, 0.2f, 0.08f), 6f, 1.45f);

        // Safe zone: split ammo into two shelves so the ending no longer looks like one straight row.
        CreateBlock("AreaSafeZone", "SafeZone_Ammo_Shelf_Left", new Vector3(-15.1f, 2.02f, -12.7f), new Vector3(2.4f, 0.35f, 1.0f), GetCoverMaterial());
        CreateBlock("AreaSafeZone", "SafeZone_Ammo_Shelf_Right", new Vector3(-8.2f, 2.02f, -3.8f), new Vector3(2.4f, 0.35f, 1.0f), GetCoverMaterial());
        CreateFloorMarker("AreaSafeZone", "SafeZone_Route_Marker_01", new Vector3(-13.0f, 1.66f, -8.0f), new Vector3(2.5f, 0.05f, 1.1f), GetRouteMaterial());
        CreateFloorMarker("AreaSafeZone", "SafeZone_Route_Marker_02", new Vector3(-8.0f, 1.66f, -6.2f), new Vector3(2.5f, 0.05f, 1.1f), GetRouteMaterial());
    }

    private static void ApplyObjectPlacements()
    {
        for (int i = 0; i < LayoutPlacements.Length; i++)
        {
            Placement placement = LayoutPlacements[i];
            Transform target = FindByAreaNameAndLocalPosition(placement.AreaName, placement.ObjectName, placement.From);
            if (target == null)
            {
                target = FindByAreaNameAndLocalPosition(placement.AreaName, placement.ObjectName, placement.To);
            }

            if (target != null)
            {
                target.localPosition = placement.To;
            }
        }
    }

    private static void ClearImportantRoutes()
    {
        ClearRoute("Area2", -1.15f, 1.15f, 7.0f, 13.5f, 2.4f);
        ClearRoute("Area3", -2.4f, 0.8f, 8.4f, 13.6f, 2.5f);
        ClearRoute("Area4", -26f, -18f, -7.4f, -3.4f, 2.8f);
        ClearRoute("Area5", -27.5f, -18f, -7.8f, -4f, 3f);
        ClearRoute("AreaSafeZone", -16f, -5f, -10f, -4f, 3f);
    }

    private static void ClearRoute(string areaName, float minX, float maxX, float minZ, float maxZ, float sideOffset)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        float centerX = (minX + maxX) * 0.5f;

        foreach (Transform t in transforms)
        {
            if (!IsSceneTransform(t) || !HasAncestor(t, areaName) || !IsMoveableObstacle(t.name))
            {
                continue;
            }

            Vector3 p = t.localPosition;
            if (p.x < minX || p.x > maxX || p.z < minZ || p.z > maxZ)
            {
                continue;
            }

            float targetX = p.x < centerX ? minX - sideOffset : maxX + sideOffset;
            t.localPosition = new Vector3(targetX, p.y, p.z);
        }
    }

    private static bool IsMoveableObstacle(string objectName)
    {
        return objectName.StartsWith("Box") || objectName.StartsWith("Ammo_Box");
    }

    private static void AddElevatorWalkPads()
    {
        Transform elevator = FindByAreaNameAndLocalPosition("Area3", "Elevator", new Vector3(7.51f, -5.15f, 12.42f), 0.75f);
        if (elevator == null)
        {
            return;
        }

        Transform floor = FindChildRecursive(elevator, "Floor");
        Transform padParent = floor != null ? floor : elevator;
        Vector3 center = padParent.position;
        Vector3 forward = padParent.forward;
        Quaternion rotation = padParent.rotation;

        CreateWalkPad("Elevator_Entry_Pad_A", center + forward * 2.2f + Vector3.up * 0.08f, rotation, new Vector3(3.8f, 0.14f, 2.8f), padParent);
        CreateWalkPad("Elevator_Entry_Pad_B", center - forward * 2.2f + Vector3.up * 0.08f, rotation, new Vector3(3.8f, 0.14f, 2.8f), padParent);
        CreateWalkPad("Elevator_Floor_Smoother", center + Vector3.up * 0.09f, rotation, new Vector3(4.2f, 0.12f, 3.2f), padParent);
    }

    private static void CreateWalkPad(string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
        {
            return;
        }

        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = name;
        pad.transform.SetPositionAndRotation(position, rotation);
        pad.transform.localScale = scale;
        pad.transform.SetParent(parent, true);

        MeshRenderer renderer = pad.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    private static void CreateBlock(string areaName, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        Transform area = FindSceneTransform(areaName);
        if (area == null)
        {
            return;
        }

        GameObject block = GetOrCreateGeneratedPrimitive(name, PrimitiveType.Cube);
        block.transform.SetParent(area, false);
        block.transform.localPosition = localPosition;
        block.transform.localRotation = Quaternion.identity;
        block.transform.localScale = localScale;

        MeshRenderer renderer = block.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        Collider collider = block.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
            collider.isTrigger = false;
        }
    }

    private static void CreateFloorMarker(string areaName, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        Transform area = FindSceneTransform(areaName);
        if (area == null)
        {
            return;
        }

        GameObject marker = GetOrCreateGeneratedPrimitive(name, PrimitiveType.Cube);
        marker.transform.SetParent(area, false);
        marker.transform.localPosition = localPosition;
        marker.transform.localRotation = Quaternion.identity;
        marker.transform.localScale = localScale;

        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
        }

        Collider collider = marker.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private static void CreateGuideLight(string areaName, string name, Vector3 localPosition, Color color, float range, float intensity)
    {
        Transform area = FindSceneTransform(areaName);
        if (area == null)
        {
            return;
        }

        GameObject lightObject = GameObject.Find(GeneratedPrefix + name);
        if (lightObject == null)
        {
            lightObject = new GameObject(GeneratedPrefix + name);
        }

        lightObject.transform.SetParent(area, false);
        lightObject.transform.localPosition = localPosition;
        lightObject.transform.localRotation = Quaternion.identity;
        lightObject.transform.localScale = Vector3.one;

        Light pointLight = lightObject.GetComponent<Light>();
        if (pointLight == null)
        {
            pointLight = lightObject.AddComponent<Light>();
        }

        pointLight.type = LightType.Point;
        pointLight.color = color;
        pointLight.range = range;
        pointLight.intensity = intensity;
    }

    private static void CreateWorldBlock(string name, Vector3 position, Quaternion rotation, Vector3 scale, Material material, bool hasCollider)
    {
        GameObject block = GetOrCreateGeneratedPrimitive(name, PrimitiveType.Cube);
        block.transform.SetParent(null, true);
        block.transform.SetPositionAndRotation(position, rotation);
        block.transform.localScale = scale;

        MeshRenderer renderer = block.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.enabled = true;
        }

        Collider collider = block.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = hasCollider;
            collider.isTrigger = false;
        }
    }

    private static void CreateWorldGuideLight(string name, Vector3 position, Color color, float range, float intensity)
    {
        GameObject lightObject = GameObject.Find(GeneratedPrefix + name);
        if (lightObject == null)
        {
            lightObject = new GameObject(GeneratedPrefix + name);
        }

        lightObject.transform.SetParent(null, true);
        lightObject.transform.position = position;
        lightObject.transform.rotation = Quaternion.identity;
        lightObject.transform.localScale = Vector3.one;

        Light pointLight = lightObject.GetComponent<Light>();
        if (pointLight == null)
        {
            pointLight = lightObject.AddComponent<Light>();
        }

        pointLight.type = LightType.Point;
        pointLight.color = color;
        pointLight.range = range;
        pointLight.intensity = intensity;
    }

    private static GameObject GetOrCreateGeneratedPrimitive(string name, PrimitiveType primitiveType)
    {
        string objectName = GeneratedPrefix + name;
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            return existing;
        }

        GameObject created = GameObject.CreatePrimitive(primitiveType);
        created.name = objectName;
        return created;
    }

    private static void RemoveGeneratedObject(string name)
    {
        GameObject existing = GameObject.Find(GeneratedPrefix + name);
        if (existing == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(existing);
        }
        else
        {
            DestroyImmediate(existing);
        }
    }

    private static Transform FindSceneTransform(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in transforms)
        {
            if (IsSceneTransform(t) && t.name == objectName)
            {
                return t;
            }
        }

        return null;
    }

    private static Material GetBarrierMaterial()
    {
        return GetWallSurfaceMaterial();
    }

    private static Material GetCeilingSurfaceMaterial()
    {
        if (ceilingSurfaceMaterial == null)
        {
            ceilingSurfaceMaterial = CreateTexturedMaterial(
                "FINAL_Scene2_Ceiling_Riet_Mat",
                "FINAL_Scene2_SurfaceMaterials/Ceiling/textures/riet_01_diff_4k",
                "FINAL_Scene2_SurfaceMaterials/Ceiling/textures/riet_01_nor_gl_4k",
                new Vector2(2.2f, 2.2f),
                0.32f,
                new Color(0.7f, 0.68f, 0.62f));
        }

        return ceilingSurfaceMaterial;
    }

    private static Material GetWallSurfaceMaterial()
    {
        if (wallSurfaceMaterial == null)
        {
            wallSurfaceMaterial = CreateTexturedMaterial(
                "FINAL_Scene2_Wall_RedBrick_Mat",
                "FINAL_Scene2_SurfaceMaterials/Wall/textures/red_brick_diff_4k",
                "FINAL_Scene2_SurfaceMaterials/Wall/textures/red_brick_nor_gl_4k",
                new Vector2(3.6f, 1.2f),
                0.24f,
                Color.white);
        }

        return wallSurfaceMaterial;
    }

    private static Material GetFloorSurfaceMaterial()
    {
        if (floorSurfaceMaterial == null)
        {
            floorSurfaceMaterial = CreateTexturedMaterial(
                "FINAL_Scene2_Floor_Wood_Mat",
                "FINAL_Scene2_SurfaceMaterials/Floor/textures/wood_floor_diff_4k",
                "FINAL_Scene2_SurfaceMaterials/Floor/textures/wood_floor_nor_gl_4k",
                new Vector2(2.8f, 2.8f),
                0.38f,
                Color.white);
        }

        return floorSurfaceMaterial;
    }

    private static Material GetCoverMaterial()
    {
        if (coverMaterial == null)
        {
            coverMaterial = CreateMaterial("FINAL_Scene2_Cover_Mat", new Color(0.33f, 0.29f, 0.24f));
        }

        return coverMaterial;
    }

    private static Material GetRouteMaterial()
    {
        if (routeMaterial == null)
        {
            routeMaterial = CreateMaterial("FINAL_Scene2_Route_Mat", new Color(0.04f, 0.42f, 0.58f));
        }

        return routeMaterial;
    }

    private static Material GetFinalRouteMaterial()
    {
        if (finalRouteMaterial == null)
        {
            finalRouteMaterial = CreateMaterial("FINAL_Scene2_Final_Route_Mat", new Color(0.62f, 0.19f, 0.07f));
        }

        return finalRouteMaterial;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Standard");
        Material material = shader != null ? new Material(shader) : new Material(Shader.Find("Diffuse"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static Material CreateTexturedMaterial(string name, string diffusePath, string normalPath, Vector2 tiling, float smoothness, Color fallbackColor)
    {
        Material material = CreateMaterial(name, fallbackColor);
        Texture2D diffuse = Resources.Load<Texture2D>(diffusePath);
        if (diffuse != null)
        {
            material.mainTexture = diffuse;
            material.mainTextureScale = tiling;
        }

        Texture2D normal = Resources.Load<Texture2D>(normalPath);
        if (normal != null && material.HasProperty("_BumpMap"))
        {
            material.SetTexture("_BumpMap", normal);
            material.SetTextureScale("_BumpMap", tiling);
            material.EnableKeyword("_NORMALMAP");
        }

        if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", smoothness);
        }

        return material;
    }

    private static Transform FindByAreaNameAndLocalPosition(string areaName, string objectName, Vector3 localPosition)
    {
        return FindByAreaNameAndLocalPosition(areaName, objectName, localPosition, PlacementTolerance);
    }

    private static Transform FindByAreaNameAndLocalPosition(string areaName, string objectName, Vector3 localPosition, float tolerance)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        float sqrTolerance = tolerance * tolerance;

        foreach (Transform t in transforms)
        {
            if (!IsSceneTransform(t) || t.name != objectName || !HasAncestor(t, areaName))
            {
                continue;
            }

            if ((t.localPosition - localPosition).sqrMagnitude <= sqrTolerance)
            {
                return t;
            }
        }

        return null;
    }

    private static bool HasAncestor(Transform transform, string ancestorName)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == ancestorName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool HasAncestorStartingWith(Transform transform, string ancestorPrefix)
    {
        string lowerPrefix = ancestorPrefix.ToLowerInvariant();
        Transform current = transform;
        while (current != null)
        {
            if (current.name.Trim().ToLowerInvariant().StartsWith(lowerPrefix))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindChildRecursive(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private static bool IsSceneTransform(Transform transform)
    {
        return transform != null && transform.gameObject.scene.IsValid() && transform.gameObject.scene.isLoaded;
    }
}
