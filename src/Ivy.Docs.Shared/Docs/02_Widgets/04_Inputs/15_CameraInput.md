---
searchHints:
  - camera
  - webcam
  - photo
  - capture
  - snapshot
  - picture
  - selfie
---

# CameraInput

<Ingress>
Capture photos directly from the user's webcam or device camera with a live video preview, one-click capture, and automatic upload.
</Ingress>

The `CameraInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a camera capture interface. It requests camera access, displays a live video preview, and lets users take a snapshot that is automatically uploaded using the same `UseUpload` + `MemoryStreamUploadHandler` pattern as [FileInput](./10_FileInput.md).

## Basic Usage

```csharp demo-below
public class BasicCameraInputDemo : ViewBase
{
    public override object? Build()
    {
        var photo = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

        return Layout.Vertical()
            | new CameraInput(upload.Value, "Take a photo")
            | (photo.Value != null
                ? Text.P($"Captured: {photo.Value.FileName} ({StringHelper.FormatBytes(photo.Value.Length)})")
                : Text.P("No photo captured yet."));
    }
}
```

## How It Works

1. The user clicks the widget to start the camera (browser prompts for permission)
2. A live video preview is displayed from the device camera
3. Clicking **Capture** takes a snapshot and uploads it as a PNG image
4. The captured image is shown with a **Retake** button to restart the camera

## Configuration

### Placeholder

Set the text shown before the camera is started:

```csharp
new CameraInput(upload.Value).Placeholder("Click to take a photo")
```

### FacingMode

Control which camera to use on devices with multiple cameras:

```csharp
// Front-facing camera (default)
new CameraInput(upload.Value).FacingMode("user")

// Rear-facing camera
new CameraInput(upload.Value).FacingMode("environment")
```

### Disabled

Prevent camera activation:

```csharp demo-below
public class DisabledCameraInputDemo : ViewBase
{
    public override object? Build()
    {
        var photo = UseState<FileUpload<byte[]>?>();
        var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

        return new CameraInput(upload.Value, "Camera disabled", disabled: true);
    }
}
```

## Upload Handling

CameraInput uses the same upload system as FileInput. The captured photo is uploaded as `image/png` via `POST` with `multipart/form-data`:

```csharp
var photo = UseState<FileUpload<byte[]>?>();
var upload = UseUpload(MemoryStreamUploadHandler.Create(photo));

// Access captured photo data
if (photo.Value != null)
{
    var fileName = photo.Value.FileName;    // "capture.png"
    var fileSize = photo.Value.Length;       // Size in bytes
    var fileData = photo.Value.Content;      // byte[] containing PNG data
}
```

<WidgetDocs Type="Ivy.CameraInput" ExtensionTypes="Ivy.CameraInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/CameraInput.cs"/>
