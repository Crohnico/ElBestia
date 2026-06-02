using ElBestia.Combat;
using ElBestia.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ElBestia.Editor
{
    public static class ArenaSceneBinder
    {
        private const string ArenaScenePath = "Assets/Scenes/Arena.unity";

        [MenuItem("Tools/El Bestia/Wire Arena Scene")]
        public static void WireArenaScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ArenaScenePath, OpenSceneMode.Single);

            GameObject managerObject = GameObject.Find("GameManager");
            GameObject canvasObject = GameObject.Find("GameplayCanvas");
            GameObject spawnLeft = GameObject.Find("SpawnLeft");
            GameObject spawnRight = GameObject.Find("SpawnRight");
            GameObject cronoObject = GameObject.Find("Crono");
            GameObject timelineObject = GameObject.Find("ActionTimeLine") ?? GameObject.Find("ActionTimeline");

            if (managerObject == null)
            {
                Debug.LogError("Arena scene wiring failed: GameManager object was not found.");
                return;
            }

            if (canvasObject == null)
            {
                Debug.LogError("Arena scene wiring failed: GameplayCanvas object was not found.");
                return;
            }

            ArenaGameManager manager = GetOrAdd<ArenaGameManager>(managerObject);
            GameplayCanvas gameplayCanvas = GetOrAdd<GameplayCanvas>(canvasObject);
            CombatCrono crono = cronoObject != null ? GetOrAdd<CombatCrono>(cronoObject) : GetOrAdd<CombatCrono>(managerObject);
            if (timelineObject == null && cronoObject != null)
            {
                timelineObject = cronoObject;
            }

            ActionTimeLine actionTimeLine = timelineObject != null ? GetOrAdd<ActionTimeLine>(timelineObject) : Object.FindFirstObjectByType<ActionTimeLine>();
            if (actionTimeLine == null)
            {
                actionTimeLine = GetOrAdd<ActionTimeLine>(managerObject);
            }

            actionTimeLine.SetCrono(crono);
            actionTimeLine.AutoBind();
            EditorUtility.SetDirty(actionTimeLine);

            gameplayCanvas.AutoBind();
            gameplayCanvas.SetCrono(crono);
            EditorUtility.SetDirty(gameplayCanvas);

            SerializedObject serializedManager = new SerializedObject(manager);
            SetObjectReference(serializedManager, "spawnLeft", spawnLeft != null ? spawnLeft.transform : null);
            SetObjectReference(serializedManager, "spawnRight", spawnRight != null ? spawnRight.transform : null);
            SetObjectReference(serializedManager, "gameplayCanvas", gameplayCanvas);
            SetObjectReference(serializedManager, "crono", crono);
            SetObjectReference(serializedManager, "actionTimeLine", actionTimeLine);
            serializedManager.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(manager);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Arena scene wired: GameManager, GameplayCanvas, SpawnLeft and SpawnRight are connected.");
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void SetObjectReference(SerializedObject serializedObject, string propertyName, Object value)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }
    }
}
