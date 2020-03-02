@echo Executing psake build
powershell -NoProfile -ExecutionPolicy unrestricted -Command "invoke-psake %*; if ($psake.build_success -eq $false) { exit 1 } else { exit 0 }"