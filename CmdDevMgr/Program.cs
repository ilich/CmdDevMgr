using System;
using System.ComponentModel;
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
                    (EnableOptions opts) => ChangeDeviceStatus(opts.HwId, true),
                    (DisableOptions opts) => ChangeDeviceStatus(opts.HwId, false),
                    (StatusOptions opts) => ShowDeviceStatus(opts),
                    errs => 3);
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
            catch (Win32Exception err)
            {
                WriteLine($"Error: {err.Message}. Error Code: {err.NativeErrorCode}");
                return 2;
            }
            catch (Exception err)
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

            Write(device);
            return 0;
        }

        static int ChangeDeviceStatus(string hwId, bool enable)
        {
            var device = _deviceManager.FindDevice(hwId);
            if (device == null)
            {
                WriteLine("Error: device is not found");
                return 1;
            }

            try
            {
                var status = enable ? DeviceStatus.Enabled : DeviceStatus.Disabled;
                var isChanged = _deviceManager.SetStatus(device, status);

                if (!isChanged)
                {
                    WriteLine($"Error: cannot change {device.DeviceDescription} status.");
                    return 2;
                }

                WriteLine($"{device.DeviceDescription} has been {status.ToString().ToLower()}.");
                return 0;
            }
            catch(Win32Exception err)
            {
                WriteLine($"Error: {err.Message}. Error Code: {err.NativeErrorCode}");
                return 2;
            }
            catch(Exception err)
            {
                WriteLine($"Error: {err.Message}");
                return 2;
            }
            
        }
    }
}
