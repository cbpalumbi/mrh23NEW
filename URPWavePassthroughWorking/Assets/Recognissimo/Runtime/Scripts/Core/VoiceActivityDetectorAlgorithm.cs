using System;
using System.Runtime.InteropServices;

namespace Recognissimo.Core
{
    internal sealed class VoiceActivityDetectorAlgorithm : Algorithm
    {
        private readonly Action<Result> _onResult;
        private readonly Func<Settings> _onSetup;

        public VoiceActivityDetectorAlgorithm(Func<Settings> onSetup, Action<Result> onResult)
            : base(AlgorithmType.VoiceActivityDetector)
        {
            _onSetup = onSetup;
            _onResult = onResult;
        }

        public override bool Setup()
        {
            var settings = _onSetup();
            return PInvoke.Setup(Handle, settings.TimeoutMs);
        }

        public override void LoadResult()
        {
            if (PInvoke.TryGetResult(Handle, out var result))
            {
                _onResult(result);
            }
        }

        public override void DisposeResult()
        {
            PInvoke.TryGetResult(Handle, out _);
        }

        public struct Settings
        {
            public int TimeoutMs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Result
        {
            public readonly bool IsActive;
        }

        private static class PInvoke
        {
            [DllImport(Internal.LibName, EntryPoint = "Recognissimo_VoiceActivityDetector_Setup")]
            public static extern bool Setup(AlgorithmHandle algorithm, int timeoutMs);

            [DllImport(Internal.LibName, EntryPoint = "Recognissimo_VoiceActivityDetector_Result")]
            public static extern bool TryGetResult(AlgorithmHandle algorithm, out Result result);
        }
    }
}