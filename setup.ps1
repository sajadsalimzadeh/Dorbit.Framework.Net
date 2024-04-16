$ExePath = Get-ChildItem -Name *.exe
Start-Process $ExePath -Args cli -verb runas
Exit