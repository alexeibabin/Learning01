using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Editor;
using System.IO;

public class GenerateInputActions
{
    public static object Main()
    {
        var assetPath = "Assets/_Scripts/Input/PlayerInputActions.inputactions";
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(assetPath);

        if (asset == null)
            return "Error: Could not load asset at " + assetPath;

        try
        {
            // Generate code using the Input System's code generator
            var codeGenerator = new InputActionCodeGenerator();
            var code = codeGenerator.Generate(asset);

            var outputPath = Path.Combine(Path.GetDirectoryName(assetPath), "PlayerInputActions.cs");
            File.WriteAllText(outputPath, code);

            AssetDatabase.Refresh();
            return "Generated " + outputPath;
        }
        catch (System.Exception ex)
        {
            return "Error generating code: " + ex.Message;
        }
    }
}
