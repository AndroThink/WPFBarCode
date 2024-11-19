namespace AndroThink.Barcode.WPF;

public class BaseBarCodeReaderResult
{
    public string Barcode { get; set; } = "";
    public object? ReaderSource { get; set; }
    public bool IsPrefixDetectResult { get; set; }
}

public class BarCodeReaderResult : BaseBarCodeReaderResult
{
    public string Source { get; set; } = "";
}
