using System;
using System.Runtime.InteropServices;

namespace SDL
{
    /**
    *  \brief Audio format flags.
    *
    *  These are what the 16 bits in SDL_AudioFormat currently mean...
    *  (Unspecified bits are always zero).
    *
    *  \verbatim
    ++-----------------------sample is signed if set
    ||
    ||       ++-----------sample is bigendian if set
    ||       ||
    ||       ||          ++---sample is float if set
    ||       ||          ||
    ||       ||          || +---sample bit size---+
    ||       ||          || |                     |
    15 14 13 12 11 10 09 08 07 06 05 04 03 02 01 00
    \endverbatim
    *
    *  There are macros in SDL 2.0 and later to query these bits.
    */

    /**
    *  \name Audio flags
    */
    /* @{ */
    public enum AudioFlags : uint
    {
        Bitsize = 0xFF,
        Datatype = (1 << 8),
        Endian = (1 << 12),
        Signed = (1 << 15)
    }

    /**
    *  \name Audio format flags
    *
    *  Defaults to LSB byte order.
    */
    /* @{ */
    public enum AudioFormatFlags : uint
    {
        U8 = 0x0008,  /**< Unsigned 8-bit samples */
        S8 = 0x8008,  /**< Signed 8-bit samples */
        U16LSB = 0x0010,  /**< Unsigned 16-bit samples */
        S16LSB = 0x8010,  /**< Signed 16-bit samples */
        U16MSB = 0x1010,  /**< As above, but big-endian byte order */
        S16MSB = 0x9010,  /**< As above, but big-endian byte order */
        U16 = U16LSB,
        S16 = S16LSB,
        S32LSB = 0x8020,  /**< 32-bit integer samples */
        S32MSB = 0x9020,  /**< As above, but big-endian byte order */
        S32 = S32LSB,
        F32LSB = 0x8120,  /**< 32-bit floating point samples */
        F32MSB = 0x9120,  /**< As above, but big-endian byte order */
        F32 = F32LSB,
        U16SYS = U16LSB,
        S16SYS = S16LSB,
        S32SYS = S32LSB,
        F32SYS = F32LSB
    }
    /* @} */

    /**
    *  \name Allow change flags
    *
    *  Which audio format changes are allowed when opening a device.
    */
    /* @{ */
    public enum AudioChangeFlags : uint
    {
        AllowFrequencyChange = 0x00000001,
        AllowFormatChange = 0x00000002,
        AllowChannelsChange = 0x00000004,
        AllowSamplesChange = 0x00000008,
        AllowAnyChange = AllowFrequencyChange | AllowFormatChange | AllowChannelsChange | AllowSamplesChange
    }
    /* @} */

    /* @} *//* Audio flags */

    /**
    *  This function is called when the audio device needs more data.
    *
    *  \param userdata An application-specific parameter saved in
    *                  the SDL_AudioSpec structure
    *  \param stream A pointer to the audio data buffer.
    *  \param len    The length of that buffer in bytes.
    *
    *  Once the callback returns, the buffer will no longer be valid.
    *  Stereo samples are stored in a LRLRLR ordering.
    *
    *  You can choose to avoid callbacks and use SDL_QueueAudio() instead, if
    *  you like. Just open your audio device with a NULL callback.
    */

    /**
    *  The calculated values in this structure are calculated by SDL_OpenAudio().
    *
    *  For multi-channel audio, the default SDL channel mapping is:
    *  2:  FL FR                       (stereo)
    *  3:  FL FR LFE                   (2.1 surround)
    *  4:  FL FR BL BR                 (quad)
    *  5:  FL FR FC BL BR              (quad + center)
    *  6:  FL FR FC LFE SL SR          (5.1 surround - last two can also be BL BR)
    *  7:  FL FR FC LFE BC SL SR       (6.1 surround)
    *  8:  FL FR FC LFE BL BR SL SR    (7.1 surround)
    */

    [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
    public unsafe delegate void AudioCallback(void* userdata, byte* stream, int len);

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct AudioSpec
    {
        public int Frequency;       /* DSP frequency -- samples per second */
        public short Format;        /* Audio data format */
        public byte Channels;       /* Number of channels: 1 mono; 2 stereo */
        public byte Silence;        /* Audio buffer silence value (calculated) */
        public short Samples;       /* Audio buffer size in sample FRAMES (total samples divided by channel count) */
        public short Padding;       /* Necessary for some compile environments */
        public ulong Size;          /* Audio buffer size in bytes (calculated) */
        public IntPtr Callback;     /* Callback that feeds the audio device (NULL to use SDL_QueueAudio()). */
        public void* UserData;      /* Userdata passed to callback (ignored for NULL callbacks). */

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Wave
    {
        public uint Format;
        public int Width;
        public int Height;
        public int RefreshRate;
        public IntPtr DriverData;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RWops
    {
        private readonly IntPtr ptr;

        public RWops(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(RWops ops)
        {
            return ops.ptr;
        }

        public static implicit operator RWops(IntPtr ptr)
        {
            return new RWops(ptr);
        }
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_RWFromFile", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern RWops RWFromFile(string file, string mode);

        [DllImport(LibraryName, EntryPoint = "SDL_LoadWAV_RW", CallingConvention = CallingConvention.Cdecl)]
        public static extern AudioSpec* LoadWavRW(RWops src, int freesrc, AudioSpec* audioSpec, byte** buf, int* len);

        public static AudioSpec* LoadWavFromFile(String file, AudioSpec* spec, byte** buf, int* len)
        {
            var src = RWFromFile(file, "rb");
            var audSpec = LoadWavRW(src, 1, spec, buf, len);
            return audSpec;
        }

        [DllImport(LibraryName, EntryPoint = "SDL_GetNumAudioDrivers", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int GetNumAudioDrivers();


        [DllImport(LibraryName, EntryPoint = "SDL_GetAudioDriver", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern IntPtr GetAudioDriver(int index);


        [DllImport(LibraryName, EntryPoint = "SDL_OpenAudioDevice", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern uint OpenAudioDevice(string device, int iscapture, AudioSpec* desired, IntPtr obtained, int allowChanges);

        [DllImport(LibraryName, EntryPoint = "SDL_CloseAudioDevice", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern uint CloseAudioDevice(uint deviceId);

        [DllImport(LibraryName, EntryPoint = "SDL_PauseAudioDevice", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
        public static extern int PauseAudioDevice(uint deviceId, bool pauseOn);

        [DllImport(LibraryName, EntryPoint = "SDL_FreeWave", SetLastError = true)]
        public static extern int FreeWave(byte* buf);


        [DllImport(LibraryName, EntryPoint = "SDL_QueueAudio", SetLastError = true)]
        public static extern int QueueAudio(uint device, byte* buf, int length);

        [DllImport(LibraryName, EntryPoint = "SDL_Delay", SetLastError = true)]
        public static extern void Delay(uint ms);

        [DllImport(LibraryName, EntryPoint = "SDL_memcpy", SetLastError = true)]
        public static extern void* MemCopy(IntPtr dst, IntPtr src, int len);


        [DllImport(LibraryName, EntryPoint = "SDL_WriteToDataQueue", SetLastError = true)]
        public static extern void* WriteToDataQueue(IntPtr queue, IntPtr src, int len);

        [DllImport(LibraryName, EntryPoint = "SDL_GetQueuedAudioSize", SetLastError = true)]
        public static extern uint GetQueuedAudioSize(uint deviceId);

        [DllImport(LibraryName, EntryPoint = "SDL_ClearQueuedAudio", SetLastError = true)]
        public static extern void ClearQueuedAudio(uint deviceId);
    }
}
