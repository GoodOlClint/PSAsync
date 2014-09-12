PSAsync
=======
Forked from https://psasync.codeplex.com

Example speed difference:

```Powershell
PS C:\> Measure-Command {1..15 | %{Start-Async {Start-Sleep -Seconds 1}} | Wait-Async}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 5
Milliseconds      : 279
Ticks             : 52791771
TotalDays         : 6.11015868055556E-05
TotalHours        : 0.00146643808333333
TotalMinutes      : 0.087986285
TotalSeconds      : 5.2791771
TotalMilliseconds : 5279.1771

PS C:\> Measure-Command {1..15 | %{Start-Job {Start-Sleep -Seconds 1}} | Wait-Job}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 30
Milliseconds      : 615
Ticks             : 306150242
TotalDays         : 0.00035434055787037
TotalHours        : 0.00850417338888889
TotalMinutes      : 0.510250403333333
TotalSeconds      : 30.6150242
TotalMilliseconds : 30615.0242

PS C:\> Measure-Command {1..15 | %{Start-Sleep -Seconds 1}}
Days              : 0
Hours             : 0
Minutes           : 0
Seconds           : 15
Milliseconds      : 202
Ticks             : 152028544
TotalDays         : 0.000175958962962963
TotalHours        : 0.00422301511111111
TotalMinutes      : 0.253380906666667
TotalSeconds      : 15.2028544
TotalMilliseconds : 15202.8544

```
