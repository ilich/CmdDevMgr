using CommandLine;

namespace CmdDevMgr.Options
{
    [Verb("status")]
    class StatusOptions
    {
        [Value(0, Required = true, HelpText = "Hardware ID (HwID)")]
        public string HwId { get; set; }
    }
}
