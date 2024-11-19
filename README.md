## WPFBarCode [![Nuget](https://img.shields.io/nuget/v/AndroThink.Identity.PermissionsAuther)](https://www.nuget.org/packages/AndroThink.Identity.PermissionsAuther)
Help in dealing with the barcode readers in your wpf application

![](https://raw.githubusercontent.com/AndroThink/PermissionsAuther/main/AndroThink.Identity.PermissionsAuther/Images/andro_think.png)

## How to use 

 ### In Your Control
```c#
var BarCodeReader = BarCoder.BarCodeReader.GetDefaultReader()
                            .WithPrefix(System.Windows.Input.Key.F12) // Configuration for the reader device for the prefix key
                            .WithSuffix(System.Windows.Input.Key.Enter)  // Configuration for the reader device for the suffix key
                            // To listen on specific input element
                            .WithUIElement(this, false, (result) =>
                            {
                                string code = result.Barcode;
                            })
                            // To notify the control using an event
                            .WithEvent(viewModel.DocumentId, viewModel.DocumentId.ToString(), new BarCoder.BaseBarCodeEvent((result) =>
                            {
                                string code = result.Barcode;
                                viewModel.OnBarcodeCallBack?.Invoke(code);
                            })).NotifyOnlyActiveEvent() // if you want to notify the ui using only the event you register
                            .SetCurrentActiveEvent(viewModel.DocumentId); // determin the current active window/page by setting a unique identifier for it this id will be with the event result

BarCodeReader.StartListen(); // start listen
....
BarCodeReader.StopListen(); // stop listen

```
