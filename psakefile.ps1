include "./psake-build-helpers.ps1"

properties {
	$configuration = 'Release'
	$projectRoot = Resolve-Path "./"
	$buildDir = "$projectRoot/Build"
	$testResultsDir = "$projectRoot/src/BulkWriter.Tests/TestResults"
	$version = "3.0.0-ci-local"
	$owner = 'Headspring'
	$yearInitiated = '2014'
}

task default -depends Run-Tests
task Run-CI -depends Clean-Solution, Run-Tests -description "Continuous Integration process"
task Run-LocalCI -depends Clean-Solution, Run-Tests, Package-Local -description "Local continuous integration process"
task ReBuild-Solution -depends Clean-Solution, Run-Tests -description "Rebuild the code, with testing"

task Get-Info -description "Display runtime information" {
	exec { dotnet --info }
}

task Run-Tests -depends Run-Restore, Build-Solution -description "Run unit tests" {
	exec { dotnet test --configuration $configuration  --logger "trx;LogFileName=Testresults.xml" } -workingDirectory src/BulkWriter.Tests
}

task Build-Solution -depends Get-Info -description "Build the solution" {
	set-project-properties $version
	exec { dotnet build --configuration $configuration --nologo --no-restore} -workingDirectory .
}

task Clean-Solution -description "Clean out all the binary folders" {
	exec { dotnet clean --configuration $configuration /nologo } -workingDirectory $projectRoot
	remove-directory-silently $testResultsDir
	remove-directory-silently $buildDir
}

task Run-Restore -depends Get-Info -description "Restore dependencies and tools" {
	exec { dotnet restore }
}

task Package-Local -description "Build local nuget file" {
	exec { dotnet pack -c Release -o $buildDir --include-symbols --no-build } -workingDirectory $projectRoot/src/BulkWriter
}
