Framework "4.6"

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$source_dir = "$base_dir\src"
	$result_dir = "$build_dir\results"
	$config = "Debug"
}


task default -depends local
task local -depends compile, test
task ci -depends clean, release, local

task clean {
	rd "$source_dir\artifacts" -recurse -force  -ErrorAction SilentlyContinue | out-null
	rd "$base_dir\build" -recurse -force  -ErrorAction SilentlyContinue | out-null
}

task release {
    $script:config = "Release"
}

task compile -depends clean {
	$version = if ($env:APPVEYOR_BUILD_NUMBER -ne $NULL) { $env:APPVEYOR_BUILD_NUMBER } else { '0' }
	$version = "{0:D5}" -f [convert]::ToInt32($version, 10)
	
    exec { & $source_dir\.nuget\Nuget.exe restore $source_dir\BulkWriter.sln }

    exec { msbuild /t:Clean /t:Build /p:Configuration=$script:config /v:q /p:NoWarn=1591 /nologo $source_dir\BulkWriter.sln }

    exec { & $source_dir\.nuget\Nuget.exe pack $source_dir\BulkWriter\BulkWriter.csproj -Symbols -Properties Configuration=$script:config }
}

task test {
    exec { & $source_dir\packages\xunit.runner.console.2.1.0\tools\xunit.console.exe $source_dir/BulkWriter.Tests/bin/$script:config/BulkWriter.Tests.dll }
}
