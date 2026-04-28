# Timer

A button to record elapsed time. Tracks duration with configurable update intervals and display options, providing read-only elapsed milliseconds and running status.

## Retool

```toolscript
// Reset timer
timer.setValue(0);

// Access elapsed time in milliseconds
console.log(timer.elapsedTimeMs);

// Check if timer is currently running
if (timer.isRunning) {
  console.log("Timer is active");
}
```

## Ivy

```csharp
// Not supported — Ivy does not have a Timer widget.
```

> **Note:** Ivy does not currently have a timer/stopwatch component. Timer-like behavior could be implemented with custom state and server-side logic.

## Parameters

| Parameter                | Documentation                                  | Ivy           |
|--------------------------|------------------------------------------------|---------------|
| `elapsedTimeMs`          | Time elapsed in milliseconds (read-only)       | Not supported |
| `isRunning`              | Whether timer is currently active (read-only)  | Not supported |
| `timeElapsedSince`       | Duration since last run (read-only)            | Not supported |
| `interval`               | Update frequency in milliseconds               | Not supported |
| `timeElapsedSinceEnabled`| Display elapsed time option                    | Not supported |
| `hidden`                 | Hide component from view                       | Not supported |
| `format`                 | Styling: decimal, percent, currency            | Not supported |
| `margin`                 | External spacing                               | Not supported |
