#if !UNITY_EDITOR && (UNITY_WEBGL || UNITY_IOS)
#define RECOGNISSIMO_STATIC
#endif

namespace Recognissimo.Core
{
    internal static class Internal
    {
#if RECOGNISSIMO_STATIC
        public const string LibName = "__Internal";
#else
        public const string LibName = "libRecognissimo";
#endif
    }
}