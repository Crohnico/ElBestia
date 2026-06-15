using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ElBestia.UI;

namespace ElBestia.Editor
{
    public static class DojoAdmissionScrollViewBuilder
    {
        private const string ScenePath = "Assets/Scenes/Dojo.unity";
        private const string SelectionTableName = "SelectionTable";
        private const string BackgroundName = "Background";
        private const string GridLayoutName = "GridLayout";
        private const string ScrollViewName = "ScrollView";
        private const string ViewportName = "Viewport";

        [MenuItem("Tools/El Bestia/Dojo/Wire Admission Selection ScrollView")]
        public static void WireAdmissionSelectionScrollView()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath);

            GameObject selectionTable = GameObject.Find(SelectionTableName);
            RectTransform background = FindChild(selectionTable.transform, BackgroundName).GetComponent<RectTransform>();
            RectTransform gridLayout = FindChild(background, GridLayoutName).GetComponent<RectTransform>();

            RectTransform scrollView = GetOrCreateRectTransform(ScrollViewName, background);
            CopyRectTransformLayout(gridLayout, scrollView);

            RectTransform viewport = GetOrCreateRectTransform(ViewportName, scrollView);
            StretchToParent(viewport);

            EnsureTransparentRaycastImage(viewport.gameObject);
            EnsureComponent<RectMask2D>(viewport.gameObject);

            gridLayout.SetParent(viewport, false);
            AnchorContentToTop(gridLayout);
            EnsureVerticalContentSize(gridLayout.gameObject);
            ConfigureGridLayout(gridLayout.gameObject);

            ScrollRect scrollRect = EnsureComponent<ScrollRect>(scrollView.gameObject);
            scrollRect.viewport = viewport;
            scrollRect.content = gridLayout;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            EnsureComponent<UIScrollRectResetToTop>(scrollView.gameObject);

            EditorUtility.SetDirty(scrollView);
            EditorUtility.SetDirty(viewport);
            EditorUtility.SetDirty(gridLayout);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static Transform FindChild(Transform root, string childName)
        {
            Transform child = root.Find(childName);
            if (child != null)
            {
                return child;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform deepChild = FindChild(root.GetChild(i), childName);
                if (deepChild != null)
                {
                    return deepChild;
                }
            }

            return null;
        }

        private static RectTransform GetOrCreateRectTransform(string objectName, Transform parent)
        {
            Transform existing = parent.Find(objectName);
            if (existing != null)
            {
                return existing.GetComponent<RectTransform>();
            }

            var gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.layer = parent.gameObject.layer;
            RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            return rectTransform;
        }

        private static void CopyRectTransformLayout(RectTransform source, RectTransform target)
        {
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.pivot = source.pivot;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }

        private static void StretchToParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private static void AnchorContentToTop(RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private static void EnsureTransparentRaycastImage(GameObject gameObject)
        {
            Image image = EnsureComponent<Image>(gameObject);
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
        }

        private static void EnsureVerticalContentSize(GameObject gameObject)
        {
            ContentSizeFitter fitter = EnsureComponent<ContentSizeFitter>(gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private static void ConfigureGridLayout(GameObject gameObject)
        {
            GridLayoutGroup gridLayout = gameObject.GetComponent<GridLayoutGroup>();
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
