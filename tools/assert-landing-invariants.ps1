#!/usr/bin/env pwsh
<#
.SYNOPSIS
  Fail if EN/RU landing pages lose Writing/Тексты nav or project-blurb links (regression guard).

.DESCRIPTION
  Run from repo root (KarataevDmitry.github.io). Used locally and in GitHub Actions.
  Exit code 0 = all required substrings present; non-zero = missing pattern (message to stderr).
#>
$ErrorActionPreference = 'Stop'
$repoRoot = if ($PSScriptRoot) { (Resolve-Path (Join-Path $PSScriptRoot '..')).Path } else { Get-Location }

function Assert-Contains {
    param(
        [string] $RelativePath,
        [string] $Needle,
        [string] $Description
    )
    $full = Join-Path $repoRoot $RelativePath
    if (-not (Test-Path -LiteralPath $full)) {
        Write-Error "Missing file: $RelativePath"
        exit 2
    }
    $text = Get-Content -LiteralPath $full -Raw -Encoding UTF8
    if ($text.IndexOf($Needle, [StringComparison]::Ordinal) -lt 0) {
        Write-Host "FAIL: $RelativePath — $Description" -ForegroundColor Red
        Write-Host "  Expected substring: $Needle" -ForegroundColor Yellow
        exit 1
    }
    Write-Host "OK: $RelativePath — $Description"
}

Assert-Contains 'docs/index.html' '<a href="/writing/">Writing</a>' 'EN nav: Writing → /writing/'
Assert-Contains 'docs/index.html' '<a href="/writing/why-these-projects-parity.html">Why these projects?</a>' 'EN projects blurb: parity article link'

Assert-Contains 'docs/ru/index.html' '<a href="/ru/writing/">Тексты</a>' 'RU nav: Тексты → /ru/writing/'
Assert-Contains 'docs/ru/index.html' '<a href="/ru/writing/why-these-projects-parity.html">Зачем эти проекты?</a>' 'RU projects blurb: parity article link'

Write-Host 'All landing invariants satisfied.' -ForegroundColor Green
exit 0
