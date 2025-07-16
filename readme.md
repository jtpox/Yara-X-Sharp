# Yara-X Sharp
A simple wrapper for Yara-X around the Yara-X C/C++ API.

You can find the Nuget package [here](https://www.nuget.org/packages/YaraXSharp).

## Requirements
For versions 0.0.3 and below, bring your own `yara_x_capi.dll` which you can find [here](https://github.com/VirusTotal/yara-x/releases).

## Usage
```csharp
try {
  /*
   * New Compiler instance.
   * You can pass multiple params from YRX_COMPILE_FLAGS.
   * E.g. new YaraX(YRX_COMPILE_FLAGS.YRX_ERROR_ON_SLOW_PATTERN)
   */
  var yara = new YaraX();
  yara.AddRuleFile("./eicar.yar");
  var rules = yara.Build(); // Compiled rules to be used in Scanner.

  Scanner scanner = new Scanner(rules, YRX_SCANNER_FLAGS.LOAD_METADATA);
  scanner.scan("./eicar.txt");
  List<Rule> results = scanner.Results();

  foreach (Rule rule in results) {
    Console.WriteLine($"Pattern match count: {rule.Patterns.Count}");
    Console.WriteLine(rule.Metadata["malware_family"]);
  }

  // Make sure to destroy.
  scanner.Destroy();
  yara.Destroy();
} catch (YrxException ex) {
  Console.WriteLine(ex.Message);
}
```

## Reference
- [Yara-X C/C++ API Documentation](https://virustotal.github.io/yara-x/docs/api/c/c-/)

## To-Dos
- ~~Compiler flags~~
- Compiler error and warnings
- Scanner timeout
- ~~Iterate matched rule patterns and tags~~
- ~~File streaming for scanning large files~~ [BYO](https://github.com/jtpox/Yara-X-Sharp/commit/596f3b0e6da6989e2936eb0bff213742737865be)

## Compatibility
| Yara-X Release Version | Wrapper Version |
|--|--|
| [1.4.0](https://github.com/VirusTotal/yara-x/releases/tag/v1.4.0) | 0.0.1, 0.0.2, 0.0.3, 0.0.4, 0.0.5 |

