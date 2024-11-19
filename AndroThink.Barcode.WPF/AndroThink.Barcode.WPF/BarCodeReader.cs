using System.Linq;

namespace AndroThink.Barcode.WPF;

public class BarCodeReader
{
    private bool _detectPrefix;
    private bool _withCallBackOnly = false;
    private System.Guid _currentActiveEvent;
    private bool _notifyOnlyActiveEvent = false;

    private BarCodeControl? _activeUIElement = null;
    private BarCodeReaderPrefixDetectEvent? _barCodeReaderPrefixDetectEvent;

    private System.Windows.Input.Key? _prefix = System.Windows.Input.Key.F12;
    private System.Windows.Input.Key _suffix = System.Windows.Input.Key.Enter;

    private System.Collections.Generic.List<System.Windows.Input.Key> _code = new();
    private System.Collections.Generic.List<BarCodeReaderEventItem> _events = new();
    private System.Collections.Generic.List<BarCodeControl> _barCodeControls = new();

    public delegate void OnBarCoderReadCallBack(BarCodeReaderResult readerResult);

    public BarCodeReader() { }

    public static BarCodeReader GetDefaultReader() => new BarCodeReader();

    public bool IsUIElementHasRegistered(System.Windows.UIElement uIElement) => _barCodeControls.FirstOrDefault(e => e.UIElement == uIElement) != null;

    public BarCodeReader WithUIElement(System.Windows.UIElement uIElement, bool alwaysNotify, OnBarCoderReadCallBack coderReadCallBack)
    {
        if (_barCodeControls.Any(e => e.UIElement == uIElement))
        {
            _barCodeControls.Remove(_barCodeControls.First(e => e.UIElement == uIElement));
            //throw new System.Exception("Element alredy registered !");
        }

        var newControl = new BarCodeControl() { UIElement = uIElement, AlwaysNotify = alwaysNotify, OnBarCoderReadCall = coderReadCallBack };
        _barCodeControls.Add(newControl);

        if (alwaysNotify)
            _activeUIElement = newControl;

        return this;
    }

    public BarCodeReader WithEvent(System.Guid eventId, string source, BaseBarCodeEvent codeEvent)
    {
        if (_events.Any(e => e.EventId == eventId))
            throw new System.Exception($"There is a barcode event registered with id {eventId} !");

        _events.Add(new BarCodeReaderEventItem() { EventId = eventId, Source = source, ReaderEvent = codeEvent });

        if (_events.Count == 1)
            _currentActiveEvent = eventId;

        return this;
    }

    public BarCodeReader WithPrefix(System.Windows.Input.Key prefix)
    {
        _prefix = prefix;
        return this;
    }

    public BarCodeReader WithSuffix(System.Windows.Input.Key suffix)
    {
        _suffix = suffix;
        return this;
    }

    public BarCodeReader NotifyOnlyActiveEvent(bool notifyOnlyActiveEvent = true)
    {
        _notifyOnlyActiveEvent = notifyOnlyActiveEvent;
        return this;
    }

    public BarCodeReader NotifyCallBackOnly(bool withCallBackOnly = true)
    {
        _withCallBackOnly = withCallBackOnly;
        return this;
    }

    public BarCodeReader SetCurrentActiveEvent(System.Guid eventId)
    {
        _notifyOnlyActiveEvent = true;
        _currentActiveEvent = eventId;

        return this;
    }

    public void SetCurrentActiveUIElement(System.Windows.UIElement uiElement)
    {
        _activeUIElement = _barCodeControls.FirstOrDefault(i => i.UIElement == uiElement);
    }

    public void RemoveAllEvents() => _events.Clear();

    public void RemoveEvent(System.Guid eventId)
    {
        if (_events.Any(e => e.EventId == eventId))
            _events.Remove(_events.First(e => e.EventId == eventId));
    }

    public void RemoveElement(System.Windows.UIElement element)
    {
        var control = _barCodeControls.FirstOrDefault(e => e.UIElement == element);

        if (control is not null)
        {
            control.UIElement.PreviewKeyDown -= new System.Windows.Input.KeyEventHandler(PreviewKeyDownEventBarcodeScanner);
            control.UIElement.PreviewKeyUp -= new System.Windows.Input.KeyEventHandler(PreviewKeyUpEventBarcodeScanner);

            _barCodeControls.Remove(control);
        }
    }

    public void DetextPrefix(BarCodeReaderPrefixDetectEvent barCodeReaderPrefixDetectEvent = null)
    {
        _prefix = null;
        _detectPrefix = true;

        _barCodeReaderPrefixDetectEvent = barCodeReaderPrefixDetectEvent;

        StartListen();
    }

    public void StartListen()
    {
        if (_barCodeControls is not null && _barCodeControls.Any())
        {
            foreach (var control in _barCodeControls)
            {
                control.UIElement.PreviewKeyDown += new System.Windows.Input.KeyEventHandler(PreviewKeyDownEventBarcodeScanner);
                control.UIElement.PreviewKeyUp += new System.Windows.Input.KeyEventHandler(PreviewKeyUpEventBarcodeScanner);
            }
        }
    }

    public void StopListen()
    {
        if (_barCodeControls is not null && _barCodeControls.Any())
        {
            foreach (var control in _barCodeControls)
            {
                control.UIElement.PreviewKeyDown -= new System.Windows.Input.KeyEventHandler(PreviewKeyDownEventBarcodeScanner);
                control.UIElement.PreviewKeyUp -= new System.Windows.Input.KeyEventHandler(PreviewKeyUpEventBarcodeScanner);
            }
        }
        _code.Clear();
    }

    private void PreviewKeyDownEventBarcodeScanner(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_detectPrefix)
        {
            e.Handled = true;

            if (_code.Count == 0)
            {
                _prefix = e.Key;
            }

            _code.Add(e.Key);
        }
        else
        {
            if (_prefix is not null && (e.Key == _prefix || _code.FirstOrDefault() == _prefix))
            {
                e.Handled = true;
                _code.Add(e.Key);
            }
        }
    }

    private void PreviewKeyUpEventBarcodeScanner(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == _suffix)
        {
            if (_detectPrefix)
            {
                //_detectPrefix = false;
                _prefix = _code.FirstOrDefault();

                if (!_withCallBackOnly && _barCodeReaderPrefixDetectEvent is not null)
                {
                    _barCodeReaderPrefixDetectEvent.OnResult.Invoke(ParseKey(_prefix.Value));
                }

                foreach (var control in _barCodeControls)
                {
                    if (control == e.Source || control == e.OriginalSource || control.AlwaysNotify)
                    {
                        control.OnBarCoderReadCall.Invoke(new BarCodeReaderResult() { IsPrefixDetectResult = true, Barcode = ParseKey(_prefix.Value), ReaderSource = control.UIElement });
                    }
                }
            }
            else
            {
                if (_code is not null && _prefix is not null && _code.FirstOrDefault() == _prefix && _code.LastOrDefault() == _suffix && _code.Count > 2)
                {
                    e.Handled = true;

                    _code.Remove(_prefix.Value);
                    _code.Remove(_suffix);

                    string code = string.Join("", _code.Select(ParseKey).ToArray());

                    NotifyCode(code, e);
                }
            }

            if (_code is not null)
                _code.Clear();
        }

        if (_code is not null && _code.Count > 0 && _code.FirstOrDefault() != _prefix)
            _code.Clear();
    }

    private string ParseKey(System.Windows.Input.Key key)
    {
        switch (key)
        {
            case System.Windows.Input.Key.D0:
            case System.Windows.Input.Key.NumPad0: return "0";
            case System.Windows.Input.Key.D1:
            case System.Windows.Input.Key.NumPad1: return "1";
            case System.Windows.Input.Key.D2:
            case System.Windows.Input.Key.NumPad2: return "2";
            case System.Windows.Input.Key.D3:
            case System.Windows.Input.Key.NumPad3: return "3";
            case System.Windows.Input.Key.D4:
            case System.Windows.Input.Key.NumPad4: return "4";
            case System.Windows.Input.Key.D5:
            case System.Windows.Input.Key.NumPad5: return "5";
            case System.Windows.Input.Key.D6:
            case System.Windows.Input.Key.NumPad6: return "6";
            case System.Windows.Input.Key.D7:
            case System.Windows.Input.Key.NumPad7: return "7";
            case System.Windows.Input.Key.D8:
            case System.Windows.Input.Key.NumPad8: return "8";
            case System.Windows.Input.Key.D9:
            case System.Windows.Input.Key.NumPad9: return "9";
            default: return key.ToString();
        }
    }

    private void NotifyCode(string code, System.Windows.Input.KeyEventArgs e)
    {
        bool isOwnerElementNotified = false;
        //UIElement? ownerElement = null;

        foreach (var control in _barCodeControls)
        {
            if (control.UIElement == e.Source || control.UIElement == e.OriginalSource)
            {
                //ownerElement = control.UIElement;
                isOwnerElementNotified = true;
                control.OnBarCoderReadCall.Invoke(new BarCodeReaderResult() { Barcode = code, ReaderSource = control.UIElement });
                break;
            }
        }

        if (!isOwnerElementNotified)
        {
            var allElementsToNotify = _barCodeControls.Where(c => c.AlwaysNotify);
            if (allElementsToNotify is not null)
            {
                if (_activeUIElement is not null && allElementsToNotify.Any(x => x.UIElement == _activeUIElement.UIElement))
                {
                    _activeUIElement.OnBarCoderReadCall.Invoke(new BarCodeReaderResult() { Barcode = code, ReaderSource = _activeUIElement.UIElement });
                }
                else
                {
                    foreach (var control in allElementsToNotify)
                    {
                        control.OnBarCoderReadCall.Invoke(new BarCodeReaderResult() { Barcode = code, ReaderSource = control.UIElement });
                    }
                }
            }
        }

        if (_events is not null && !_withCallBackOnly)
        {
            System.Windows.UIElement? readerSource = null;

            if (e.OriginalSource is System.Windows.UIElement uiElement && uiElement is not null)
                readerSource = uiElement;

            if (_notifyOnlyActiveEvent)
            {
                BarCodeReaderEventItem? codeEvent = _events.FirstOrDefault(e => e.EventId == _currentActiveEvent);
                if (!_withCallBackOnly && codeEvent is not null)
                {
                    codeEvent.ReaderEvent.OnResult.Invoke(new BarCodeReaderResult() { Source = codeEvent.Source, Barcode = code, ReaderSource = readerSource });
                }
            }
            else
            {
                foreach (var codeEvent in _events)
                {
                    codeEvent.ReaderEvent.OnResult.Invoke(new BarCodeReaderResult() { Source = codeEvent.Source, Barcode = code, ReaderSource = readerSource });
                }
            }
        }
    }
}
