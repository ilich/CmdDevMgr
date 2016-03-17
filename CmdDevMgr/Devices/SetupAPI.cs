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

        private const string SETUPAPI = "setupapi.dll";

        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int ERROR_NOT_FOUND = 1168;

        public const int CR_SUCCESS = 0;

        public const int CM_PROB_DISABLED = 0x16;

        public const int SPDRP_DEVICEDESC = 0x0;

        public const int SPDRP_HARDWAREID = 0x1;

        [Flags]
        public enum DiGetClassFlags : uint
        {
            DIGCF_DEFAULT = 0x00000001,  // only valid with DIGCF_DEVICEINTERFACE
            DIGCF_PRESENT = 0x00000002,
            DIGCF_ALLCLASSES = 0x00000004,
            DIGCF_PROFILE = 0x00000008,
            DIGCF_DEVICEINTERFACE = 0x00000010,
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

        [DllImport(SETUPAPI, CharSet = CharSet.Auto)]
        public static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string enumerator,
            IntPtr hwndParent,
            uint flags);

        [DllImport(SETUPAPI, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr deviceInfoSet);

        [DllImport(SETUPAPI, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(
            IntPtr deviceInfoSet, 
            uint memberIndex, 
            ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport(SETUPAPI, SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetupDiGetDeviceInstanceId(
            IntPtr deviceInfoSet,
            ref SP_DEVINFO_DATA deviceInfoData,
            IntPtr deviceInstanceIdBuffer,
            int deviceInstanceIdSize,
            out uint requiredSize);

        [DllImport(SETUPAPI, SetLastError = true)]
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

        #endregion

        #region Helpers

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
