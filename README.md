PSAsync
=======
Forked from https://psasync.codeplex.com

Example speed difference:
```Powershell
PS C:\> Measure-Command {Start-Async {Test-Connection localhost -Count 15} | Wait-Async}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 14
Milliseconds      : 377
Ticks             : 143773635
TotalDays         : 0.000166404670138889
TotalHours        : 0.00399371208333333
TotalMinutes      : 0.239622725
TotalSeconds      : 14.3773635
TotalMilliseconds : 14377.3635

PS C:\> Measure-Command {Start-Job {Test-Connection localhost -Count 15} | Wait-Job}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 19
Milliseconds      : 499
Ticks             : 194994021
TotalDays         : 0.000225687524305556
TotalHours        : 0.00541650058333333
TotalMinutes      : 0.324990035
TotalSeconds      : 19.4994021
TotalMilliseconds : 19499.4021

PS C:\> Measure-Command {Test-Connection localhost Count 15}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 14
Milliseconds      : 99
Ticks             : 140995935
TotalDays         : 0.000163189739583333
TotalHours        : 0.00391655375
TotalMinutes      : 0.234993225
TotalSeconds      : 14.0995935
TotalMilliseconds : 14099.5935
```
