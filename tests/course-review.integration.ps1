# Run from the repository root after building. Requires LocalDB, IIS Express and Python.
# The fixture uses a new database and temporary site, removed at the end.
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$testId = [Guid]::NewGuid().ToString('N')
$databaseName = 'CookingLah_ReviewCourseTest_' + $testId
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('CookingLahReviewCourseTest_' + $testId)
$connection = New-Object System.Data.SqlClient.SqlConnection('Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True')
$server = $null
function Invoke-TestSql([string]$sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $command.CommandTimeout = 60
    try { [void]$command.ExecuteNonQuery() } finally { $command.Dispose() }
}
try {
    $connection.Open()
    Invoke-TestSql "CREATE DATABASE [$databaseName]"
    $connection.ChangeDatabase($databaseName)
    Invoke-TestSql (Get-Content (Join-Path $projectRoot 'App_Data/InsertTable.sql') -Raw)
    Invoke-TestSql "IF COL_LENGTH('Courses','RejectionReason') IS NULL ALTER TABLE Courses ADD RejectionReason NVARCHAR(500) NULL"
    Invoke-TestSql (Get-Content (Join-Path $PSScriptRoot "course-review.seed.sql") -Raw)
    [void](New-Item -ItemType Directory -Path $testRoot)
    foreach ($folder in @('bin','Pages','F_CSS','F_JavaScripts')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot $folder) -Destination $testRoot -Recurse
    }
    [xml]$config = Get-Content (Join-Path $projectRoot 'Web.config')
    $config.configuration.connectionStrings.add.connectionString = "Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=$databaseName;Integrated Security=True"
    $config.Save((Join-Path $testRoot 'Web.config'))
    # This session fixture exists only in the disposable site.
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'course-review.fixture.aspx.txt') -Destination (Join-Path $testRoot 'Fixture.aspx')
    $listener = New-Object System.Net.Sockets.TcpListener([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $port = $listener.LocalEndpoint.Port
    $listener.Stop()
    $arguments = @('/path:"' + $testRoot + '"', '/port:' + $port, '/systray:false')
    $server = Start-Process -FilePath 'C:/Program Files/IIS Express/iisexpress.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $testRoot 'iis.log') -RedirectStandardError (Join-Path $testRoot 'iis-error.log')
    & node (Join-Path $PSScriptRoot 'course-review.browser.cjs') "http://localhost:$port"
    if ($LASTEXITCODE -ne 0) { throw 'Course review browser checks failed.' }
    Write-Output 'Course review integration checks passed.'
} finally {
    if ($server -and !$server.HasExited) {
        Stop-Process -Id $server.Id -Force
        $server.WaitForExit()
    }
    # ASP.NET's compiler server can outlive IIS and hold the copied assemblies open.
    Get-CimInstance Win32_Process -Filter "Name = 'VBCSCompiler.exe'" | Where-Object {
        $_.ExecutablePath -and $_.ExecutablePath.StartsWith($testRoot + '\', [StringComparison]::OrdinalIgnoreCase)
    } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force }
    if ($connection.State -eq 'Open') {
        $connection.ChangeDatabase('master')
        Invoke-TestSql "IF DB_ID('$databaseName') IS NOT NULL BEGIN ALTER DATABASE [$databaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$databaseName]; END"
        $connection.Close()
    }
    $connection.Dispose()
    $expectedRoot = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('CookingLahReviewCourseTest_' + $testId)))
    if ((Test-Path -LiteralPath $testRoot) -and ([IO.Path]::GetFullPath($testRoot) -eq $expectedRoot)) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}

