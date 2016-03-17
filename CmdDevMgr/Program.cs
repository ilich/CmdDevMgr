using System.Linq;
using CmdDevMgr.Devices;
using CmdDevMgr.Options;
using CommandLine;
using static System.Console;

namespace CmdDevMgr
{
    class Program
    {
        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ListOptions, EnableOptions, DisableOptions, StatusOptions>(args)
                .MapResult(
                    (ListOptions opts) => ListDevices(opts),
                    (EnableOptions opts) => 0,
                    (DisableOptions opts) => 0,
                    (StatusOptions opts) => 0,
                    errs => 1);
        }

        static int ListDevices(ListOptions options)
        {
            var manager = new DeviceManager();

            bool? enabled = null;
            if (options.OnlyEnabled)
            {
                enabled = true;
            }
            else if (options.OnlyDisabled)
            {
                enabled = false;
            }

            var devices = manager.FindDevices(options.Search, enabled);
            foreach(var device in devices)
            {
                Write(device);
            }

            WriteLine($"{devices.Count()} matching device(s) found.");

            return 0;
        }
    }
}
