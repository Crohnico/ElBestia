using System;
using System.IO;
using ElBestia.Skills;
using ElBestia.Visuals;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    public static class CharacterPartCatalogBuilder
    {
        private const string CharacterPartsPath = "Assets/Resources/CharacterParts";
        private const string WeaponPartsPath = CharacterPartsPath + "/Weapons";
        private const string PropsPath = "Assets/Prefabs/Props";
        private const string StickmanMaterialPath = "Assets/Materials/StickmanUnlit.mat";

        [MenuItem("Tools/El Bestia/Rebuild Character Part Catalogs")]
        public static void RebuildCharacterPartCatalogs()
        {
            EnsureFolders();
            Material material = EnsureStickmanMaterial();
            ApplyMaterialToProps(material);
            ConfigureBaseStickmanPrefab("Assets/BaseStickman.prefab", material);
            ConfigureBaseStickmanPrefab("Assets/Prefabs/BaseStickman.prefab", material);

            HairSO hair = EnsureAsset<HairSO>(CharacterPartsPath + "/HairSO.asset");
            BeardSO beard = EnsureAsset<BeardSO>(CharacterPartsPath + "/BeardSO.asset");
            EarSO ear = EnsureAsset<EarSO>(CharacterPartsPath + "/EarSO.asset");

            FillCatalog(hair, PropsPath + "/Hair/Male", PropsPath + "/Hair/Female", PropsPath + "/Hair/Both");
            FillCatalog(beard, PropsPath + "/Beard", null, null);
            FillCatalog(ear, null, null, PropsPath + "/Ear");
            EnsureWeaponCatalogs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Character part catalogs rebuilt.");
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "CharacterParts");
            EnsureFolder(CharacterPartsPath, "Weapons");
            EnsureFolder("Assets", "Materials");
        }

        private static Material EnsureStickmanMaterial()
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(StickmanMaterialPath);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }

            material = new Material(shader)
            {
                name = "StickmanUnlit"
            };
            SetMaterialColor(material, new Color(0.82f, 0.62f, 0.48f));
            AssetDatabase.CreateAsset(material, StickmanMaterialPath);
            return material;
        }

        private static void ApplyMaterialToProps(Material material)
        {
            if (material == null || !AssetDatabase.IsValidFolder(PropsPath))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PropsPath });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Renderer targetRenderer = renderers[rendererIndex];
                    Material[] materials = targetRenderer.sharedMaterials;
                    bool changed = false;
                    for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                    {
                        if (materials[materialIndex] != null && materials[materialIndex] != material)
                        {
                            materials[materialIndex] = material;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        targetRenderer.sharedMaterials = materials;
                        EditorUtility.SetDirty(targetRenderer);
                    }
                }

                EditorUtility.SetDirty(prefab);
            }
        }

        private static void ConfigureBaseStickmanPrefab(string path, Material material)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || material == null)
            {
                return;
            }

            StickmanBodyConfigurator configurator = prefab.GetComponentInChildren<StickmanBodyConfigurator>(true);
            if (configurator == null)
            {
                return;
            }

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                Material[] materials = targetRenderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    if (materials[materialIndex] != null)
                    {
                        materials[materialIndex] = material;
                    }
                }

                targetRenderer.sharedMaterials = materials;
                EditorUtility.SetDirty(targetRenderer);
            }

            SerializedObject serializedConfigurator = new SerializedObject(configurator);
            serializedConfigurator.FindProperty("headAttachment").objectReferenceValue = FindChild(prefab.transform, "HeadAttachment");
            serializedConfigurator.FindProperty("beardAttachment").objectReferenceValue = FindChild(prefab.transform, "BeardAttachment");
            serializedConfigurator.FindProperty("earAttachment").objectReferenceValue = FindChild(prefab.transform, "EarAttachment");
            serializedConfigurator.FindProperty("leftHandAttachment").objectReferenceValue = FindChild(prefab.transform, "LeftHandAttachment");
            serializedConfigurator.FindProperty("rightHandAttachment").objectReferenceValue = FindChild(prefab.transform, "RightHandAttachment");
            SerializedProperty bodyRenderers = serializedConfigurator.FindProperty("bodyRenderers");
            bodyRenderers.ClearArray();
            for (int i = 0; i < renderers.Length; i++)
            {
                int index = bodyRenderers.arraySize;
                bodyRenderers.InsertArrayElementAtIndex(index);
                bodyRenderers.GetArrayElementAtIndex(index).objectReferenceValue = renderers[i];
            }

            serializedConfigurator.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(configurator);
            EditorUtility.SetDirty(prefab);
        }

        private static T EnsureAsset<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void FillCatalog(BodyPartCatalogSO catalog, string malePath, string femalePath, string bothPath)
        {
            if (catalog == null)
            {
                return;
            }

            SerializedObject serializedCatalog = new SerializedObject(catalog);
            FillList(serializedCatalog.FindProperty("male"), malePath);
            FillList(serializedCatalog.FindProperty("female"), femalePath);
            FillList(serializedCatalog.FindProperty("both"), bothPath);
            serializedCatalog.FindProperty("includeEmptyOption").boolValue = true;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static void FillList(SerializedProperty list, string folder)
        {
            list.ClearArray();
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            Array.Sort(guids, (left, right) => string.Compare(AssetDatabase.GUIDToAssetPath(left), AssetDatabase.GUIDToAssetPath(right), StringComparison.OrdinalIgnoreCase));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                int index = list.arraySize;
                list.InsertArrayElementAtIndex(index);
                list.GetArrayElementAtIndex(index).objectReferenceValue = prefab;
            }
        }

        private static void EnsureWeaponCatalogs()
        {
            EnsureWeaponCatalog(WeaponType.Fists);
            EnsureWeaponCatalog(WeaponType.Sword);
            EnsureWeaponCatalog(WeaponType.Axe);
            EnsureWeaponCatalog(WeaponType.Spear);
            EnsureWeaponCatalog(WeaponType.Staff);
            EnsureWeaponCatalog(WeaponType.Bow);
        }

        private static void EnsureWeaponCatalog(WeaponType weaponType)
        {
            string path = $"{WeaponPartsPath}/{weaponType}WeaponSO.asset";
            WeaponPartCatalogSO catalog = EnsureAsset<WeaponPartCatalogSO>(path);
            SerializedObject serializedCatalog = new SerializedObject(catalog);
            serializedCatalog.FindProperty("weaponType").enumValueIndex = (int)weaponType;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = Path.Combine(parent, child).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }
            else if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null || string.IsNullOrEmpty(childName))
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == childName)
                {
                    return children[i];
                }
            }

            return null;
        }
    }
}
