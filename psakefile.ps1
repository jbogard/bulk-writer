include "./psake-build-helpers.ps1"

try {
	$tag = $(git tag -l --points-at HEAD)
	$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:BUILD_BUILDID, 10); $false = "local" }[$env:Build.BuildId -ne $NULL];
	$suffix = @{ $true = ""; $false = "ci-$revision"}[$tag -ne $NULL -and $revision -ne "local"]
}

catch {
	Write-Output "Install git for windows or run this script using git bash."
}

properties {
    $configuration = 'Release'
    $projectRoot = Resolve-Path "./"
	$buildDir = "$projectRoot/Build"
    $testResultsDir = "$projectRoot/src/BulkWriter.Tests/TestResults"
	$versionPrefix = "2.1.0"
}

task default -depends Run-Tests
task Run-CI -depends Clean-Solution, Run-Tests, Package-Local -description "Local continuous integration process"
task ReBuild-Solution -depends Clean-Solution, Run-Tests -description "Rebuild the code, with testing"

task Get-Info -description "Display runtime information" {
    exec { dotnet --info }
}

task Run-Tests -depends Run-Restore, Build-Solution -description "Run unit tests" {
    Push-Location -Path .\src\BulkWriter.Tests
    exec { & dotnet test --configuration $configuration  --logger "trx;LogFileName=Testresults.xml" }
}
  
task Build-Solution -depends Get-Info -description "Build the solution" {
	if($env:BUILD_BUILDID -ne $NULL)
	{
		set-project-properties $versionPrefix
	}
	exec { dotnet build BulkWriter.sln -c $configuration -v q /nologo }
}
  
task Clean-Solution -description "Clean out all the binary folders" {
    exec { dotnet clean --configuration $configuration /nologo } $projectRoot
    remove-directory-silently $testResultsDir
}

task Run-Restore -depends Get-Info -description "Restore dependencies and tools" {
	exec { dotnet restore }
}

task Package-Local -description "Build local nuget file" {
	Pop-Location
	if ($suffix -ne "") {
		exec { & dotnet pack .\src\BulkWriter\BulkWriter.csproj -c Release -o $buildDir --include-symbols --no-build }
	} 
}
