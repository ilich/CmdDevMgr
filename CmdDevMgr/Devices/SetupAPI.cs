using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CmdDevMgr.Devices
{
    static class SetupAPI
    {
        private const int BUFFER_SIZE = 8192;

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int ERROR_NOT_FOUND = 1168;

        public const int CR_SUCCESS = 0;

        public const int CM_PROB_DISABLED = 0x16;

        public const int SPDRP_DEVICEDESC = 0x0;

        public const int SPDRP_HARDWAREID = 0x1;

        public const uint DIF_PROPERTYCHANGE = 0x12;

        public const uint DICS_ENABLE = 1;

        public const uint DICS_DISABLE = 2;

        public const uint DICS_FLAG_GLOBAL = 1;

        [Flags]
        public enum DiGetClassFlags : uint
        {
            DIGCF_DEFAULT = 0x00000001,
            DIGCF_PRESENT = 0x00000002,
            DIGCF_ALLCLASSES = 0x00000004,
            DIGCF_PROFILE = 0x00000008,
            DIGCF_DEVICEINTERFACE = 0x00000010,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_CLASSINSTALL_HEADER
        {
            public int cbSize;
            public uint installFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_PROPCHANGE_PARAMS
        {
            public SP_CLASSINSTALL_HEADER classInstallHeader;
            public uint stateChange;
            public uint scope;
            public uint hwProfile;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid classGuid;
            public uint devInst;
            public IntPtr reserved;
        }

        #region Windows API

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string enumerator,
            IntPtr hwndParent,
            uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr deviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(
            IntPtr deviceInfoSet, 
            uint memberIndex, 
            ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceInstanceId(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            IntPtr deviceInstanceIdBuffer,
            int deviceInstanceIdSize,
            out uint requiredSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_DevNode_Status(
            ref int pulStatus, 
            ref int pulProblemNumber, 
            uint dnDevInst, int 
            ulFlags);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            uint property,
            out uint propertyRegDataType,
            IntPtr propertyBuffer,
            uint propertyBufferSize,
            out uint requiredSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiSetClassInstallParams(
            IntPtr deviceInfoSet, 
            ref SP_DEVINFO_DATA deviceInfoData, 
            ref SP_PROPCHANGE_PARAMS classInstallParams, 
            int classInstallParamsSize);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiChangeState(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData);

        #endregion

        #region Helper methods

        public static string GetDeviceInstanceId(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData)
        {
            IntPtr buffer = Marshal.AllocHGlobal(BUFFER_SIZE);
            try
            {
                
                uint requiredSize;
                if (!SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, buffer, BUFFER_SIZE, out requiredSize))
                {
                    return string.Empty;
                }

                return Marshal.PtrToStringAuto(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static IEnumerable<string> GetDeviceRegistryProperty(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            uint property)
        {
            IntPtr buffer = Marshal.AllocHGlobal(BUFFER_SIZE);
            try
            {
                uint propertyDataType;
                uint requiredSize;
                if (!SetupDiGetDeviceRegistryProperty(
                    deviceInfoSet, 
                    ref deviceInfoData, 
                    property, 
                    out propertyDataType, 
                    buffer, 
                    BUFFER_SIZE, 
                    out requiredSize))
                {
                    return null;
                }

                var result = new byte[requiredSize];
                Marshal.Copy(buffer, result, 0, (int)requiredSize);
                var strResult = Encoding.Unicode.GetString(result);

                var values = strResult.Split('\0').Select(v => v.Trim()).Where(v => v.Length > 0);
                return values;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        #endregion
    }
}
