rule VerySlowExample
{
    meta:
        malware_family = "Slow Test"

    strings:
        // Unanchored regex with lots of backtracking potential
        $re1 = /[A-Za-z0-9]{5,1000}/
        
        // Wildcard-heavy hex pattern
        $hex1 = { 6A ?? ?? ?? ?? 68 ?? ?? ?? ?? ?? ?? 6A ?? }
        
        // Large string with wide/ascii modifiers
        $s1 = "This is a very long string that might appear in multiple encodings across the file" wide ascii

    condition:
        // Scans for any of these across the entire file
        any of them
}
