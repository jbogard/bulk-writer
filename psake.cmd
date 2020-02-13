@echo Executing psake build
rem Helper script for those who want to run psake from cmd.exe

powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted; Install-Module -Force -Name psake -Scope CurrentUser; invoke-psake %*; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"