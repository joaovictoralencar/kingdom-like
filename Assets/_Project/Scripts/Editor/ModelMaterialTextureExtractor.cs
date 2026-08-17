using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility that scans a folder for imported 3D models, extracts their
/// embedded materials and textures into real asset files, and avoids creating
/// duplicates by reusing any material/texture that already exists in the
/// folder with a matching name. If a material slot has no match on disk, a
/// brand-new Universal Render Pipeline/Lit material is created for it, and
/// the target folder tree is searched for textures to auto-wire into it.
///
/// Usage:
///  - Right-click a folder in the Project window ->
///    "Extract Materials & Textures (No Duplicates)"
///  - Or: Tools > Model Extraction > Extract Materials & Textures...
///    (opens a folder picker)
///
/// HOW "NO MATERIAL FOUND" IS HANDLED (both cases):
///  1. An embedded material exists (has a name) but nothing matching lives
///     in the model's folder -> a new URP/Lit material is created with that
///     name instead of extracting the embedded one.
///  2. A slot was previously mapped to "None" (via Unity's own Search and
///     Remap UI, or an earlier run of this tool) -> Unity's external object
///     map still remembers that slot's name even though it points at
///     nothing, so that name is recovered from
///     ModelImporter.GetExternalObjectMap() and a material is created for it
///     the same way.
///  A slot that never had an embedded material AND was never remapped has no
///  recoverable name anywhere in Unity's API, so that (rare) case is not
///  handled - there's nothing to name the new material after.
///
/// TEXTURE AUTO-ASSIGNMENT:
///  For slot "X" - whether its material is newly created OR reused from the
///  folder - the ENTIRE target folder tree (recursive, since package assets
///  often keep textures in a separate subfolder from the models) is searched
///  for textures named like "X_Albedo", "X_BaseColor", "X_Normal",
///  "X_Metallic", "X_Occlusion", "X_Emission", "X_Height", etc.
///  (case-insensitive, "_"/"-"/" " separator). Matches get wired into the
///  appropriate URP/Lit texture slot. For a REUSED material, only slots that
///  are currently empty get filled in - an existing assignment is never
///  overwritten. See TextureSuffixMap below for the full keyword list - flag
///  if your package uses different suffixes.
///
/// OTHER ASSUMPTIONS (flag these if you want different behavior):
///  - "Models" = imported model files (FBX, OBJ, etc.), matched via Unity's
///    built-in "t:Model" search filter, found recursively under the chosen
///    folder.
///  - MATERIAL duplicate-check is still scoped to the model's OWN folder
///    (unchanged from before) - only the texture search for auto-wiring new
///    materials was widened to the whole target folder tree.
/// </summary>
public static class ModelMaterialTextureExtractor
{
    private const string TempTextureFolderName = "__temp_tex_extract__";

    private static readonly (string Keyword, string ShaderProperty)[] TextureSuffixMap =
    {
        ("BaseColor", "_BaseMap"),
        ("Albedo", "_BaseMap"),
        ("Diffuse", "_BaseMap"),
        ("Color", "_BaseMap"),
        ("Normal", "_BumpMap"),
        ("Nrm", "_BumpMap"),
        ("Bump", "_BumpMap"),
        ("MetallicSmoothness", "_MetallicGlossMap"),
        ("MetallicGloss", "_MetallicGlossMap"),
        ("Metallic", "_MetallicGlossMap"),
        ("Occlusion", "_OcclusionMap"),
        ("AO", "_OcclusionMap"),
        ("Emission", "_EmissionMap"),
        ("Emissive", "_EmissionMap"),
        ("Height", "_ParallaxMap"),
        ("Displacement", "_ParallaxMap"),
    };

    [MenuItem("Assets/Extract Materials & Textures (No Duplicates)", true)]
    private static bool ValidateExtractFromContextMenu()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
    }

    [MenuItem("Assets/Extract Materials & Textures (No Duplicates)")]
    private static void ExtractFromContextMenu()
    {
        string folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        Run(folderPath);
    }

    [MenuItem("Tools/Model Extraction/Extract Materials & Textures...")]
    private static void ExtractFromMenu()
    {
        string absolutePath = EditorUtility.OpenFolderPanel("Choose folder with models", Application.dataPath, "");
        if (string.IsNullOrEmpty(absolutePath)) return;

        if (!absolutePath.StartsWith(Application.dataPath))
        {
            Debug.LogError("Please choose a folder inside this project's Assets folder.");
            return;
        }

        string relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
        Run(relativePath);
    }

    public static void Run(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { folderPath });

        int modelsProcessed = 0;
        int matsCreated = 0, matsReused = 0;
        int texExtracted = 0, texReused = 0;

        foreach (string guid in guids)
        {
            string modelPath = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) continue;

            string modelFolder = Path.GetDirectoryName(modelPath).Replace("\\", "/");

            // Embedded texture data first (rare for most packages, but if
            // present this rewires whichever materials reference it, so
            // the correct texture ends up carried into materials extracted
            // or created below).
            ExtractTexturesForModel(importer, modelPath, modelFolder, ref texExtracted, ref texReused);

            // Re-scan AFTER this model's own textures were extracted/deduped,
            // and search the WHOLE target folder tree - package assets often
            // keep textures in a different subfolder than the models.
            List<string> textureCandidates = AssetDatabase.FindAssets("t:Texture", new[] { folderPath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();

            bool needsReimport = ProcessMaterialsForModel(importer, modelPath, modelFolder, textureCandidates, ref matsCreated, ref matsReused);

            if (needsReimport)
                importer.SaveAndReimport();

            modelsProcessed++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ModelMaterialTextureExtractor] Processed {modelsProcessed} model(s). " +
                  $"Materials - created: {matsCreated}, reused: {matsReused}. " +
                  $"Textures - extracted: {texExtracted}, reused: {texReused}.");
    }

    // ModelImporter.ExtractTextures returns only a bool (success/fail) and
    // writes straight into the given folder, so we point it at an empty
    // scratch folder and treat everything that shows up there afterward as
    // "what got extracted" for this model.
    private static void ExtractTexturesForModel(ModelImporter importer, string modelPath, string modelFolder, ref int extracted, ref int reused)
    {
        string tempFolder = modelFolder + "/" + TempTextureFolderName;
        if (!AssetDatabase.IsValidFolder(tempFolder))
            AssetDatabase.CreateFolder(modelFolder, TempTextureFolderName);

        bool success = importer.ExtractTextures(tempFolder);
        AssetDatabase.Refresh();

        if (!success)
        {
            AssetDatabase.DeleteAsset(tempFolder);
            return;
        }

        string[] extractedGuids = AssetDatabase.FindAssets("t:Texture", new[] { tempFolder });
        bool anyDirty = false;

        foreach (string guid in extractedGuids)
        {
            string tempAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            Texture tempTex = AssetDatabase.LoadAssetAtPath<Texture>(tempAssetPath);
            string fileName = Path.GetFileName(tempAssetPath);
            string finalPath = modelFolder + "/" + fileName;
            Texture existingTex = AssetDatabase.LoadAssetAtPath<Texture>(finalPath);

            if (existingTex != null)
            {
                // Same-named texture already lives here. Repoint any of this
                // model's materials off the freshly extracted copy and onto
                // the existing one, then discard the copy.
                RepointMaterialTextures(modelPath, tempTex, existingTex);
                AssetDatabase.DeleteAsset(tempAssetPath);
                reused++;
                anyDirty = true;
            }
            else
            {
                string error = AssetDatabase.MoveAsset(tempAssetPath, finalPath);
                if (string.IsNullOrEmpty(error))
                    extracted++;
                else
                    Debug.LogWarning($"Could not move extracted texture '{tempAssetPath}' -> '{finalPath}': {error}");
            }
        }

        AssetDatabase.DeleteAsset(tempFolder);

        if (anyDirty)
            AssetDatabase.SaveAssets();
    }

    private static void RepointMaterialTextures(string modelPath, Texture oldTex, Texture newTex)
    {
        UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        foreach (Material mat in subAssets.OfType<Material>())
        {
            Shader shader = mat.shader;
            int propCount = ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < propCount; i++)
            {
                if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv)
                    continue;

                string propName = ShaderUtil.GetPropertyName(shader, i);
                if (mat.GetTexture(propName) == oldTex)
                {
                    mat.SetTexture(propName, newTex);
                    EditorUtility.SetDirty(mat);
                }
            }
        }
    }

    private static bool ProcessMaterialsForModel(ModelImporter importer, string modelPath, string modelFolder, List<string> textureCandidates, ref int created, ref int reused)
    {
        bool dirty = false;
        var handledNames = new HashSet<string>();

        // Case 1: slots with an embedded material (the normal case).
        UnityEngine.Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(modelPath);
        List<Material> embeddedMats = subAssets.OfType<Material>()
            .Where(m => AssetDatabase.IsSubAsset(m))
            .ToList();

        foreach (Material embeddedMat in embeddedMats)
        {
            var identifier = new AssetImporter.SourceAssetIdentifier(embeddedMat);
            ProcessSlot(importer, identifier, embeddedMat.name, modelFolder, textureCandidates, ref created, ref reused);
            handledNames.Add(embeddedMat.name);
            dirty = true;
        }

        // Case 2: slots with no current material, but a name Unity still
        // remembers because they were explicitly remapped to "None" before.
        var externalMapSnapshot = importer.GetExternalObjectMap().ToList();
        foreach (var kvp in externalMapSnapshot)
        {
            if (kvp.Key.type != typeof(Material)) continue;
            if (kvp.Value != null) continue;
            if (handledNames.Contains(kvp.Key.name)) continue;

            ProcessSlot(importer, kvp.Key, kvp.Key.name, modelFolder, textureCandidates, ref created, ref reused);
            handledNames.Add(kvp.Key.name);
            dirty = true;
        }

        return dirty;
    }

    private static void ProcessSlot(ModelImporter importer, AssetImporter.SourceAssetIdentifier identifier, string slotName, string modelFolder, List<string> textureCandidates, ref int created, ref int reused)
    {
        string matFileName = slotName + ".mat";
        string existingPath = modelFolder + "/" + matFileName;
        Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(existingPath);

        if (existingMat != null)
        {
            // Even for a reused material, search the folder tree for
            // matching textures and fill in any slot that's currently
            // empty. Slots that already have a texture assigned are left
            // alone - this only fills gaps, never overwrites.
            bool changed = AssignMatchingTextures(existingMat, slotName, textureCandidates, onlyIfEmpty: true);
            if (changed)
            {
                EditorUtility.SetDirty(existingMat);
                AssetDatabase.SaveAssets();
            }

            // Point the model's material slot at the existing asset instead
            // of creating a duplicate.
            importer.AddRemap(identifier, existingMat);
            reused++;
        }
        else
        {
            Material newMat = CreateMaterialForSlot(slotName, modelFolder, textureCandidates);
            importer.AddRemap(identifier, newMat);
            created++;
        }
    }

    private static Material CreateMaterialForSlot(string slotName, string modelFolder, List<string> textureCandidates)
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = new Material(urpLit != null ? urpLit : Shader.Find("Standard")) { name = slotName };

        AssignMatchingTextures(mat, slotName, textureCandidates, onlyIfEmpty: false);

        string newPath = AssetDatabase.GenerateUniqueAssetPath(modelFolder + "/" + slotName + ".mat");
        AssetDatabase.CreateAsset(mat, newPath);
        return mat;
    }

    // Searches textureCandidates (already gathered from the whole target
    // folder tree) for names like "<slotName>_Albedo", "<slotName>_Normal",
    // etc. and wires matches into the corresponding URP/Lit texture slot.
    // When onlyIfEmpty is true, a slot that already has a texture is left
    // untouched - used for reused materials so we never clobber an existing
    // deliberate assignment. Returns true if anything was assigned.
    private static bool AssignMatchingTextures(Material mat, string slotName, List<string> textureCandidates, bool onlyIfEmpty)
    {
        bool anyAssigned = false;

        foreach (string texPath in textureCandidates)
        {
            string texName = Path.GetFileNameWithoutExtension(texPath);
            if (!texName.StartsWith(slotName, StringComparison.OrdinalIgnoreCase))
                continue;

            string suffix = texName.Substring(slotName.Length).TrimStart('_', '-', ' ');
            if (suffix.Length == 0)
                continue;

            foreach (var map in TextureSuffixMap)
            {
                if (!string.Equals(suffix, map.Keyword, StringComparison.OrdinalIgnoreCase)) continue;
                if (!mat.HasProperty(map.ShaderProperty)) continue;

                if (onlyIfEmpty && mat.GetTexture(map.ShaderProperty) != null)
                    break; // already assigned - don't touch it

                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texPath);
                if (tex != null)
                {
                    mat.SetTexture(map.ShaderProperty, tex);
                    anyAssigned = true;
                }
                break;
            }
        }

        return anyAssigned;
    }
}