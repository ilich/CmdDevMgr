using System;
using System.Linq;
using CmdDevMgr.Devices;
using CmdDevMgr.Options;
using CommandLine;
using static System.Console;

namespace CmdDevMgr
{
    class Program
    {
        static readonly DeviceManager _deviceManager = new DeviceManager();

        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListOptions, EnableOptions, DisableOptions, StatusOptions>(args)
                .MapResult(
                    (ListOptions opts) => ListDevices(opts),
                    (EnableOptions opts) => 0,
                    (DisableOptions opts) => 0,
                    (StatusOptions opts) => ShowDeviceStatus(opts),
                    errs => 1);
        }

        static int ListDevices(ListOptions options)
        {
            bool? enabled = null;
            if (options.OnlyEnabled)
            {
                enabled = true;
            }
            else if (options.OnlyDisabled)
            {
                enabled = false;
            }

            try
            {
                var devices = _deviceManager.FindDevices(options.Search, enabled);
                foreach (var device in devices)
                {
                    Write(device);
                }

                WriteLine($"{devices.Count()} matching device(s) found.");
                return 0;
            }
            catch(Exception err)
            {
                WriteLine($"Error: {err.Message}");
                return 1;
            }
        }

        static int ShowDeviceStatus(StatusOptions options)
        {
            var device = _deviceManager.FindDevice(options.HwId);
            if (device == null)
            {
                WriteLine("Error: device is not found");
                return 1;
            }

            WriteLine(device);
            return 0;
        }
    }
}
