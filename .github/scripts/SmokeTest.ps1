[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ExePath,

    [ValidateRange(1, 300)]
    [int]$SecondsToObserve = 10,

    [ValidateRange(1, 120)]
    [int]$ShutdownTimeoutSeconds = 10
)

$ErrorActionPreference = 'Stop'

function Write-ApplicationOutput {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Title,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    Write-Host "::group::$Title"
    if (Test-Path -LiteralPath $Path) {
        $content = Get-Content -LiteralPath $Path -Raw -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($content)) {
            Write-Host "<no output>"
        }
        else {
            Write-Host $content
        }
    }
    else {
        Write-Host "<output file was not created>"
    }
    Write-Host "::endgroup::"
}

function Send-EscapeKey {
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process
    )

    Add-Type -Namespace NativeMethods -Name User32 -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
public static extern bool PostMessage(System.IntPtr hWnd, uint msg, System.IntPtr wParam, System.IntPtr lParam);
[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
public static extern bool SetForegroundWindow(System.IntPtr hWnd);
'@

    $Process.Refresh()
    $windowHandle = $Process.MainWindowHandle
    if ($windowHandle -eq [IntPtr]::Zero) {
        Write-Host 'No main window handle was available; skipping Escape shutdown attempt.'
        return $false
    }

    $wmKeyDown = 0x0100
    $wmKeyUp = 0x0101
    $vkEscape = [IntPtr]0x1B

    try {
        Add-Type -AssemblyName System.Windows.Forms
        [void][NativeMethods.User32]::SetForegroundWindow($windowHandle)
        Start-Sleep -Milliseconds 200
        [System.Windows.Forms.SendKeys]::SendWait('{ESC}')
        Write-Host "Sent Escape via SendKeys to main window handle $windowHandle."
        return $true
    }
    catch {
        Write-Warning "SendKeys Escape attempt failed: $($_.Exception.Message)"
    }

    Write-Host "Sending Escape via PostMessage to main window handle $windowHandle."
    [void][NativeMethods.User32]::PostMessage($windowHandle, $wmKeyDown, $vkEscape, [IntPtr]::Zero)
    Start-Sleep -Milliseconds 100
    [void][NativeMethods.User32]::PostMessage($windowHandle, $wmKeyUp, $vkEscape, [IntPtr]::Zero)

    return $true
}

function Get-ExitCodeText {
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process
    )

    try {
        $Process.Refresh()
        if ($Process.HasExited) {
            $exitCode = $Process.ExitCode
            if ($null -eq $exitCode -or [string]::IsNullOrWhiteSpace("$exitCode")) {
                return '<unknown>'
            }

            return "$exitCode"
        }
    }
    catch {
        return '<unknown>'
    }

    return '<still running>'
}

$resolvedExePath = (Resolve-Path -LiteralPath $ExePath).Path
$workingDirectory = Split-Path -Parent $resolvedExePath
$tempDirectory = if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { [System.IO.Path]::GetTempPath() }
$stdoutPath = Join-Path $tempDirectory 'deep-smoke-stdout.log'
$stderrPath = Join-Path $tempDirectory 'deep-smoke-stderr.log'

Remove-Item -LiteralPath $stdoutPath, $stderrPath -Force -ErrorAction SilentlyContinue

Write-Host "Starting application: $resolvedExePath"
$process = Start-Process `
    -FilePath $resolvedExePath `
    -WorkingDirectory $workingDirectory `
    -RedirectStandardOutput $stdoutPath `
    -RedirectStandardError $stderrPath `
    -PassThru

try {
    $deadline = (Get-Date).AddSeconds($SecondsToObserve)
    while ((Get-Date) -lt $deadline) {
        if ($process.HasExited) {
            Write-ApplicationOutput -Title 'Application stdout' -Path $stdoutPath
            Write-ApplicationOutput -Title 'Application stderr' -Path $stderrPath
            throw "Application exited before $SecondsToObserve seconds elapsed. Exit code: $(Get-ExitCodeText -Process $process)."
        }

        Start-Sleep -Milliseconds 500
        $process.Refresh()
    }

    Write-Host "Application was still running after $SecondsToObserve seconds."

    $escapeSent = Send-EscapeKey -Process $process
    if ($escapeSent -and $process.WaitForExit($ShutdownTimeoutSeconds * 1000)) {
        Write-Host "Application exited after Escape. Exit code: $(Get-ExitCodeText -Process $process)."
        return
    }

    $process.Refresh()
    if (-not $process.HasExited) {
        Write-Host 'Attempting graceful shutdown via CloseMainWindow.'
        [void]$process.CloseMainWindow()
    }

    if ($process.WaitForExit($ShutdownTimeoutSeconds * 1000)) {
        Write-Host "Application exited after CloseMainWindow. Exit code: $(Get-ExitCodeText -Process $process)."
        return
    }

    Write-Warning 'Application did not exit gracefully within the timeout; terminating process.'
    try {
        $process.Kill($true)
    }
    catch [System.Management.Automation.MethodException] {
        $process.Kill()
    }
    $process.WaitForExit()
}
finally {
    Write-ApplicationOutput -Title 'Application stdout' -Path $stdoutPath
    Write-ApplicationOutput -Title 'Application stderr' -Path $stderrPath
}
