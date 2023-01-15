#if UNITY_WEBGL && !UNITY_EDITOR
#define UNITY_WEBGL_STANDALONE
#endif

using System.Collections;
#if UNITY_WEBGL_STANDALONE
using System.Runtime.InteropServices;
using AOT;
#endif

namespace Recognissimo.Utils
{
    public static class Filesystem
    {
        private static bool _isCommiting;

        public static bool RequiresSyncing()
        {
#if UNITY_WEBGL_STANDALONE
            return true;
#else
            return false;
#endif
        }

        public static IEnumerator Commit()
        {
#if UNITY_WEBGL_STANDALONE
            _isCommiting = true;

            RecognissimoUtils_Filesystem_Commit(CommitCallback);

            while (_isCommiting)
            {
                yield return null;
            }
#else
            yield break;
#endif
        }

#if UNITY_WEBGL_STANDALONE
        [MonoPInvokeCallback(typeof(NativeCallback))]
        private static void CommitCallback()
        {
            _isCommiting = false;
        }

        [DllImport("__Internal")]
        private static extern void RecognissimoUtils_Filesystem_Commit(NativeCallback callback);

        private delegate void NativeCallback();
#endif
    }
}