using UnityEditor;

namespace Recognissimo.Editor.BuildUtils
{
    internal static class Utils
    {
        public static void CheckAndroidForceSDCardPermission()
        {
            if (PlayerSettings.Android.forceSDCardPermission)
            {
                return;
            }
            
            const string message = "Recognissimo requires SD card permission for Android, " +
                                   "but this option is disabled in the project settings. " +
                                   "Do you want to enable it?";
            
            PlayerSettings.Android.forceSDCardPermission = EditorUtility.DisplayDialog("Android permission",
                message, "Enable", "Skip");
        }
    }
}