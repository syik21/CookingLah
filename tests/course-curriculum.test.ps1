Add-Type -AssemblyName System.Web
Add-Type -AssemblyName System.Data
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $PSScriptRoot '../bin/CookingLah.dll'))
$type = $assembly.GetType('CookingLah.Pages.Learner.CourseLearning.CourseLearningPage')
$page = [Activator]::CreateInstance($type)
$flags = [Reflection.BindingFlags]'Instance,NonPublic'
function Set-PageProperty($name, $value) {
    $type.GetProperty($name, $flags).SetValue($page, $value, $null)
}
function Assert-Equal($actual, $expected) {
    if ($actual -ne $expected) { throw "Expected '$expected', got '$actual'" }
}
$lessons = New-Object System.Data.DataTable
[void]$lessons.Columns.Add('LessonID', [int])
[void]$lessons.Columns.Add('LessonOrder', [int])
[void]$lessons.Rows.Add(1, 1)
[void]$lessons.Rows.Add(2, 2)
Set-PageProperty 'CourseLessons' $lessons
Set-PageProperty 'SelectedLessonId' 2
Set-PageProperty 'LastCompletedLessonOrder' 2
$css = $type.GetMethod('LessonItemClass', $flags)
$icon = $type.GetMethod('LessonStateIcon', $flags)
foreach ($section in @('Quiz', 'Activity', 'Lesson')) {
    Set-PageProperty 'CurrentSection' $section
    $expected = if ($section -eq 'Lesson') { 'curriculum-item is-active' } else { 'curriculum-item is-completed' }
    Assert-Equal ($css.Invoke($page, @([object]2))) $expected
    Assert-Equal ($icon.Invoke($page, @([object]2))) ([string][char]0x2713)
}
Set-PageProperty 'LastCompletedLessonOrder' 1
Set-PageProperty 'CurrentSection' 'Quiz'
Assert-Equal ($css.Invoke($page, @([object]2))) 'curriculum-item'
Assert-Equal ($icon.Invoke($page, @([object]2))) ([string][char]0x25CB)
Set-PageProperty 'CurrentSection' 'Lesson'
Assert-Equal ($icon.Invoke($page, @([object]2))) ([string][char]0x25CF)
Write-Output 'Course curriculum selection checks passed.'
