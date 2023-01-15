using System;
using System.Runtime.InteropServices;

namespace Recognissimo.Core
{
    internal sealed class VoiceControlAlgorithm : Algorithm
    {
        private readonly Action<Result> _onResult;
        private readonly Func<Settings> _onSetup;

        public VoiceControlAlgorithm(Func<Settings> onSetup, Action<Result> onResult) : base(AlgorithmType.VoiceControl)
        {
            _onSetup = onSetup;
            _onResult = onResult;
        }

        public override bool Setup()
        {
            var settings = _onSetup();
            return PInvoke.Setup(Handle, settings.Commands, settings.AsapMode);
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
            public string Commands;
            public bool AsapMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Result
        {
            public readonly int CommandIndex;
        }

        private static class PInvoke
        {
            [DllImport(Internal.LibName, EntryPoint = "Recognissimo_VoiceControl_Setup")]
            public static extern bool Setup(AlgorithmHandle algorithm,
                [In] [MarshalAs(UnmanagedType.LPStr)] string commands, bool asapMode);

            [DllImport(Internal.LibName, EntryPoint = "Recognissimo_VoiceControl_Result")]
            public static extern bool TryGetResult(AlgorithmHandle algorithm, out Result result);
        }
    }
}