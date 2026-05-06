# Scanner

An interface to scan a barcode or QR code using the device's camera. Enables users to capture and process barcode/QR code data within an application.

## Retool

```toolscript
// Clear the current scanned value
scanner.clearValue();

// Pause scanning operation
scanner.pause();

// Resume scanning operation
scanner.resume();

// Configure scanner to capture only one scan
scanner.setSingleScan(true);

// Set time between successive scans (in milliseconds)
scanner.setTimeBetweenScans(1000);

// Enable duplicate scan filtering
scanner.setIgnoreDuplicates(true);
```

## Ivy

```csharp
// Not supported — Ivy does not have a Scanner widget.
```

> **Note:** Ivy does not currently have a barcode/QR code scanner component.

## Parameters

| Parameter              | Documentation                                     | Ivy           |
|------------------------|---------------------------------------------------|---------------|
| `data`                 | The scanned data value                            | Not supported |
| `autoClose`            | Automatically close after input                   | Not supported |
| `ephemeralConfirm`     | Automatically confirm each scan                   | Not supported |
| `ignoreDuplicates`     | Ignore duplicate scans                            | Not supported |
| `launchButtonLabel`    | Button text content                               | Not supported |
| `hidden`               | Hide from view                                    | Not supported |
| `isHiddenOnDesktop`    | Desktop layout visibility                         | Not supported |
| `isHiddenOnMobile`     | Mobile layout visibility                          | Not supported |
| `margin`               | Outer spacing                                     | Not supported |
