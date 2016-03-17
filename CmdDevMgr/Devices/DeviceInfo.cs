using System;
using System.Collections.Generic;
using System.Text;

namespace CmdDevMgr.Devices
{
    class DeviceInfo
    {
        public DeviceInfo()
        {
            HardwareIds = new List<string>();
        }

        public Guid Guid { get; set; }

        public DeviceStatus Status { get; set; }

        public string DeviceDescription { get; set; }

        public string InstanceId { get; set; }

        public List<string> HardwareIds { get; set; }

        public override string ToString()
        {
            var text = new StringBuilder();
            text.AppendLine(InstanceId);
            text.AppendLine($"    Device Description: {DeviceDescription}");
            text.AppendLine($"    Status: {Status}");

            text.AppendLine("    Hardware IDs:");
            HardwareIds?.ForEach(id => text.AppendLine($"        {id}"));

            return text.ToString();
        }
    }
}
