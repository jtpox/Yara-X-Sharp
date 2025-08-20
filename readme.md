# Yara-X Sharp
A simple wrapper for Yara-X around the Yara-X C/C++ API.

You can find the Nuget package [here](https://www.nuget.org/packages/YaraXSharp).

## Requirements
As of 1.0.2, `Windows x86_64` and `Linux x86_64` version of Yara X CAPI is included in the Nuget package.

If you are on Apple silicon or want to bring your own `yara_x_capi.dll`, you can follow the compile guide [here](https://github.com/jtpox/Yara-X-Sharp/wiki/Bring-your-own-Yara%E2%80%90X-C-C---API).

## Usage
```csharp
/*
 * New Compiler instance.
 * You can pass multiple params from YRX_COMPILE_FLAGS.
 * E.g. new Compiler(YRX_COMPILE_FLAGS.YRX_ERROR_ON_SLOW_PATTERN)
 */
var yara = new Compiler();
yara.AddRuleFile("./eicar.yar");
var (rules, errors, warnings) = yara.Build(); // Compiled rules to be used in Scanner.

Scanner scanner = new Scanner(rules, YRX_SCANNER_FLAGS.LOAD_METADATA, YRX_SCANNER_FLAGS.LOAD_PATTERNS);
scanner.scan("./eicar.txt");
List<Match> results = scanner.Results();

foreach (Match rule in results) {
  Console.WriteLine($"Pattern match count: {rule.Patterns.Count}");
  Console.WriteLine(rule.Metadata["malware_family"]);
}

// Make sure to destroy.
scanner.Destroy();
yara.Destroy();
```

Or 

```csharp
using (var yara = new Compiler())
{
  yara.AddRuleFile(Path.Combine(Environment.CurrentDirectory, "../../../", "eicar.yar"));
  yara.AddRuleFile(Path.Combine(Environment.CurrentDirectory, "../../../", "eitwo.yar"));
  var (rules, errors, warnings) = yara.Build();

  Console.WriteLine($"Number of rules: {rules.Count()}");

  using (Scanner scanner = new Scanner(rules, YRX_SCANNER_FLAGS.LOAD_METADATA, YRX_SCANNER_FLAGS.LOAD_PATTERNS))
  {
      scanner.Scan(Path.Combine(Environment.CurrentDirectory, "eicar.txt"));
      List<Match> results = scanner.Results();
      Console.WriteLine($"Matches: {results.Count}");

      foreach (Match rule in results)
      {
          Console.WriteLine($"Pattern match count: {rule.Patterns.Count}");
          Console.WriteLine(rule.Metadata["malware_family"]);
      }
  }
}
```

## Reference
- [Yara-X C/C++ API Documentation](https://virustotal.github.io/yara-x/docs/api/c/c-/)

## Compatibility
| Yara-X Release Version | Wrapper Version |
|--|--|
| [1.4.0](https://github.com/VirusTotal/yara-x/releases/tag/v1.4.0) | 0.0.1, 0.0.2, 0.0.3, 0.0.4, 0.0.5, 0.1.0, [1.0.0](https://github.com/jtpox/Yara-X-Sharp/pull/4) |
| [1.5.0](https://github.com/VirusTotal/yara-x/releases/tag/v1.5.0) | 1.0.3

