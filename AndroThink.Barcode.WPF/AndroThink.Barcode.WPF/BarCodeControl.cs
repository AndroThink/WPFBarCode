using System.Windows;

namespace AndroThink.Barcode.WPF;

public class BarCodeControl
{
    public bool AlwaysNotify { get; set; }
    public UIElement UIElement { get; set; }
    public BarCodeReader.OnBarCoderReadCallBack OnBarCoderReadCall { get; set; }
}
