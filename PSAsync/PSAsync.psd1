@{
    ModuleToProcess = 'PSAsync.psm1'
    NestedModules   = 'PSAsync.dll'
    ModuleVersion = '0.1'
    GUID = '1aad797e-694b-4635-9d29-cfe13fb48c66'
    Author = 'GoodOlClint'
    CompanyName = 'GoodOlClint'
    AliasesToExport = '*'
    CmdletsToExport = @(
	'Get-Async'
	'Start-Async'
	'Stop-Async'
	'Wait-Async'
	'Receive-Async'
	'Remove-Async'
	'Get-RunspacePool'
	'New-RunspacePool'
	)
} 