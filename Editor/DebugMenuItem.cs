// 9/5/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;

public class DebugMenuItem
{
    [MenuItem("Tools/Debug/Create C# Script Menu")]
    private static void DebugCreateScriptMenu()
    {
        // Check if the menu item exists
        bool menuExists = Menu.GetChecked("Assets/Create/Scripting/C# Script");
        Debug.Log($"Menu 'Assets/Create/Scripting/C# Script' exists: {menuExists}");

        // Attempt to revalidate the menu
        if (!menuExists)
        {
            Debug.LogWarning("The menu item 'Assets/Create/Scripting/C# Script' is missing. Attempting to revalidate...");
            EditorApplication.ExecuteMenuItem("Assets/Create/Scripting/C# Script");
        }
        else
        {
            Debug.Log("The menu item is available and functional.");
        }
    }
}
