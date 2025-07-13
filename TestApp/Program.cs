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

                // Error and warning checks have to come before Build.
                var errors = yara.Errors();
                if (errors.Length > 0)
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error.text);
                    }
                }

                var rules = yara.Build();

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
    }
}
