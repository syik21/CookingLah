# Run from the repository root after building. Requires LocalDB, IIS Express and Python.
# The fixture uses a new database and temporary site, removed at the end.
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$testId = [Guid]::NewGuid().ToString('N')
$databaseName = 'CookingLah_CategoriesTest_' + $testId
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('CookingLahCategoriesTest_' + $testId)
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
    Invoke-TestSql "INSERT INTO Users (Username,Email,PasswordHash,FullName,Role) VALUES ('admintest','admin@example.invalid','test-only','Test Admin','Admin'),('cheftest','chef@example.invalid','test-only','Test Chef','Instructor'); INSERT INTO Categories (CategoryName,Description) VALUES ('Baking','Bread and pastry'),('Grilling','Outdoor cooking'); INSERT INTO Courses (InstructorID,CategoryID,Title,DifficultyLevel) VALUES (2,1,'Bread basics','Beginner');"

    [void](New-Item -ItemType Directory -Path $testRoot)
    foreach ($folder in @('bin','Pages','F_CSS','F_JavaScripts')) {
        Copy-Item -LiteralPath (Join-Path $projectRoot $folder) -Destination $testRoot -Recurse
    }
    [xml]$config = Get-Content (Join-Path $projectRoot 'Web.config')
    $config.configuration.connectionStrings.add.connectionString = "Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=$databaseName;Integrated Security=True"
    $config.Save((Join-Path $testRoot 'Web.config'))
    # This session fixture exists only in the disposable site.
    $fixture = @'
<%@ Page Language="C#" %>
<script runat="server">
protected void Page_Load(object sender, EventArgs e) {
    if (Request.QueryString["link"] == "1") {
        using (var connection = CookingLah.F_Services.DatabaseManager.ConnectDB())
        using (var command = new System.Data.SqlClient.SqlCommand("INSERT INTO Courses (InstructorID,CategoryID,Title,DifficultyLevel) SELECT 2,CategoryID,'Grilling basics','Beginner' FROM Categories WHERE CategoryName='Grilling'", connection)) {
            connection.Open();
            command.ExecuteNonQuery();
        }
        Response.Write("linked");
        return;
    }
    Session.Clear();
    Session["UserId"] = Request.QueryString["other"] == "1" ? 2 : 1;
    Session["Role"] = Request.QueryString["other"] == "1" ? "Instructor" : "Admin";
    Session["Username"] = "Test Chef";
    Response.Write("fixture ready");
}
</script>
'@
    [IO.File]::WriteAllText((Join-Path $testRoot 'Fixture.aspx'), $fixture)
    $listener = New-Object System.Net.Sockets.TcpListener([Net.IPAddress]::Loopback, 0)
    $listener.Start()
    $port = $listener.LocalEndpoint.Port
    $listener.Stop()
    $arguments = @('/path:"' + $testRoot + '"', '/port:' + $port, '/systray:false')
    $server = Start-Process -FilePath 'C:/Program Files/IIS Express/iisexpress.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $testRoot 'iis.log') -RedirectStandardError (Join-Path $testRoot 'iis-error.log')
    & python (Join-Path $PSScriptRoot 'categories.integration.py') "http://localhost:$port"
    if ($LASTEXITCODE -ne 0) { throw 'Category management integration checks failed.' }
    Write-Output 'Category management integration checks passed.'
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
    $expectedRoot = [IO.Path]::GetFullPath((Join-Path ([IO.Path]::GetTempPath()) ('CookingLahCategoriesTest_' + $testId)))
    if ((Test-Path -LiteralPath $testRoot) -and ([IO.Path]::GetFullPath($testRoot) -eq $expectedRoot)) {
        Remove-Item -LiteralPath $testRoot -Recurse -Force
    }
}
