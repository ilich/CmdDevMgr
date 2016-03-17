using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static CmdDevMgr.Devices.SetupAPI;

namespace CmdDevMgr.Devices
{
    class DeviceManager : IDeviceManager
    {
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
                    throw new InvalidOperationException("SetupDiGetClassDevs call failed.");
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
                        device.Status = DeviceStatus.Error;
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

        public DeviceStatus GetStatus(string hwId)
        {
            throw new NotImplementedException();
        }

        public bool SetStatus(string hwId, DeviceStatus status)
        {
            throw new NotImplementedException();
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
    }
}
