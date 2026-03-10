namespace OasisWords.Core.CrossCuttingConcerns.Logging;

public class LogDetail
{
    public string MethodName { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public List<LogParameter> Parameters { get; set; } = new();
}

public class LogParameter
{
    public string Name { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string Type { get; set; } = string.Empty;
}
