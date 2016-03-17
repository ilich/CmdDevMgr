using CommandLine;

namespace CmdDevMgr.Options
{
    [Verb("enable")]
    class EnableOptions
    {
        [Value(0, Required = true, HelpText = "Hardware ID (HwID)")]
        public string HwId { get; set; }
    }
}
