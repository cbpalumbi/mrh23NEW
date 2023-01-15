using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Recognissimo.Core
{
    internal sealed class ContextHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public ContextHandle() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            try
            {
                PInvoke.ReleaseContext(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal enum Error
    {
        InvalidLanguageModel,
        InvalidSampleRate,
        Algorithm,
        Internal
    }

    internal struct ErrorReport
    {
        public Error Error;
        public string Description;
    }

    internal static partial class PInvoke
    {
        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_Create")]
        public static extern ContextHandle CreateContext(AlgorithmHandle algorithmHandle);


        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_Free")]
        public static extern void ReleaseContext(IntPtr contextHandle);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_Start")]
        public static extern int Start(ContextHandle contextHandle, LanguageModelResourceHandle modelResourceHandle,
            int sampleRate);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_Stop")]
        public static extern void Stop(ContextHandle contextHandle);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_Abort")]
        public static extern void Abort(ContextHandle contextHandle);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_LastError")]
        public static extern NativeErrorReport LastError(ContextHandle contextHandle);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_NextEvent")]
        public static extern ContextEvent NextEvent(ContextHandle contextHandle);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_EnqueueFloat32")]
        public static extern void EnqueueFloat32(ContextHandle contextHandle,
            [In] [MarshalAs(UnmanagedType.LPArray)]
            float[] samples, int length);

        [DllImport(Internal.LibName, EntryPoint = "Recognissimo_Context_EnqueuePCM16")]
        public static extern void EnqueuePcm16(ContextHandle contextHandle,
            [In] [MarshalAs(UnmanagedType.LPArray)]
            short[] samples, int length);

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeErrorReport
        {
            public readonly Error Error;

            public readonly IntPtr DescriptionPtr;
        }
    }

    internal enum ContextEvent
    {
        None,
        Started,
        Finished,
        Crashed,
        ResultReady
    }

    internal sealed class Context : IDisposable
    {
        private readonly ContextHandle _handle;

        public Context(Algorithm algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            _handle = PInvoke.CreateContext(algorithm.Handle);
        }

        public void Dispose()
        {
            if (_handle != null && !_handle.IsInvalid)
            {
                _handle.Dispose();
            }
        }

        public void Start(LanguageModelResource modelResource, int sampleRate)
        {
            PInvoke.Start(_handle, modelResource.Handle, sampleRate);
        }

        public void Stop()
        {
            PInvoke.Stop(_handle);
        }

        public void Abort()
        {
            PInvoke.Abort(_handle);
        }

        public ErrorReport LastError()
        {
            var nativeReport = PInvoke.LastError(_handle);

            return new ErrorReport
            {
                Error = nativeReport.Error,
                Description = Marshal.PtrToStringAnsi(nativeReport.DescriptionPtr)
            };
        }

        public ContextEvent NextEvent()
        {
            return PInvoke.NextEvent(_handle);
        }

        public void EnqueueFloat32(float[] buffer, int length)
        {
            PInvoke.EnqueueFloat32(_handle, buffer, length);
        }

        public void EnqueuePcm16(short[] buffer, int length)
        {
            PInvoke.EnqueuePcm16(_handle, buffer, length);
        }
    }
}