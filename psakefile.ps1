include "./psake-build-helpers.ps1"

try {
	#Add try catch to handle git not recognised exception
	$tag = $(git tag -l --points-at HEAD)
	$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:Build.BuildId, 10); $false = "local" }[$env:Build.BuildId -ne $NULL];
	$suffix = @{ $true = ""; $false = "ci-$revision"}[$tag -ne $NULL -and $revision -ne "local"]
	$commitHash = $(git rev-parse --short HEAD)
	$buildSuffix = @{ $true = "$($suffix)-$($commitHash)"; $false = "$($branch)-$($commitHash)" }[$suffix -ne ""]
}

catch {
	Write-Output "Install git for windows or run this script using git bash."
}

properties {
    $configuration = 'Release'
    $projectRootDirectory = "C:/MyDev/bulk-writer"
    $testResults = "$projectRootDirectory/TestResults"
	$src = "../../BukWriter"
}
 
task Restore -depends Info -description "Restore dependencies and tools"{
exec { dotnet restore }
}
task default -depends Test
#task CI -depends Clean, Test -description "Continuous Integration process"
task Rebuild -depends Clean, Build -description "Rebuild the code and database, no testing"

task Info -description "Display runtime information" {
    exec { dotnet --info }
}

task Test -depends Restore, Build -description "Run unit tests" {
   Push-Location -Path .\src\BulkWriter.Tests
   exec { & dotnet test --configuration $configuration }
}
  
task Build -depends Info -description "Build the solution" {
	exec { dotnet build BulkWriter.sln -c $configuration --version-suffix=$buildSuffix -v q /nologo }
}
  
task Clean -description "Clean out all the binary folders" {
    exec { dotnet clean --configuration $configuration /nologo } $src
    remove-directory-silently $testResults
}