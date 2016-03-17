using CommandLine;

namespace CmdDevMgr.Options
{
    [Verb("disable")]
    class DisableOptions
    {
        [Value(0, Required = true, HelpText = "Hardware ID (HwID)")]
        public string HwId { get; set; }
    }
}
