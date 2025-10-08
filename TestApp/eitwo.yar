rule malw_eicar2 : malw w32  {
	
	meta:

		description = "Rule to detect the EICAR pattern"
      	author = "Marc Rivero | McAfee ATR Team"
      	reference = "https://www.eicar.org/"
      	rule_version = "v1"
        malware_type = "eicar2"
        malware_family = "W32/Eicar2"
        actor_type = "Unknown"
        actor_group = "Unknown"
		hash = "275a021bbfb6489e54d471899f7db9d1663fc695ec2fe2a2c4538aabf651fd0f"

	strings:

		$s1 = "STANDA" fullword ascii

	condition:
		any of them
}