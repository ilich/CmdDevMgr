using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static CmdDevMgr.Devices.SetupAPI;

namespace CmdDevMgr.Devices
{
    class DeviceManager : IDeviceManager
    {
        public bool SetStatus(DeviceInfo device, DeviceStatus status)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            IntPtr deviceInfoSet = IntPtr.Zero;
            Guid nullGuid = Guid.Empty;

            try
            {
                deviceInfoSet = SetupDiGetClassDevs(
                    ref nullGuid,
                    null,
                    IntPtr.Zero,
                    (uint)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

                if (deviceInfoSet == INVALID_HANDLE_VALUE)
                {
                    throw new Win32Exception("SetupDiGetClassDevs call failed");
                }

                var deviceInfoData = new SP_DEVINFO_DATA();
                deviceInfoData.cbSize = (uint)Marshal.SizeOf(deviceInfoData);

                uint deviceIdx = 0;
                while (SetupDiEnumDeviceInfo(deviceInfoSet, deviceIdx, ref deviceInfoData))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == ERROR_NOT_FOUND)
                    {
                        break;
                    }

                    var instanceId = GetDeviceInstanceId(deviceInfoSet, ref deviceInfoData);
                    if (instanceId == device.InstanceId)
                    {
                        return ChangeDeviceStatus(deviceInfoSet, ref deviceInfoData, status);
                    }
                    
                    deviceIdx++;
                }
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }

            return false;
        }

        public IEnumerable<DeviceInfo> FindDevices(string filter = null, bool? enabled = default(bool?))
        {
            IntPtr deviceInfoSet = IntPtr.Zero;
            Guid nullGuid = Guid.Empty;

            try
            {
                deviceInfoSet = SetupDiGetClassDevs(
                    ref nullGuid, 
                    null, 
                    IntPtr.Zero, 
                    (uint)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

                if (deviceInfoSet == INVALID_HANDLE_VALUE)
                {
                    throw new Win32Exception("SetupDiGetClassDevs call failed");
                }

                var deviceInfoData = new SP_DEVINFO_DATA();
                deviceInfoData.cbSize = (uint)Marshal.SizeOf(deviceInfoData);

                uint deviceIdx = 0;
                while(SetupDiEnumDeviceInfo(deviceInfoSet, deviceIdx, ref deviceInfoData))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error == ERROR_NOT_FOUND)
                    {
                        break;
                    }

                    var device = new DeviceInfo();
                    device.Guid = deviceInfoData.classGuid;
                    device.InstanceId = GetDeviceInstanceId(deviceInfoSet, ref deviceInfoData);

                    device.DeviceDescription = 
                        GetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_DEVICEDESC)?.FirstOrDefault() 
                            ?? string.Empty;

                    device.HardwareIds = GetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SPDRP_HARDWAREID)?.ToList();

                    int status = 0;
                    int problem = 0;
                    if (CM_Get_DevNode_Status(ref status, ref problem, deviceInfoData.devInst, 0) == CR_SUCCESS)
                    {
                        device.Status = problem == CM_PROB_DISABLED ? DeviceStatus.Disabled : DeviceStatus.Enabled;
                    }
                    else
                    {
                        device.Status = null;
                    }

                    if (Filter(device, filter, enabled))
                    {
                        yield return device;
                    }
                    
                    deviceIdx++;
                }
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                }
            }
        }

        public DeviceInfo FindDevice(string hwId)
        {
            var device = FindDevices().FirstOrDefault(
                d => string.Compare(d.InstanceId, hwId, true) == 0
                    || (d.HardwareIds?.Any(dhwid => string.Compare(dhwid, hwId, true) == 0) ?? false));

            return device;
        }

        private bool Filter(DeviceInfo device, string filter, bool? enabled)
        {
            if (enabled == true && device.Status != DeviceStatus.Enabled)
            {
                return false;
            }

            if (enabled == false && device.Status == DeviceStatus.Enabled)
            {
                return false;
            }

            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }

            return Regex.IsMatch(device.InstanceId, filter, RegexOptions.IgnoreCase)
                || Regex.IsMatch(device.DeviceDescription, filter, RegexOptions.IgnoreCase)
                || (device.HardwareIds?.Any(id => Regex.IsMatch(id, filter, RegexOptions.IgnoreCase)) ?? false);
        }

        private bool ChangeDeviceStatus(IntPtr deviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, DeviceStatus newStatus)
        {
            var header = new SP_CLASSINSTALL_HEADER();
            header.cbSize = Marshal.SizeOf(header);
            header.installFunction = DIF_PROPERTYCHANGE;

            var settings = new SP_PROPCHANGE_PARAMS();
            settings.classInstallHeader = header;
            settings.stateChange = newStatus == DeviceStatus.Enabled ? DICS_ENABLE : DICS_DISABLE;
            settings.scope = DICS_FLAG_GLOBAL;
            settings.hwProfile = 0;

            SetupDiSetClassInstallParams(deviceInfoSet, ref deviceInfoData, ref settings, Marshal.SizeOf(settings));
            int error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                throw new Win32Exception(error, "SetupDiSetClassInstallParams call failed");
            }

            SetupDiChangeState(deviceInfoSet, ref deviceInfoData);
            error = Marshal.GetLastWin32Error();
            if (error != 0)
            {
                throw new Win32Exception(error, "SetupDiChangeState call failed");
            }

            return true;
        }
    }
}
