$content = Get-Content backend\Program.cs -Raw
$pattern = '(?s)public class ModuleGroupingOperationFilter.*?\}'
$content = [System.Text.RegularExpressions.Regex]::Replace($content, $pattern, $code)
Set-Content backend\Program.cs $content
