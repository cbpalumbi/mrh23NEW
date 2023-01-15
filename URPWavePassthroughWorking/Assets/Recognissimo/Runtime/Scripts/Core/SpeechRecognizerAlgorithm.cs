using System;
using System.Runtime.InteropServices;

namespace Recognissimo.Core
{
    internal sealed class SpeechRecognizerAlgorithm : Algorithm
    {
        public enum ResultType
        {
            Partial,
            Complete
        }

        private readonly Action<Result> _onResult;
        private readonly Func<Settings> _onSetup;

        public SpeechRecognizerAlgorithm(Func<Settings> onSetup, Action<Result> onResult) : base(AlgorithmType
            .SpeechRecognizer)
        {
            _onSetup = onSetup;
            _onResult = onResult;
        }

        public override bool Setup()
        {
            var settings = _onSetup();
            return PInvoke.Setup(Handle, settings.Vocabulary, settings.EnableDetails, settings.MaxAlternatives);
        }

        public override void LoadResult()
        {
            if (!PInvoke.TryGetResult(Handle, out var nativeResult))
            {
                return;
            }

            _onResult(new Result
            {
                Data = Marshal.PtrToStringAnsi(nativeResult.Data),
                ResultType = (ResultType) nativeResult.Complete
            });
        }

        public override void DisposeResult()
        {
            PInvoke.TryGetResult(Handle, out _);
        }

        public struct Settings
        {
            public string Vocabulary;
            public bool EnableDetails;
            public int MaxAlternatives;
        }

        public struct Result
        {
            public ResultType ResultType;
            public string Data;
        }

        private static class PInvoke
        {
            [DllImport(Internal.LibName, EntryPoint = "Recognissimo_SpeechRecognizer_Setup")]
            public static extern bool Setup(AlgorithmHandle algorithm,
                [In] [MarshalAs(UnmanagedType.LPStr)] string vocabulary, bool enableDetails, int maxAlternatives);

            [DllImport(Internal.LibName, EntryPoint = "Recognissimo_SpeechRecognizer_Result")]
            public static extern bool TryGetResult(AlgorithmHandle algorithm, out NativeResult nativeResult);

            [StructLayout(LayoutKind.Sequential)]
            public struct NativeResult
            {
                public readonly int Complete;
                public readonly IntPtr Data;
            }
        }
    }
}