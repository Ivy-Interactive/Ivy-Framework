$cs = Get-CimInstance Win32_ComputerSystem
$cs | Set-CimInstance -Property @{AutomaticManagedPagefile=$true}
$result = (Get-CimInstance Win32_ComputerSystem).AutomaticManagedPagefile
"AutomaticManagedPagefile=$result" | Out-File "D:\Tendril\pagefile_result.txt"
