namespace AndroThink.Barcode.WPF;

public class BaseBarCodeEvent
{

    public System.Action<BarCodeReaderResult> OnResult { get; private set; }

    public BaseBarCodeEvent(System.Action<BarCodeReaderResult> onResult)
    {
        OnResult = onResult;
    }
}

public class BarCodeReaderEvent : BaseBarCodeEvent
{
    public BarCodeReaderEvent(System.Action<BarCodeReaderResult> onResult) : base(onResult) { }
}

public class BarCodeReaderPrefixDetectEvent
{
    public System.Action<string> OnResult { get; private set; }

    public BarCodeReaderPrefixDetectEvent(System.Action<string> onResult)
    {
        OnResult = onResult;
    }
}
