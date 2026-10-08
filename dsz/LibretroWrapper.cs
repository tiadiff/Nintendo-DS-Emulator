using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace dsz
{
    public static class LibretroWrapper
    {
        private const string DllName = "melonds_libretro.dll";

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_game_info
        {
            public string path;
            public IntPtr data;
            public uint size;
            public string meta;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_system_timing
        {
            public double fps;
            public double sample_rate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_game_geometry
        {
            public uint base_width;
            public uint base_height;
            public uint max_width;
            public uint max_height;
            public float aspect_ratio;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_system_av_info
        {
            public retro_game_geometry geometry;
            public retro_system_timing timing;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool retro_environment_t(uint cmd, IntPtr data);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_video_refresh_t(IntPtr data, uint width, uint height, uint pitch);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_audio_sample_t(short left, short right);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint retro_audio_sample_batch_t(IntPtr data, uint frames);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void retro_input_poll_t();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short retro_input_state_t(uint port, uint device, uint index, uint id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_environment(retro_environment_t cb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_video_refresh(retro_video_refresh_t cb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_audio_sample(retro_audio_sample_t cb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_audio_sample_batch(retro_audio_sample_batch_t cb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_input_poll(retro_input_poll_t cb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_input_state(retro_input_state_t cb);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_deinit();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_set_controller_port_device(uint port, uint device);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool retro_load_game(ref retro_game_info game);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_unload_game();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_run();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void retro_get_system_av_info(out retro_system_av_info info);

        public const uint RETRO_MEMORY_SAVE_RAM = 0;

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr retro_get_memory_data(uint id);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint retro_get_memory_size(uint id);

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_memory_map
        {
            public IntPtr descriptors;
            public uint num_descriptors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct retro_memory_descriptor
        {
            public ulong flags;
            public IntPtr ptr;
            public UIntPtr offset;
            public UIntPtr start;
            public UIntPtr select;
            public UIntPtr disconnect;
            public UIntPtr len;
            public IntPtr addrspace;
        }
    }
}
