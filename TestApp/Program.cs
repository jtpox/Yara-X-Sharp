using YaraXSharp;
namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("Test 1");
                using (Compiler yara = new Compiler(YRX_COMPILE_FLAGS.YRX_ERROR_ON_SLOW_PATTERN, YRX_COMPILE_FLAGS.YRX_DISABLE_INCLUDES))
                {
                    yara.IgnoreModule("console");
                    yara.BanModule("console", "Console", "Console is banned");

                    yara.AddRuleFile("./eicar.yar");
                    yara.AddRuleFile("./eitwo.yar");
                    var (rules, errors, warnings) = yara.Build();

                    if (errors.Length != 0) _LoopErrorFormat(errors);
                    if (warnings.Length != 0) _LoopErrorFormat(warnings);

                    Console.WriteLine($"Number of rules: {rules.Count()}");

                    using (Scanner scanner = new Scanner(
                        rules,
                        YRX_SCANNER_FLAGS.LOAD_METADATA,
                        YRX_SCANNER_FLAGS.LOAD_PATTERNS,
                        YRX_SCANNER_FLAGS.LOAD_NAMESPACE,
                        YRX_SCANNER_FLAGS.LOAD_IDENTIFIER
                    ))
                    {
                        scanner.SetTimeout(3);
                        scanner.Scan(Path.Combine(Environment.CurrentDirectory, "eicar.txt"));
                        List<Match> results = scanner.Results();
                        Console.WriteLine($"Matches: {results.Count}");

                        foreach (Match rule in results)
                        {
                            Console.WriteLine($"Pattern match count: {rule.Patterns.Count}");
                            Console.WriteLine($"Family: {rule.Metadata["malware_family"]}");
                            Console.WriteLine($"Namespace: {rule.Namespace}");
                            Console.WriteLine($"Identifier: {rule.Identifier}");
                            Console.WriteLine("-");
                        }
                    }
                }

                Console.WriteLine("---");

                Console.WriteLine("Test 2");
                if (File.Exists("./ydb.dat")) File.Delete("./ydb.dat");
                using (Compiler yara = new Compiler(YRX_COMPILE_FLAGS.YRX_ERROR_ON_SLOW_PATTERN, YRX_COMPILE_FLAGS.YRX_DISABLE_INCLUDES))
                {
                    yara.AddRuleFile("./eicar.yar");
                    yara.AddRuleFile("./eitwo.yar");
                    var (rules, errors, warnings) = yara.Build();

                    if (errors.Length != 0) _LoopErrorFormat(errors);
                    if (warnings.Length != 0) _LoopErrorFormat(warnings);

                    var export = rules.Export();
                    using (var fs = new FileStream("./ydb.dat", FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(export, 0, export.Length);
                        Console.WriteLine("Serialized rules exported to ydb.dat");
                    }
                }

                Console.WriteLine("---");

                Console.WriteLine("Test 3");
                using (Rules rules = new Rules())
                {
                    var serializedRules = File.ReadAllBytes("./ydb.dat");
                    rules.Import(serializedRules);
                    Console.WriteLine($"Number of rules: {rules.Count()}");

                    using (Scanner scanner = new Scanner(
                        rules,
                        YRX_SCANNER_FLAGS.LOAD_METADATA,
                        YRX_SCANNER_FLAGS.LOAD_PATTERNS,
                        YRX_SCANNER_FLAGS.LOAD_NAMESPACE,
                        YRX_SCANNER_FLAGS.LOAD_IDENTIFIER
                    ))
                    {
                        scanner.SetTimeout(3);
                        scanner.Scan(Path.Combine(Environment.CurrentDirectory, "eicar.txt"));
                        List<Match> results = scanner.Results();
                        Console.WriteLine($"Matches: {results.Count}");

                        foreach (Match rule in results)
                        {
                            Console.WriteLine($"Pattern match count: {rule.Patterns.Count}");
                            Console.WriteLine($"Family: {rule.Metadata["malware_family"]}");
                            Console.WriteLine($"Namespace: {rule.Namespace}");
                            Console.WriteLine($"Identifier: {rule.Identifier}");
                            Console.WriteLine("-");
                        }
                    }
                }
            } catch (YrxException ex) {
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
