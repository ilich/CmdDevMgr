using System.Collections.Generic;

namespace CmdDevMgr.Devices
{
    interface IDeviceManager
    {
        DeviceStatus GetStatus(string hwId);

        bool SetStatus(string hwId, DeviceStatus status);

        IEnumerable<DeviceInfo> FindDevices(string filter = null, bool? enabled = null);
    }
}
