using ElBestia.Combat;
using ElBestia.Skills;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ChampionCombatPresentation))]
    public sealed class ChampionCombatPresentationEditor : UnityEditor.Editor
    {
        private WeaponType previewWeapon = WeaponType.Sword;
        private float previewTime = 0.72f;
        private float previewSpeed = 0.2f;
        private bool playing;
        private double lastUpdate;

        private ChampionCombatPresentation Presentation => (ChampionCombatPresentation)target;

        private void OnEnable()
        {
            EditorApplication.update += TickPreview;
        }

        private void OnDisable()
        {
            EditorApplication.update -= TickPreview;
            StopPreview();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Slash Timing Preview", EditorStyles.boldLabel);
            previewWeapon = (WeaponType)EditorGUILayout.EnumPopup("Weapon", previewWeapon);
            previewTime = EditorGUILayout.Slider("Normalized Time", previewTime, 0f, 1f);
            previewSpeed = EditorGUILayout.Slider("Playback Speed", previewSpeed, 0.02f, 1f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scrub"))
                {
                    StopPreview();
                    Preview();
                }

                if (GUILayout.Button("Play Slowly"))
                {
                    previewTime = 0f;
                    playing = true;
                    lastUpdate = EditorApplication.timeSinceStartup;
                }

                if (GUILayout.Button("Stop"))
                {
                    StopPreview();
                }
            }

            EditorGUILayout.HelpBox(
                "Ajusta Projectile Release en Weapon Animations y usa este visor para encontrar el punto exacto del slash.",
                MessageType.Info);
        }

        private void TickPreview()
        {
            if (!playing || Presentation == null)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            previewTime += (float)(now - lastUpdate) * previewSpeed;
            lastUpdate = now;
            if (previewTime >= 1f)
            {
                previewTime = 1f;
                playing = false;
            }

            Preview();
            Repaint();
            SceneView.RepaintAll();
        }

        private void Preview()
        {
            Presentation.PreviewSlash(previewWeapon, previewTime);
        }

        private void StopPreview()
        {
            playing = false;
        }
    }
}
