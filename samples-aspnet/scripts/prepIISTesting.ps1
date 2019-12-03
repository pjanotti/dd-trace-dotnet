﻿# For now, just create the web apps for aspnet samples. Later, may want to use parameters to generalize.
#
# param (
#    [string]$name,
#    [string]$path
# )

$siteName = 'Default Web Site'

function createIISWebApps
{
    New-WebApplication -Site $siteName -Name Samples.AspNetMvc4 -PhysicalPath C:\src\DataDog\dd-trace-dotnet\samples-aspnet\Samples.AspNetMvc4 -Force -ErrorAction Stop
    New-WebApplication -Site $siteName -Name Samples.AspNetMvc5 -PhysicalPath C:\src\DataDog\dd-trace-dotnet\samples-aspnet\Samples.AspNetMvc5 -Force -ErrorAction Stop
}

function installDotnetTracer
{
    # TODO
    #  1. Get full path name of msi file
    Start-Process "msiexec.exe" -Wait -ArgumentList '/I datadog-dotnet-apm-1.8.0-x64.msi /quiet'
}


# Main entry point of script

createIISWebApps

installDotnetTracer


