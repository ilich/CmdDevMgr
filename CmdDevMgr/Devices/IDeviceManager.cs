using System.Collections.Generic;

namespace CmdDevMgr.Devices
{
    interface IDeviceManager
    {
        bool SetStatus(string hwId, DeviceStatus status);

        IEnumerable<DeviceInfo> FindDevices(string filter = null, bool? enabled = null);

        DeviceInfo FindDevice(string hwId);
    }
}
