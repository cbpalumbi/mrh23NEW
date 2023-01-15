using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Recognissimo.Core
{
    internal sealed class AlgorithmHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public AlgorithmHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                PInvoke.ReleaseAlgorithm(handle);
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
        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Algorithm_Create")]
        public static extern AlgorithmHandle CreateAlgorithm(AlgorithmType type);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Algorithm_Free")]
        public static extern void ReleaseAlgorithm(IntPtr algorithm);
    }

    internal enum AlgorithmType
    {
        SpeechRecognizer,
        VoiceActivityDetector,
        VoiceControl
    }

    internal abstract class Algorithm : IDisposable
    {
        internal readonly AlgorithmHandle Handle;

        internal Algorithm(AlgorithmType type)
        {
            Handle = PInvoke.CreateAlgorithm(type);
        }

        public void Dispose()
        {
            if (Handle is {IsInvalid: false})
            {
                Handle?.Dispose();
            }
        }

        public abstract bool Setup();

        public abstract void LoadResult();

        public abstract void DisposeResult();
    }
}