# Bounding Box

An interface to display a bounding box and tag areas of an image. Supports annotation modes for bounding boxes and landmarks, allowing users to draw rectangular regions on an image and assign labels to them.

## Retool

```toolscript
{{boundingBox1.boundingBoxes}}
// Returns an array of objects with x, y, width, height, and label properties

// Configure via Inspector:
// - imageUrl: URL of the image to annotate
// - labels: ["Label 1", "Label 2", "Label 3"]
// - mode: "boundingbox" | "landmarking"

// Scroll into view programmatically
boundingBox1.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

```csharp
// Ivy has an Image widget but no bounding box annotation support.
// The Image widget can display images but cannot annotate or tag regions.
new Image("https://example.com/image.jpg");
```

## Parameters

| Parameter              | Documentation                                                              | Ivy           |
|------------------------|----------------------------------------------------------------------------|---------------|
| boundingBoxes          | A list of bounding box regions (array of {x, y, width, height, label})     | Not supported |
| imageUrl               | The image to display                                                       | `Src`         |
| imageAltText           | An accessible description for screen readers                               | Not supported |
| labels                 | A list of labels for each item                                             | Not supported |
| mode                   | Annotation mode: `boundingbox`, `landmarking`, `stretch`, or `contain`     | `Scale`       |
| hidden                 | Whether this object is hidden from view                                    | `Visible`     |
| isHiddenOnDesktop      | Whether to hide in the desktop layout                                      | Not supported |
| isHiddenOnMobile       | Whether to hide in the mobile layout                                       | Not supported |
| maintainSpaceWhenHidden| Whether to take up space on the canvas if hidden                           | Not supported |
| margin                 | The amount of margin to render outside                                     | Not supported |
| style                  | Custom style options                                                       | Not supported |
| showInEditor           | Whether the component remains visible in the editor if hidden              | Not supported |
| scrollIntoView()       | Scrolls the canvas so that the component appears in the visible area       | Not supported |
| Change event           | Triggered when the value is changed                                        | Not supported |
