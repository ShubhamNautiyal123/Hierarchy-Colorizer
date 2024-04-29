using UnityEngine;
using UnityEditor;

namespace LS
{
    [InitializeOnLoad]
    public static class HierarchyColorizer
    {
        static HierarchyColorizer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyChanged += RepaintHierarchyWindow;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null)
            {
                int hierarchyLevel = GetHierarchyLevel(gameObject.transform);

                bool isActive = GetActiveState(gameObject);

                (Color backgroundColor, Color textColor) = GetColorForLevel(hierarchyLevel, isActive);

                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = textColor;

                // Apply different font styles based on hierarchy level
                if (hierarchyLevel == 0)
                {
                    style.alignment = TextAnchor.MiddleCenter; // Align text to center
                    style.fontStyle = FontStyle.Bold;
                }
                else if (hierarchyLevel == 1)
                {
                    style.fontStyle = FontStyle.Bold;
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
            // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            // Left
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            // Right
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

        private static (Color backgroundColor, Color textColor) GetColorForLevel(int level, bool isActive)
        {
            // You can customize the colors for each level here
            float bgIntensity = isActive ? 1.0f : 0.5f;
            float textIntensity = 1.0f; // Always full intensity for text
            switch (level % 9)
            {
                case 0:
                    return (new Color(1f * bgIntensity, 0.85f * bgIntensity, 0.5f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel yellow
                case 1:
                    return (new Color(0.6f * bgIntensity, 0.8f * bgIntensity, 0.5f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel green
                case 2:
                    return (new Color(0.5f * bgIntensity, 0.7f * bgIntensity, 1f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel blue
                case 3:
                    return (new Color(1f * bgIntensity, 0.6f * bgIntensity, 0.6f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel red
                case 4:
                    return (new Color(1f * bgIntensity, 0.6f * bgIntensity, 1f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel magenta
                case 5:
                    return (new Color(0.8f * bgIntensity, 0.8f * bgIntensity, 0.8f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel gray
                case 6:
                    return (new Color(0.5f * bgIntensity, 1f * bgIntensity, 1f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Pastel cyan
                case 7:
                    return (new Color(0.4f * bgIntensity, 0.4f * bgIntensity, 0.4f * bgIntensity), new Color(1f, 1f, 1f, textIntensity)); // Light gray
                case 8:
                    return (new Color(0.9f * bgIntensity, 0.9f * bgIntensity, 0.9f * bgIntensity), new Color(0f, 0f, 0f, textIntensity)); // Very light gray
                default:
                    return (Color.black, Color.white);
            }
        }

        private static void RepaintHierarchyWindow()
        {
            EditorApplication.RepaintHierarchyWindow();
        }
    }

}

