using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Recognissimo.Editor.BuildUtils
{
    public class RemoteLanguageModelProvider : IPreprocessBuildWithReport
    {
        public int callbackOrder { get; }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            Utils.CheckAndroidForceSDCardPermission();
        }
    }
}