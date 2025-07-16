using YaraXSharp;
namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var yara = new YaraX(YRX_COMPILE_FLAGS.YRX_ERROR_ON_SLOW_PATTERN, YRX_COMPILE_FLAGS.YRX_DISABLE_INCLUDES);
                yara.AddRuleFile(Path.Combine(Environment.CurrentDirectory, "../../../", "eicar.yar"));
                yara.AddRuleFile(Path.Combine(Environment.CurrentDirectory, "../../../", "eitwo.yar"));
                var rules = yara.Build();

                Console.WriteLine($"Number of rules: {yara.RulesCount()}");

                Scanner scanner = new Scanner(rules, YRX_SCANNER_FLAGS.LOAD_METADATA, YRX_SCANNER_FLAGS.LOAD_PATTERNS);
                scanner.Scan(Path.Combine(Environment.CurrentDirectory, "eicar.txt"));
                List<Rule> results = scanner.Results();
                Console.WriteLine($"Matches: {results.Count}");

                foreach (Rule rule in results)
                {
                    Console.WriteLine($"Pattern match count: {rule.Patterns.Count}");
                    Console.WriteLine(rule.Metadata["malware_family"]);
                }

                // Make sure to destroy.
                scanner.Destroy();
                yara.Destroy();
            } catch (YrxException ex)
            {
                Console.Write(ex.Message);
            }
        }
    }
}
