# set PSGallery as trusted so we can install packages from there
Write-Host 'Trusting PS Gallery'
Set-PSRepository -Name "PSGallery" -InstallationPolicy Trusted
 
# Install PSAKE
Write-Host 'Installing PSake'
Install-Module -Name psake -Scope CurrentUser

# Install dotnet based tools (requires a manifest)
Write-Host 'Install dotnet tools'
dotnet tool restore