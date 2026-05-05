using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lightweight generated world visual for an element.
/// The prefab stores only an element id; primitives are built at runtime.
/// </summary>
[DisallowMultipleComponent]
public sealed class ElementWorldModel : MonoBehaviour
{
    [SerializeField] private string elementId;
    [SerializeField] private float modelScale = 1f;

    [Header("Imported Model Override")]
    [SerializeField] private GameObject sourceModel;
    [SerializeField] private string sourceChildName;
    [SerializeField] private Vector3 sourceModelOffset = Vector3.zero;
    [SerializeField] private Vector3 sourceModelEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 sourceModelScale = Vector3.one;
    [SerializeField] private float sourceModelFitSize = 0.65f;
    [SerializeField] private bool includeSourceNameVariants = true;

    private const string RootName = "GeneratedWorldModel";

    private static readonly Color Clay = new Color(0.55f, 0.38f, 0.28f);
    private static readonly Color Stone = new Color(0.45f, 0.45f, 0.42f);
    private static readonly Color Iron = new Color(0.55f, 0.58f, 0.6f);
    private static readonly Color Copper = new Color(0.75f, 0.36f, 0.14f);
    private static readonly Color Paper = new Color(0.86f, 0.73f, 0.47f);
    private static readonly Color Wood = new Color(0.42f, 0.23f, 0.1f);
    private static readonly Color Fire = new Color(1f, 0.35f, 0.05f);
    private static readonly Color Gold = new Color(1f, 0.74f, 0.22f);
    private static readonly Color Magic = new Color(0.55f, 0.28f, 1f);
    private static readonly Color DeepMagic = new Color(0.22f, 0.1f, 0.42f);
    private static readonly Color Glass = new Color(0.65f, 0.9f, 1f);

    private void Awake()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        ClearGeneratedRoot();

        Transform root = new GameObject(RootName).transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.Scale(Vector3.one * modelScale, GetParentScaleCompensation());

        Build(root, elementId);
    }

    private void Build(Transform root, string id)
    {
        if (TryBuildSourceModel(root))
            return;

        switch (id)
        {
            case "clay":
                LumpyPile(root, Clay);
                break;
            case "iron":
                Bar(root, Iron);
                break;
            case "flint":
                RockShard(root, new Color(0.25f, 0.27f, 0.28f));
                break;
            case "coal":
                LumpyPile(root, new Color(0.04f, 0.04f, 0.04f));
                break;
            case "wood":
                Log(root);
                break;
            case "air":
                Wind(root);
                break;
            case "water":
                Bottle(root, new Color(0.15f, 0.45f, 1f), true);
                break;
            case "saltpeter":
                Crystal(root, Color.white);
                break;
            case "lime":
                LumpyPile(root, new Color(0.82f, 0.82f, 0.62f));
                break;
            case "vitriol":
                Bottle(root, new Color(0.08f, 0.55f, 0.72f), true);
                break;
            case "herbs":
                Herbs(root);
                break;
            case "berries":
                Berries(root);
                break;
            case "horn":
                Horn(root);
                break;
            case "yeast":
                Jar(root, new Color(0.86f, 0.72f, 0.43f));
                break;
            case "cinnabar":
                Crystal(root, new Color(0.75f, 0.05f, 0.04f));
                break;
            case "chest":
                Chest(root);
                break;
            case "firesteel":
                Firesteel(root);
                break;
            case "fire":
                Flame(root);
                break;
            case "torch":
                Torch(root);
                break;
            case "ore":
                Ore(root);
                break;
            case "copper":
                Bar(root, Copper);
                break;
            case "dust":
                Dust(root);
                break;
            case "acid":
                Bottle(root, new Color(0.45f, 0.9f, 0.08f), true);
                break;
            case "must":
                Bottle(root, new Color(0.75f, 0.48f, 0.08f), false);
                break;
            case "explosion":
                Explosion(root);
                break;
            case "poison":
                Bottle(root, new Color(0.2f, 0.75f, 0.12f), true);
                AddSkullMark(root);
                break;
            case "gypsum":
                BrickStack(root, new Color(0.78f, 0.76f, 0.68f));
                break;
            case "alcohol":
                Bottle(root, new Color(0.9f, 0.92f, 1f), false);
                break;
            case "tincture":
                Bottle(root, new Color(0.42f, 0.12f, 0.72f), true);
                break;
            case "wine":
                Bottle(root, new Color(0.42f, 0.02f, 0.12f), false);
                break;
            case "elixir":
                Bottle(root, Magic, true);
                AddGlow(root, Magic);
                break;
            case "glue":
                Jar(root, new Color(0.92f, 0.86f, 0.62f));
                break;
            case "paper":
                Sheet(root, false);
                break;
            case "mechanism":
                Mechanism(root);
                break;
            case "blank_scroll":
                Scroll(root, false);
                break;
            case "ancient_scroll":
                Scroll(root, true);
                break;
            case "base_stone":
                CarvedStone(root, Stone);
                break;
            case "philosopher_stone":
                CarvedStone(root, new Color(0.8f, 0.08f, 0.12f));
                AddGlow(root, Gold);
                break;
            case "magic_wand":
                Wand(root);
                break;
            case "vessel":
                Vessel(root);
                break;
            case "spirit":
                Spirit(root);
                break;
            case "mind":
                Mind(root);
                break;
            case "body":
                Body(root);
                break;
            case "soul_essence":
                SoulEssence(root);
                break;
            case "soul":
                Soul(root);
                break;
            case "brick":
                BrickStack(root, new Color(0.62f, 0.18f, 0.12f));
                break;
            case "compote":
                Bottle(root, new Color(0.62f, 0.12f, 0.2f), false);
                AddBerry(root, new Vector3(0.08f, 0.16f, 0.12f));
                break;
            case "paint":
                Jar(root, new Color(0.8f, 0.1f, 0.12f));
                Cube(root, "Brush", new Vector3(0.18f, 0.28f, 0f), new Vector3(0.05f, 0.45f, 0.05f), Wood);
                break;
            case "slag":
                LumpyPile(root, new Color(0.18f, 0.16f, 0.14f));
                AddGlow(root, new Color(0.9f, 0.2f, 0.04f));
                break;
            case "slurry":
                Jar(root, new Color(0.35f, 0.38f, 0.28f));
                break;
            case "stone":
                CarvedStone(root, Stone);
                break;
            default:
                LumpyPile(root, Color.gray);
                break;
        }
    }

    private bool TryBuildSourceModel(Transform root)
    {
        if (sourceModel == null || string.IsNullOrWhiteSpace(sourceChildName))
            return false;

        GameObject instance = Instantiate(sourceModel, root);
        instance.name = "SourceModel_" + sourceChildName;
        instance.SetActive(true);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(sourceModelEulerAngles);
        instance.transform.localScale = sourceModelScale;

        List<Transform> targets = FindSourceChildren(instance.transform, sourceChildName);

        if (targets.Count == 0)
        {
            DestroyGenerated(instance);
            return false;
        }

        List<Renderer> targetRendererList = new List<Renderer>();

        for (int i = 0; i < targets.Count; i++)
        {
            EnsureActive(targets[i], instance.transform);
            targetRendererList.AddRange(targets[i].GetComponentsInChildren<Renderer>(true));
        }

        if (targetRendererList.Count == 0)
        {
            DestroyGenerated(instance);
            return false;
        }

        HashSet<Renderer> visibleRenderers = new HashSet<Renderer>(targetRendererList);
        Renderer[] allRenderers = instance.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in allRenderers)
            renderer.enabled = visibleRenderers.Contains(renderer);

        foreach (Renderer renderer in visibleRenderers)
            renderer.gameObject.SetActive(true);

        Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);

        foreach (Collider collider in colliders)
            DestroyGenerated(collider);

        Renderer[] targetRenderers = new Renderer[visibleRenderers.Count];
        visibleRenderers.CopyTo(targetRenderers);

        FitImportedModel(root, instance.transform, targetRenderers);
        CenterImportedModel(root, instance.transform, targetRenderers);

        return true;
    }

    private void FitImportedModel(Transform root, Transform modelRoot, Renderer[] renderers)
    {
        if (sourceModelFitSize <= 0f)
            return;

        Bounds bounds = GetRendererBounds(renderers);
        Vector3 localSize = root.InverseTransformVector(bounds.size);
        float currentSize = Mathf.Max(
            Mathf.Abs(localSize.x),
            Mathf.Abs(localSize.y),
            Mathf.Abs(localSize.z)
        );

        if (currentSize <= 0.0001f)
            return;

        float fitScale = sourceModelFitSize / currentSize;
        modelRoot.localScale *= fitScale;
    }

    private void CenterImportedModel(Transform root, Transform modelRoot, Renderer[] renderers)
    {
        Bounds bounds = GetRendererBounds(renderers);
        Vector3 localCenter = root.InverseTransformPoint(bounds.center);
        modelRoot.localPosition += sourceModelOffset - localCenter;
    }

    private static Bounds GetRendererBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return bounds;
    }

    private List<Transform> FindSourceChildren(Transform root, string childName)
    {
        List<Transform> matches = new List<Transform>();
        CollectSourceChildren(root, childName, matches);
        return matches;
    }

    private void CollectSourceChildren(Transform root, string childName, List<Transform> matches)
    {
        if (IsSourceChildMatch(root.name, childName))
            matches.Add(root);

        foreach (Transform child in root)
            CollectSourceChildren(child, childName, matches);
    }

    private bool IsSourceChildMatch(string objectName, string childName)
    {
        if (string.Equals(objectName, childName, StringComparison.OrdinalIgnoreCase))
            return true;

        if (!includeSourceNameVariants)
            return false;

        if (!objectName.StartsWith(childName, StringComparison.OrdinalIgnoreCase))
            return false;

        if (objectName.Length <= childName.Length)
            return false;

        char next = objectName[childName.Length];
        return next == '.' || next == '_' || char.IsDigit(next);
    }

    private static void EnsureActive(Transform target, Transform stopAt)
    {
        Transform current = target;

        while (current != null)
        {
            current.gameObject.SetActive(true);

            if (current == stopAt)
                return;

            current = current.parent;
        }
    }

    private void Bar(Transform root, Color color)
    {
        Cube(root, "IngotLower", new Vector3(-0.06f, -0.04f, 0.02f), new Vector3(0.42f, 0.12f, 0.2f), color);
        Cube(root, "IngotUpper", new Vector3(0.08f, 0.08f, -0.03f), new Vector3(0.36f, 0.1f, 0.18f), Color.Lerp(color, Color.white, 0.12f));
        Cube(root, "Highlight", new Vector3(0.02f, 0.135f, -0.04f), new Vector3(0.22f, 0.018f, 0.12f), Color.Lerp(color, Color.white, 0.38f));
    }

    private void Log(Transform root)
    {
        Cylinder(root, "LogA", new Vector3(0f, -0.04f, -0.06f), new Vector3(0.12f, 0.42f, 0.12f), Wood, Quaternion.Euler(0f, 0f, 90f));
        Cylinder(root, "LogB", new Vector3(0.03f, 0.1f, 0.08f), new Vector3(0.105f, 0.36f, 0.105f), Color.Lerp(Wood, Color.white, 0.08f), Quaternion.Euler(0f, 0f, 90f));
        Cylinder(root, "CutA", new Vector3(-0.22f, -0.04f, -0.06f), new Vector3(0.125f, 0.015f, 0.125f), new Color(0.72f, 0.49f, 0.24f), Quaternion.Euler(0f, 0f, 90f));
        Cylinder(root, "CutB", new Vector3(0.22f, -0.04f, -0.06f), new Vector3(0.125f, 0.015f, 0.125f), new Color(0.72f, 0.49f, 0.24f), Quaternion.Euler(0f, 0f, 90f));
    }

    private void LumpyPile(Transform root, Color color)
    {
        Sphere(root, "LumpA", new Vector3(-0.14f, 0f, 0f), new Vector3(0.32f, 0.2f, 0.28f), color);
        Sphere(root, "LumpB", new Vector3(0.12f, 0.03f, 0.05f), new Vector3(0.26f, 0.24f, 0.24f), Color.Lerp(color, Color.white, 0.08f));
        Sphere(root, "LumpC", new Vector3(0.02f, 0.08f, -0.12f), new Vector3(0.2f, 0.16f, 0.18f), Color.Lerp(color, Color.black, 0.1f));
    }

    private void RockShard(Transform root, Color color)
    {
        Cube(root, "ShardA", new Vector3(-0.1f, 0f, 0f), new Vector3(0.25f, 0.42f, 0.2f), color, Quaternion.Euler(18f, 0f, -18f));
        Cube(root, "ShardB", new Vector3(0.14f, -0.03f, 0.04f), new Vector3(0.18f, 0.34f, 0.16f), Color.Lerp(color, Color.white, 0.18f), Quaternion.Euler(-12f, 20f, 12f));
    }

    private void Crystal(Transform root, Color color)
    {
        Cylinder(root, "CrystalA", new Vector3(-0.08f, 0.08f, 0f), new Vector3(0.13f, 0.48f, 0.13f), color, Quaternion.Euler(0f, 0f, -10f));
        Cylinder(root, "CrystalB", new Vector3(0.11f, 0.02f, 0.05f), new Vector3(0.1f, 0.36f, 0.1f), Color.Lerp(color, Color.white, 0.2f), Quaternion.Euler(0f, 0f, 14f));
    }

    private void Bottle(Transform root, Color liquid, bool stopper)
    {
        Cylinder(root, "BottleBody", Vector3.zero, new Vector3(0.22f, 0.42f, 0.22f), Glass);
        Cylinder(root, "Liquid", new Vector3(0f, -0.06f, 0f), new Vector3(0.2f, 0.26f, 0.2f), liquid);
        Cylinder(root, "Neck", new Vector3(0f, 0.31f, 0f), new Vector3(0.1f, 0.22f, 0.1f), Glass);

        if (stopper)
            Cylinder(root, "Stopper", new Vector3(0f, 0.45f, 0f), new Vector3(0.12f, 0.08f, 0.12f), Wood);
    }

    private void Jar(Transform root, Color fill)
    {
        Cylinder(root, "Jar", Vector3.zero, new Vector3(0.24f, 0.34f, 0.24f), Glass);
        Cylinder(root, "Fill", new Vector3(0f, -0.04f, 0f), new Vector3(0.22f, 0.22f, 0.22f), fill);
        Cylinder(root, "Lid", new Vector3(0f, 0.23f, 0f), new Vector3(0.25f, 0.06f, 0.25f), Wood);
    }

    private void Sheet(Transform root, bool marked)
    {
        Cube(root, "Sheet", Vector3.zero, new Vector3(0.55f, 0.03f, 0.38f), Paper, Quaternion.Euler(0f, 0f, -8f));

        if (marked)
            Cube(root, "Mark", new Vector3(0f, 0.025f, 0f), new Vector3(0.32f, 0.02f, 0.035f), new Color(0.22f, 0.14f, 0.08f));
    }

    private void Scroll(Transform root, bool ancient)
    {
        Color parchment = ancient ? new Color(0.68f, 0.5f, 0.25f) : Paper;
        Cylinder(root, "RollA", new Vector3(-0.2f, 0f, 0f), new Vector3(0.07f, 0.28f, 0.07f), Wood, Quaternion.Euler(90f, 0f, 0f));
        Cylinder(root, "RollB", new Vector3(0.2f, 0f, 0f), new Vector3(0.07f, 0.28f, 0.07f), Wood, Quaternion.Euler(90f, 0f, 0f));
        Cube(root, "Parchment", Vector3.zero, new Vector3(0.4f, 0.025f, 0.28f), parchment);
        Cube(root, "EdgeTop", new Vector3(0f, 0.026f, 0.12f), new Vector3(0.34f, 0.018f, 0.025f), Color.Lerp(parchment, Color.white, 0.18f));
        Cube(root, "EdgeBottom", new Vector3(0f, 0.026f, -0.12f), new Vector3(0.34f, 0.018f, 0.025f), Color.Lerp(parchment, Color.black, 0.1f));

        if (ancient)
        {
            Cube(root, "RuneLine", new Vector3(0f, 0.03f, 0f), new Vector3(0.28f, 0.02f, 0.035f), new Color(0.12f, 0.28f, 0.45f));
            Cube(root, "RuneLineSmall", new Vector3(-0.04f, 0.032f, -0.065f), new Vector3(0.16f, 0.02f, 0.03f), new Color(0.12f, 0.28f, 0.45f));
            Sphere(root, "Seal", new Vector3(0.16f, 0.04f, 0.09f), Vector3.one * 0.08f, new Color(0.7f, 0.05f, 0.04f));
        }
    }

    private void Mechanism(Transform root)
    {
        Cylinder(root, "GearCore", Vector3.zero, new Vector3(0.24f, 0.08f, 0.24f), Copper);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(0.28f, 0f, 0f);
            Cube(root, "Tooth" + i, position, new Vector3(0.13f, 0.08f, 0.08f), Copper, Quaternion.Euler(0f, angle, 0f));
        }

        Cylinder(root, "Axle", Vector3.zero, new Vector3(0.08f, 0.13f, 0.08f), Iron);
    }

    private void CarvedStone(Transform root, Color color)
    {
        Sphere(root, "StoneCore", Vector3.zero, new Vector3(0.3f, 0.28f, 0.3f), color);
        Cube(root, "FacetA", new Vector3(0.08f, 0.14f, -0.06f), new Vector3(0.18f, 0.025f, 0.14f), Color.Lerp(color, Color.white, 0.28f), Quaternion.Euler(20f, 0f, 10f));
        Cube(root, "FacetB", new Vector3(-0.1f, -0.02f, 0.08f), new Vector3(0.14f, 0.025f, 0.12f), Color.Lerp(color, Color.black, 0.18f), Quaternion.Euler(-18f, 0f, -18f));
    }

    private void Wand(Transform root)
    {
        Cylinder(root, "Handle", Vector3.zero, new Vector3(0.045f, 0.55f, 0.045f), Wood, Quaternion.Euler(0f, 0f, -42f));
        Sphere(root, "Gem", new Vector3(0.2f, 0.2f, 0f), Vector3.one * 0.13f, Magic);
        Cube(root, "BandA", new Vector3(0.08f, 0.08f, 0f), new Vector3(0.14f, 0.035f, 0.07f), Copper, Quaternion.Euler(0f, 0f, -42f));
        Cube(root, "SparkA", new Vector3(0.29f, 0.2f, 0f), new Vector3(0.14f, 0.018f, 0.018f), Gold);
        Cube(root, "SparkB", new Vector3(0.2f, 0.29f, 0f), new Vector3(0.018f, 0.14f, 0.018f), Gold);
    }

    private void Vessel(Transform root)
    {
        Cylinder(root, "Bowl", new Vector3(0f, 0.02f, 0f), new Vector3(0.26f, 0.22f, 0.26f), new Color(0.35f, 0.46f, 0.5f));
        Cylinder(root, "Rim", new Vector3(0f, 0.16f, 0f), new Vector3(0.31f, 0.035f, 0.31f), Color.Lerp(Iron, Color.white, 0.12f));
        Cylinder(root, "Opening", new Vector3(0f, 0.19f, 0f), new Vector3(0.2f, 0.03f, 0.2f), new Color(0.08f, 0.12f, 0.14f));
        Cylinder(root, "Foot", new Vector3(0f, -0.14f, 0f), new Vector3(0.14f, 0.08f, 0.14f), Stone);
        Sphere(root, "Breath", new Vector3(0f, 0.34f, 0f), Vector3.one * 0.09f, new Color(0.72f, 0.9f, 1f));
    }

    private void Spirit(Transform root)
    {
        Sphere(root, "Head", new Vector3(0f, 0.2f, 0f), new Vector3(0.2f, 0.19f, 0.2f), new Color(0.72f, 0.92f, 1f));
        Sphere(root, "Body", new Vector3(0f, -0.03f, 0f), new Vector3(0.18f, 0.28f, 0.18f), new Color(0.48f, 0.78f, 1f));
        Sphere(root, "Tail", new Vector3(0f, -0.28f, 0f), new Vector3(0.08f, 0.14f, 0.08f), new Color(0.48f, 0.78f, 1f));
        Sphere(root, "EyeL", new Vector3(-0.06f, 0.22f, -0.16f), Vector3.one * 0.025f, DeepMagic);
        Sphere(root, "EyeR", new Vector3(0.06f, 0.22f, -0.16f), Vector3.one * 0.025f, DeepMagic);
    }

    private void Mind(Transform root)
    {
        Sphere(root, "MindOrb", Vector3.zero, Vector3.one * 0.27f, Magic);
        Sphere(root, "LobeA", new Vector3(-0.12f, 0.06f, -0.02f), Vector3.one * 0.12f, Color.Lerp(Magic, Color.white, 0.16f));
        Sphere(root, "LobeB", new Vector3(0.12f, 0.06f, -0.02f), Vector3.one * 0.12f, Color.Lerp(Magic, Color.white, 0.16f));
        Cube(root, "ThoughtA", new Vector3(0f, 0.02f, -0.22f), new Vector3(0.38f, 0.02f, 0.02f), Color.white, Quaternion.Euler(0f, 0f, 35f));
        Cube(root, "ThoughtB", new Vector3(0f, 0.02f, -0.22f), new Vector3(0.38f, 0.02f, 0.02f), Color.white, Quaternion.Euler(0f, 0f, -35f));
    }

    private void Body(Transform root)
    {
        Sphere(root, "Head", new Vector3(0f, 0.26f, 0f), Vector3.one * 0.13f, Clay);
        Cube(root, "Torso", new Vector3(0f, 0.02f, 0f), new Vector3(0.24f, 0.3f, 0.16f), Clay);
        Cube(root, "ArmL", new Vector3(-0.2f, 0.04f, 0f), new Vector3(0.09f, 0.24f, 0.09f), Clay, Quaternion.Euler(0f, 0f, -10f));
        Cube(root, "ArmR", new Vector3(0.2f, 0.04f, 0f), new Vector3(0.09f, 0.24f, 0.09f), Clay, Quaternion.Euler(0f, 0f, 10f));
        Cube(root, "LegL", new Vector3(-0.07f, -0.22f, 0f), new Vector3(0.09f, 0.2f, 0.09f), Clay);
        Cube(root, "LegR", new Vector3(0.07f, -0.22f, 0f), new Vector3(0.09f, 0.2f, 0.09f), Clay);
        Cube(root, "ChestMark", new Vector3(0f, 0.06f, -0.085f), new Vector3(0.13f, 0.025f, 0.025f), Copper);
    }

    private void SoulEssence(Transform root)
    {
        Sphere(root, "Essence", Vector3.zero, Vector3.one * 0.24f, new Color(0.75f, 0.35f, 1f));
        Sphere(root, "InnerSpark", new Vector3(0.06f, 0.08f, -0.05f), Vector3.one * 0.08f, Color.white);
        AddGlow(root, new Color(0.85f, 0.7f, 1f));
    }

    private void Soul(Transform root)
    {
        Sphere(root, "SoulCore", Vector3.zero, Vector3.one * 0.28f, new Color(0.95f, 0.78f, 1f));
        Sphere(root, "SoulCenter", Vector3.zero, Vector3.one * 0.14f, Color.white);
        Cube(root, "RayA", Vector3.zero, new Vector3(0.58f, 0.025f, 0.025f), Color.white, Quaternion.Euler(0f, 0f, 45f));
        Cube(root, "RayB", Vector3.zero, new Vector3(0.58f, 0.025f, 0.025f), Color.white, Quaternion.Euler(0f, 0f, -45f));
        Cube(root, "RayC", Vector3.zero, new Vector3(0.025f, 0.58f, 0.025f), Color.white);
    }

    private void Herbs(Transform root)
    {
        for (int i = -1; i <= 1; i++)
            Cube(root, "Stem" + i, new Vector3(i * 0.07f, 0.04f, 0f), new Vector3(0.025f, 0.42f, 0.025f), new Color(0.1f, 0.45f, 0.14f), Quaternion.Euler(0f, 0f, i * 16f));

        Sphere(root, "LeafA", new Vector3(-0.12f, 0.17f, 0f), new Vector3(0.12f, 0.05f, 0.06f), new Color(0.18f, 0.62f, 0.18f));
        Sphere(root, "LeafB", new Vector3(0.12f, 0.12f, 0f), new Vector3(0.12f, 0.05f, 0.06f), new Color(0.18f, 0.62f, 0.18f));
    }

    private void Berries(Transform root)
    {
        AddBerry(root, new Vector3(-0.11f, 0.02f, 0f));
        AddBerry(root, new Vector3(0.07f, 0.03f, 0.06f));
        AddBerry(root, new Vector3(0.01f, 0.17f, -0.04f));
    }

    private void Horn(Transform root)
    {
        Cylinder(root, "HornBase", new Vector3(-0.06f, 0f, 0f), new Vector3(0.18f, 0.42f, 0.18f), new Color(0.78f, 0.68f, 0.48f), Quaternion.Euler(0f, 0f, 65f));
        Cylinder(root, "HornTip", new Vector3(0.18f, 0.18f, 0f), new Vector3(0.1f, 0.28f, 0.1f), new Color(0.92f, 0.85f, 0.68f), Quaternion.Euler(0f, 0f, 65f));
    }

    private void Chest(Transform root)
    {
        Cube(root, "Box", new Vector3(0f, -0.04f, 0f), new Vector3(0.52f, 0.28f, 0.34f), Wood);
        Cube(root, "Lid", new Vector3(0f, 0.15f, 0f), new Vector3(0.56f, 0.12f, 0.38f), new Color(0.5f, 0.28f, 0.12f));
        Cube(root, "Band", new Vector3(0f, 0.05f, 0f), new Vector3(0.08f, 0.42f, 0.39f), Iron);
        Cube(root, "Lock", new Vector3(0f, 0.02f, -0.2f), new Vector3(0.12f, 0.12f, 0.04f), Gold);
    }

    private void Firesteel(Transform root)
    {
        Cube(root, "Steel", new Vector3(-0.08f, 0f, 0f), new Vector3(0.42f, 0.08f, 0.08f), Iron, Quaternion.Euler(0f, 0f, 25f));
        RockShard(root, new Color(0.18f, 0.2f, 0.2f));
    }

    private void Flame(Transform root)
    {
        Sphere(root, "OuterFlame", Vector3.zero, new Vector3(0.26f, 0.42f, 0.22f), Fire);
        Sphere(root, "InnerFlame", new Vector3(0f, -0.03f, 0f), new Vector3(0.14f, 0.27f, 0.12f), Gold);
    }

    private void Torch(Transform root)
    {
        Cylinder(root, "Handle", new Vector3(0f, -0.12f, 0f), new Vector3(0.06f, 0.62f, 0.06f), Wood);
        Sphere(root, "Flame", new Vector3(0f, 0.28f, 0f), new Vector3(0.22f, 0.3f, 0.18f), Fire);
        Sphere(root, "Core", new Vector3(0f, 0.24f, 0f), new Vector3(0.12f, 0.18f, 0.1f), Gold);
    }

    private void Ore(Transform root)
    {
        LumpyPile(root, Stone);
        Sphere(root, "MetalSpeck", new Vector3(0.11f, 0.1f, -0.1f), Vector3.one * 0.07f, Copper);
    }

    private void Dust(Transform root)
    {
        for (int i = 0; i < 7; i++)
        {
            float x = (i % 3 - 1) * 0.11f;
            float z = (i / 3 - 1) * 0.1f;
            Sphere(root, "Dust" + i, new Vector3(x, 0f, z), Vector3.one * (0.045f + i % 2 * 0.015f), new Color(0.55f, 0.52f, 0.45f));
        }
    }

    private void Explosion(Transform root)
    {
        Sphere(root, "Burst", Vector3.zero, Vector3.one * 0.28f, Fire);
        Cube(root, "SpikeA", Vector3.zero, new Vector3(0.78f, 0.06f, 0.06f), Gold, Quaternion.Euler(0f, 0f, 25f));
        Cube(root, "SpikeB", Vector3.zero, new Vector3(0.78f, 0.06f, 0.06f), Gold, Quaternion.Euler(0f, 0f, -25f));
        Cube(root, "SpikeC", Vector3.zero, new Vector3(0.06f, 0.78f, 0.06f), Gold);
    }

    private void BrickStack(Transform root, Color color)
    {
        Cube(root, "BrickA", new Vector3(-0.14f, 0f, 0f), new Vector3(0.28f, 0.12f, 0.18f), color);
        Cube(root, "BrickB", new Vector3(0.16f, 0f, 0f), new Vector3(0.28f, 0.12f, 0.18f), color);
        Cube(root, "BrickC", new Vector3(0.02f, 0.13f, 0f), new Vector3(0.28f, 0.12f, 0.18f), Color.Lerp(color, Color.white, 0.1f));
    }

    private void Wind(Transform root)
    {
        Color airColor = new Color(0.72f, 0.9f, 1f);
        Cube(root, "WindA", new Vector3(0f, 0.12f, 0f), new Vector3(0.36f, 0.03f, 0.03f), airColor, Quaternion.Euler(0f, 0f, -10f));
        Cube(root, "WindB", new Vector3(0.05f, -0.02f, 0f), new Vector3(0.28f, 0.03f, 0.03f), Color.Lerp(airColor, Color.white, 0.18f), Quaternion.Euler(0f, 0f, 14f));
        Cube(root, "WindC", new Vector3(-0.03f, -0.13f, 0f), new Vector3(0.2f, 0.03f, 0.03f), Color.Lerp(airColor, Color.white, 0.28f), Quaternion.Euler(0f, 0f, -18f));
        Sphere(root, "PuffA", new Vector3(-0.2f, -0.02f, 0f), Vector3.one * 0.09f, airColor);
        Sphere(root, "PuffB", new Vector3(0.18f, 0.12f, 0f), Vector3.one * 0.06f, Color.Lerp(airColor, Color.white, 0.25f));
    }

    private void AddGlow(Transform root, Color color)
    {
        Sphere(root, "Glow", new Vector3(0f, 0.02f, 0f), Vector3.one * 0.48f, new Color(color.r, color.g, color.b, 0.18f));
    }

    private void AddSkullMark(Transform root)
    {
        Sphere(root, "Skull", new Vector3(0f, 0.12f, -0.22f), Vector3.one * 0.08f, Color.white);
        Cube(root, "SkullJaw", new Vector3(0f, 0.04f, -0.22f), new Vector3(0.1f, 0.06f, 0.03f), Color.white);
    }

    private void AddBerry(Transform root, Vector3 position)
    {
        Sphere(root, "Berry", position, Vector3.one * 0.11f, new Color(0.55f, 0.02f, 0.12f));
    }

    private GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        return Cube(parent, name, position, scale, color, Quaternion.identity);
    }

    private GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
    {
        return Primitive(parent, PrimitiveType.Cube, name, position, scale, color, rotation);
    }

    private GameObject Sphere(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        return Primitive(parent, PrimitiveType.Sphere, name, position, scale, color, Quaternion.identity);
    }

    private GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        return Cylinder(parent, name, position, scale, color, Quaternion.identity);
    }

    private GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Color color, Quaternion rotation)
    {
        return Primitive(parent, PrimitiveType.Cylinder, name, position, scale, color, rotation);
    }

    private GameObject Primitive(
        Transform parent,
        PrimitiveType primitiveType,
        string objectName,
        Vector3 position,
        Vector3 scale,
        Color color,
        Quaternion rotation)
    {
        GameObject instance = GameObject.CreatePrimitive(primitiveType);
        instance.name = objectName;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = position;
        instance.transform.localRotation = rotation;
        instance.transform.localScale = scale;

        Collider collider = instance.GetComponent<Collider>();

        if (collider != null)
            Destroy(collider);

        Renderer renderer = instance.GetComponent<Renderer>();

        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);
            material.color = color;
            renderer.material = material;
        }

        return instance;
    }

    private void ClearGeneratedRoot()
    {
        Transform existing = transform.Find(RootName);

        if (existing == null)
            return;

        DestroyGenerated(existing.gameObject);
    }

    private static void DestroyGenerated(UnityEngine.Object target)
    {
        if (target == null)
            return;

        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }

    private Vector3 GetParentScaleCompensation()
    {
        if (transform.parent == null)
            return Vector3.one;

        Vector3 parentScale = transform.parent.lossyScale;

        return new Vector3(
            SafeInverse(parentScale.x),
            SafeInverse(parentScale.y),
            SafeInverse(parentScale.z)
        );
    }

    private static float SafeInverse(float value)
    {
        if (Mathf.Abs(value) < 0.0001f)
            return 1f;

        return 1f / value;
    }
}
