using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Bililive_dm
{
    public static class Store
    {
        public static double MainOverlayXoffset = 0;
        public static double MainOverlayYoffset = 0;
        public static double MainOverlayWidth = 250;
        public static double MainOverlayEffect1 = 0.4; //拉伸
        public static double MainOverlayEffect2 = 0.4; //文字出現
        public static double MainOverlayEffect3 = 6; //文字停留
        public static double MainOverlayEffect4 = 1; //窗口消失
        public static double MainOverlayFontsize = 17;

        public static double FullOverlayEffect1 = 400; //文字速度
        public static double FullOverlayFontsize = 35;
        public static bool WtfEngineEnabled = true;
    }


    public static class DefaultStore
    {
        public static double MainOverlayXoffset = 0;
        public static double MainOverlayYoffset = 0;
        public static double MainOverlayWidth = 250;
        public static double MainOverlayEffect1 = 0.4; //拉伸
        public static double MainOverlayEffect2 = 0.4; //文字出現
        public static double MainOverlayEffect3 = 6; //文字停留
        public static double MainOverlayEffect4 = 0.6; //窗口消失
        public static double MainOverlayFontsize = 17;

        public static double FullOverlayEffect1 = 400; //文字速度
        public static double FullOverlayFontsize = 35;
        public static bool WtfEngineEnabled = true;
    }

    public static class Utils
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr process,
            UIntPtr minimumWorkingSetSize, UIntPtr maximumWorkingSetSize);


        public static void ReleaseMemory(bool removePages)
        {
            // release any unused pages
            // making the numbers look good in task manager
            // this is totally nonsense in programming
            // but good for those users who care
            // making them happier with their everyday life
            // which is part of user experience
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            if (removePages)
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,
                    (UIntPtr) 0xFFFFFFFF, (UIntPtr) 0xFFFFFFFF);
        }
    }
}