using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Recognissimo.Core
{
    internal class LanguageModelResourceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public LanguageModelResourceHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                PInvoke.ReleaseLanguageModel(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal static partial class PInvoke
    {
        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_LanguageModel_Create")]
        public static extern LanguageModelResourceHandle CreateLanguageModel(string path);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_LanguageModel_Free")]
        public static extern void ReleaseLanguageModel(IntPtr languageModelResource);
    }

    internal sealed class LanguageModelResource : IDisposable
    {
        internal readonly LanguageModelResourceHandle Handle;

        public LanguageModelResource(string path)
        {
            Handle = PInvoke.CreateLanguageModel(path);
        }

        public void Dispose()
        {
            if (Handle is {IsInvalid: false})
            {
                Handle.Dispose();
            }
        }
    }
}