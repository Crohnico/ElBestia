using System;
using System.Collections.Generic;
using ElBestia.Skills;
using ElBestia.UI;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ChargeIconCatalogSO))]
    public sealed class ChargeIconCatalogSOEditor : UnityEditor.Editor
    {
        private const float PreviewSize = 64f;
        private const float RowMinHeight = PreviewSize + 12f;
        private SerializedProperty entriesProperty;
        private bool normalizeOnNextDraw;

        private void OnEnable()
        {
            entriesProperty = serializedObject.FindProperty("entries");
            normalizeOnNextDraw = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool requestedNormalize = GUILayout.Button("Normalize Charge List", GUILayout.Height(26));
            if (normalizeOnNextDraw || requestedNormalize)
            {
                NormalizeEntries();
                normalizeOnNextDraw = false;
            }

            EditorGUILayout.Space(6f);
            DrawEntries();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntries()
        {
            EditorGUILayout.LabelField("Charge Icons", EditorStyles.boldLabel);
            foreach (ChargeType charge in GetDrawableCharges())
            {
                SerializedProperty entry = FindEntry(charge);
                if (entry == null)
                {
                    continue;
                }

                SerializedProperty iconProperty = entry.FindPropertyRelative("icon");
                DrawChargeRow(charge, iconProperty);
            }
        }

        private static void DrawChargeRow(ChargeType charge, SerializedProperty iconProperty)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox, GUILayout.MinHeight(RowMinHeight)))
            {
                Rect previewRect = GUILayoutUtility.GetRect(PreviewSize, PreviewSize, GUILayout.Width(PreviewSize), GUILayout.Height(PreviewSize));
                DrawSpritePreview(previewRect, iconProperty.objectReferenceValue as Sprite);

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.Space(8f);
                    EditorGUILayout.LabelField(FormatChargeName(charge), EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(iconProperty, GUIContent.none);
                    if (iconProperty.objectReferenceValue == null)
                    {
                        EditorGUILayout.LabelField("No icon assigned", EditorStyles.miniLabel);
                    }
                }
            }
        }

        private static void DrawSpritePreview(Rect rect, Sprite sprite)
        {
            EditorGUI.DrawRect(rect, new Color(0.13f, 0.13f, 0.13f));
            GUI.Box(rect, GUIContent.none);
            if (sprite == null)
            {
                EditorGUI.LabelField(rect, "No Icon", CenteredMiniLabel());
                return;
            }

            Texture2D texture = sprite.texture;
            if (texture == null)
            {
                return;
            }

            Rect textureRect = sprite.textureRect;
            Rect coordinates = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);

            GUI.DrawTextureWithTexCoords(rect, texture, coordinates, true);
        }

        private static string FormatChargeName(ChargeType charge)
        {
            string raw = charge.ToString();
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            var result = new System.Text.StringBuilder(raw.Length + 8);
            result.Append(raw[0]);
            for (int i = 1; i < raw.Length; i++)
            {
                char current = raw[i];
                char previous = raw[i - 1];
                if (char.IsUpper(current) && !char.IsUpper(previous))
                {
                    result.Append(' ');
                }

                result.Append(current);
            }

            return result.ToString();
        }

        private void NormalizeEntries()
        {
            var iconsByCharge = new Dictionary<ChargeType, UnityEngine.Object>();
            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(i);
                ChargeType charge = (ChargeType)entry.FindPropertyRelative("charge").intValue;
                if (charge == ChargeType.None || iconsByCharge.ContainsKey(charge))
                {
                    continue;
                }

                iconsByCharge[charge] = entry.FindPropertyRelative("icon").objectReferenceValue;
            }

            ChargeType[] charges = GetDrawableCharges();
            entriesProperty.arraySize = charges.Length;
            for (int i = 0; i < charges.Length; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(i);
                SerializedProperty chargeProperty = entry.FindPropertyRelative("charge");
                SerializedProperty iconProperty = entry.FindPropertyRelative("icon");

                chargeProperty.intValue = (int)charges[i];
                iconProperty.objectReferenceValue = iconsByCharge.TryGetValue(charges[i], out UnityEngine.Object icon) ? icon : null;
            }
        }

        private SerializedProperty FindEntry(ChargeType charge)
        {
            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                SerializedProperty entry = entriesProperty.GetArrayElementAtIndex(i);
                if ((ChargeType)entry.FindPropertyRelative("charge").intValue == charge)
                {
                    return entry;
                }
            }

            return null;
        }

        private static ChargeType[] GetDrawableCharges()
        {
            var result = new List<ChargeType>();
            foreach (ChargeType charge in Enum.GetValues(typeof(ChargeType)))
            {
                if (charge != ChargeType.None)
                {
                    result.Add(charge);
                }
            }

            return result.ToArray();
        }

        private static GUIStyle CenteredMiniLabel()
        {
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
        }
    }
}
