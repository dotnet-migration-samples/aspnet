[string[]]$folders = '.\eShopOnWeb\eShopLegacyMVC',
                     '.\MvcMusicStore\MvcMusicStore',
                     '.\Orchard\src\Orchard.Web',
                     '.\PartsUnlimited\src\PartsUnlimitedWebsite'
[string]$migrationContentOutFile = '.\migration-content-files.md'

function GetContentFilesToMigrate{
    [cmdletbinding()]
    param(
        [string]$folder = (pwd)
    )
    process{
        [string]$oldloc = Get-Location
        Set-Location -Path $folder
        $filter = 'App_Data','App_Start','bin','obj','global.asax','*.aspx','*.ascx','*.ashx','*.master','*.cshtml',
                  '*.vbhtml','*.sql','*.??proj','*.user','*.cs','*.resx','web.config','web.*.config',
                  'packages.config','ApplicationInsights.config','app.config'
        Get-ChildItem -Path .\ *.* -Exclude $filter|%{ 
            if(!$_.PSIsContainer){$_} 
            else{Get-ChildItem -Path $_.FullName -Recurse -File -Exclude $filter}}|
        Select-Object -ExpandProperty FullName | Resolve-Path -Relative
        Set-Location -LiteralPath $oldloc
    }
}

function CreateContentFilesReport{
    [cmdletbinding()]
    param(
        [string]$reportPath = $migrationContentOutFile,
        [string[]]$foldersToAnalyze = $folders
    )
    process{
        if(-not ($foldersToAnalyze)){
            'folders to analyze is empty' | Write-Error
            return
        }
        # if the file exists delete it before starting
        if(test-path $reportPath){
            Remove-Item $reportPath
        }
        $foldersToAnalyze |% {
            "## {0}`n" -f (Split-Path $_ -Leaf) | Out-File -FilePath $reportPath -Append
            '```' | Out-File -FilePath $reportPath -Append
            GetContentFilesToMigrate -folder $_ | Out-File -FilePath $reportPath -Append
            "`````` `n" | Out-File -FilePath $reportPath -Append
        }
    }
}

CreateContentFilesReport -reportPath $migrationContentOutFile -foldersToAnalyze $folders