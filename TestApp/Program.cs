using YaraXSharp;
namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var yara = new YaraX();
                yara.AddRuleFile(Path.Combine(Environment.CurrentDirectory, "../../../", "eicar.yar"));
                yara.AddRuleFile(Path.Combine(Environment.CurrentDirectory, "../../../", "eitwo.yar"));

                var (rules, errors, warnings) = yara.Build();

                if (errors.Length != 0) _LoopErrorFormat(errors);
                if (warnings.Length != 0) _LoopErrorFormat(warnings);

                Console.WriteLine($"Number of rules: {yara.RulesCount()}");

                Scanner scanner = new Scanner(rules);
                scanner.Scan(Path.Combine(Environment.CurrentDirectory, "eicar.txt"));
                scanner.Scan(Path.Combine(Environment.CurrentDirectory, "eicar.txt"));
                List<Rule> results = scanner.Results();
                Console.WriteLine($"Matches: {results.Count}");

                foreach (Rule rule in results)
                {
                    Console.WriteLine(rule.Metadata["malware_family"]);
                }

                // Make sure to destroy.
                scanner.Destroy();
                yara.Destroy();
            }
            catch (YrxException ex)
            {
                Console.Write(ex.Message);
            }
        }

        public static void _LoopErrorFormat(YrxErrorFormat[] errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error.text);
            }
        }
    }
}
