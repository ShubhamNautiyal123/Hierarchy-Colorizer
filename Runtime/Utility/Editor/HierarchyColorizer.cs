using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace LS.Utility.HierarchyColorizer
{
    public static class HierarchyColorizerPrefs
    {
        // PlayerPrefs keys
        public const string BackgroundColorKeyPrefix = "CustomBackgroundColor";
        public const string TextColorKeyPrefix = "CustomTextColor";
    }

    public class HierarchyColorizerEditor : EditorWindow
    {
        private List<Color> customBackgroundColors = new List<Color>();
        private List<Color> customTextColors = new List<Color>();

        private Vector2 scrollPosition;

        [MenuItem("Window/Hierarchy Colorizer")]
        public static void ShowWindow()
        {
            GetWindow<HierarchyColorizerEditor>("Hierarchy Colorizer");
        }

        private void OnEnable()
        {
            LoadCustomColors();
        }

        private void LoadCustomColors()
        {
            customBackgroundColors.Clear();
            customTextColors.Clear();

            for (int i = 0; i < 9; i++)
            {
                Color bgColor = PlayerPrefs.HasKey(HierarchyColorizerPrefs.BackgroundColorKeyPrefix + i) ?
                    PlayerPrefsX.GetColor(HierarchyColorizerPrefs.BackgroundColorKeyPrefix + i) : Color.white;
                customBackgroundColors.Add(bgColor);

                Color textColor = PlayerPrefs.HasKey(HierarchyColorizerPrefs.TextColorKeyPrefix + i) ?
                    PlayerPrefsX.GetColor(HierarchyColorizerPrefs.TextColorKeyPrefix + i) : Color.black;
                customTextColors.Add(textColor);
            }
        }

        private void SaveCustomColors()
        {
            for (int i = 0; i < customBackgroundColors.Count; i++)
            {
                PlayerPrefsX.SetColor(HierarchyColorizerPrefs.BackgroundColorKeyPrefix + i, customBackgroundColors[i]);
                PlayerPrefsX.SetColor(HierarchyColorizerPrefs.TextColorKeyPrefix + i, customTextColors[i]);
            }
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            GUILayout.Label("Customize Hierarchy Colors", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            for (int i = 0; i < customBackgroundColors.Count; i++)
            {
                customBackgroundColors[i] = EditorGUILayout.ColorField("Bg Color - Level " + i, customBackgroundColors[i]);
            }

            for (int i = 0; i < customTextColors.Count; i++)
            {
                customTextColors[i] = EditorGUILayout.ColorField("Text Color - Level " + i, customTextColors[i]);
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Add Level"))
            {
                customBackgroundColors.Add(Color.white);
                customTextColors.Add(Color.white);
            }

            if (GUILayout.Button("Apply Colors"))
            {
                HierarchyColorizer.UpdateCustomColors(customBackgroundColors.ToArray(), customTextColors.ToArray());
                SaveCustomColors();
            }
        }
    }

    public static class PlayerPrefsX
    {
        public static void SetColor(string key, Color color)
        {
            PlayerPrefs.SetFloat(key + "_r", color.r);
            PlayerPrefs.SetFloat(key + "_g", color.g);
            PlayerPrefs.SetFloat(key + "_b", color.b);
            PlayerPrefs.SetFloat(key + "_a", color.a);
        }

        public static Color GetColor(string key)
        {
            float r = PlayerPrefs.GetFloat(key + "_r");
            float g = PlayerPrefs.GetFloat(key + "_g");
            float b = PlayerPrefs.GetFloat(key + "_b");
            float a = PlayerPrefs.GetFloat(key + "_a");
            return new Color(r, g, b, a);
        }
    }

    [InitializeOnLoad]
    public static class HierarchyColorizer
    {
        private static Color[] customBackgroundColors = new Color[9];
        private static Color[] customTextColors = new Color[9];

        static HierarchyColorizer()
        {
            LoadCustomColors();
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyChanged += RepaintHierarchyWindow;
        }

        public static void UpdateCustomColors(Color[] backgroundColors, Color[] textColors)
        {
            customBackgroundColors = backgroundColors;
            customTextColors = textColors;
            RepaintHierarchyWindow();
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null)
            {
                int hierarchyLevel = GetHierarchyLevel(gameObject.transform);

                bool isActive = GetActiveState(gameObject);

                (Color backgroundColor, Color textColor) = GetColorsForLevel(hierarchyLevel, isActive);

                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = textColor;

                // Apply different font styles based on hierarchy level
                if (hierarchyLevel == 0)
                {
                    style.alignment = TextAnchor.MiddleCenter; // Align text to center
                    style.fontStyle = FontStyle.Bold;
                    style.fontSize = Mathf.RoundToInt(style.fontSize + 2);
                }

                Rect offsetRect = new Rect(selectionRect);
                offsetRect.x += 15; // Offset for better visibility

                // Draw background for children of expanded GameObjects
                if (Selection.activeGameObject != null && Selection.activeGameObject.transform.IsChildOf(gameObject.transform) && !gameObject.transform.IsChildOf(Selection.activeGameObject.transform))
                {
                    EditorGUI.DrawRect(offsetRect, new Color(0.2f, 0.2f, 0.2f, 0.2f)); // Adjust background color and transparency as desired
                }

                // Calculate the size of the game object's name text
                Vector2 textSize = style.CalcSize(new GUIContent(gameObject.name));

                // Draw the background rectangle
                EditorGUI.DrawRect(offsetRect, backgroundColor);

                // Draw the black outline around the background rectangle
                DrawOutline(offsetRect, 1, Color.black); // Draw the black outline

                // Draw toggle button
                Rect toggleRect = new Rect(selectionRect);
                toggleRect.x = selectionRect.xMax - 20; // Adjust the position of the toggle button
                toggleRect.width = 20; // Set the width of the toggle button
                isActive = GUI.Toggle(toggleRect, isActive, "");

                // Toggle the active state if the button is clicked
                SetActiveState(gameObject, isActive);

                EditorGUI.LabelField(offsetRect, gameObject.name, style);
            }
        }

        private static void DrawOutline(Rect rect, int thickness, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static int GetHierarchyLevel(Transform transform)
        {
            int level = 0;
            while (transform.parent != null)
            {
                level++;
                transform = transform.parent;
            }
            return level;
        }

        private static bool GetActiveState(GameObject gameObject)
        {
            return gameObject.activeSelf;
        }

        private static void SetActiveState(GameObject gameObject, bool isActive)
        {
            if (gameObject.activeSelf != isActive)
            {
                Undo.RecordObject(gameObject, "Toggle Active State");
                gameObject.SetActive(isActive);
                EditorUtility.SetDirty(gameObject);
            }
        }

        private static (Color backgroundColor, Color textColor) GetColorsForLevel(int level, bool isActive)
        {
            Color bgIntensity = isActive ? Color.white : Color.gray;

            if (level < customBackgroundColors.Length && level < customTextColors.Length)
            {
                Color backgroundColor = customBackgroundColors[level];
                Color textColor = customTextColors[level];
                return (backgroundColor * bgIntensity, textColor);
            }
            else
            {
                Color defaultBgColor = customBackgroundColors[customBackgroundColors.Length - 1]; // Use the last color as default
                Color defaultTextColor = customTextColors[customTextColors.Length - 1];             // Use the last color as default
                return (defaultBgColor * bgIntensity, defaultTextColor);
            }
        }

        private static void RepaintHierarchyWindow()
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void LoadCustomColors()
        {
            for (int i = 0; i < 9; i++)
            {
                customBackgroundColors[i] = PlayerPrefsX.GetColor(HierarchyColorizerPrefs.BackgroundColorKeyPrefix + i);
                customTextColors[i] = PlayerPrefsX.GetColor(HierarchyColorizerPrefs.TextColorKeyPrefix + i);
            }
        }
    }
}
