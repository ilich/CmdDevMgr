using System.Collections.Generic;

namespace CmdDevMgr.Devices
{
    interface IDeviceManager
    {
        bool SetStatus(DeviceInfo device, DeviceStatus status);

        IEnumerable<DeviceInfo> FindDevices(string filter = null, bool? enabled = null);

        DeviceInfo FindDevice(string hwId);
    }
}
