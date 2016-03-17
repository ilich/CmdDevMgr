using CommandLine;

namespace CmdDevMgr.Options
{
    [Verb("list")]
    class ListOptions
    {
        [Value(0, HelpText = "Filter devices using a regular expression")]
        public string Search { get; set; }

        [Option('e', "enabled", HelpText = "Show only enabled devices")]
        public bool OnlyEnabled { get; set; }

        [Option('d', "disabled", HelpText = "Show only disabled devices")]
        public bool OnlyDisabled { get; set; }
    }
}
