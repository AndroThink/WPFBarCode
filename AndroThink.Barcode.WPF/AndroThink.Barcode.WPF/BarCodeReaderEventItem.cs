namespace AndroThink.Barcode.WPF;

public class BarCodeReaderEventItem
{
    public System.Guid EventId { get; set; }
    public string Source { get; set; } = "";
    public BaseBarCodeEvent ReaderEvent { get; set; }
    public System.Windows.UIElement ReaderSource { get; set; }
}
