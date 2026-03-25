import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Mic, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getWidth } from "@/lib/styles";
import { logger } from "@/lib/logger";
import { Densities } from "@/types/density";
import {
  audioInputVariant,
  textSizeVariant,
  timerSizeVariant,
  iconSizeVariant,
} from "@/components/ui/input/audio-input-variant";

interface AudioInputWidgetProps {
  label?: string;
  recordingLabel?: string;
  mimeType: string;
  disabled: boolean;
  events: string[];
  width?: string;
  uploadUrl: string;
  chunkInterval: number;
  sampleRate?: number | null;
  density?: Densities;
}

const supportedMimeTypes = [
  "audio/webm", // Chromium/Firefox
  "audio/mp4", // Safari/iOS
  "audio/ogg", // Older Firefox/desktop
  "audio/aac", // Safari/iOS
  "audio/webm;codecs=opus",
  "audio/ogg;codecs=opus",
  "audio/wav", // uncompressed fallback (always supported, large files)
];

export const AudioInputWidget: React.FC<AudioInputWidgetProps> = ({
  label,
  recordingLabel,
  mimeType = "audio/webm",
  disabled = false,
  width,
  uploadUrl,
  chunkInterval = 1000,
  sampleRate,
  density = Densities.Medium,
}) => {
  const normalizedMimeTypes = useMemo(() => {
    const candidates: string[] = [];
    const addCandidate = (value?: string) => {
      const trimmed = value?.trim();
      if (trimmed && !candidates.includes(trimmed)) {
        candidates.push(trimmed);
      }
    };

    addCandidate(mimeType);
    supportedMimeTypes.forEach(addCandidate);

    return candidates;
  }, [mimeType]);

  const selectedMimeTypeRef = useRef<string | null>(null);

  const uploadChunk = useCallback(
    async (chunk: Blob): Promise<void> => {
      if (!uploadUrl) return;

      // Get the correct host from meta tag or use relative URL
      const getUploadUrl = () => {
        const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
        if (ivyHostMeta) {
          const host = ivyHostMeta.getAttribute("content");
          return host + uploadUrl;
        }
        // If no meta tag, use relative URL (should work in production)
        return uploadUrl;
      };

      const selectedMime =
        selectedMimeTypeRef.current ?? normalizedMimeTypes[0] ?? supportedMimeTypes[0];

      const formData = new FormData();
      formData.append("file", chunk);
      formData.append("mimeType", selectedMime);

      try {
        const response = await fetch(getUploadUrl(), {
          method: "POST",
          body: formData,
        });

        if (!response.ok) {
          throw new Error(`Upload failed: ${response.statusText}`);
        }
      } catch (error) {
        logger.error("File upload error:", error);
      }
    },
    [uploadUrl, normalizedMimeTypes],
  );

  const [recording, setRecording] = useState(false);
  const [error, setError] = useState(false);
  const [mimeSupportError, setMimeSupportError] = useState(false);

  const [recordingStartedAt, setRecordingStartedAt] = useState<number | null>(null);
  const [recordingStoppedAt, setRecordingStoppedAt] = useState<number | null>(null);

  const [volume, setVolume] = useState(0);

  useEffect(() => {
    if (!recording) {
      return;
    }
    setRecordingStartedAt(null);
    setRecordingStoppedAt(null);
    setMimeSupportError(false);

    let cancelled = false;
    let onCancel = () => {};
    (async () => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({
          audio: true,
        });
        if (cancelled) {
          return;
        }

        const mediaRecorderAvailable = typeof MediaRecorder !== "undefined";
        const canProbeTypeSupport =
          mediaRecorderAvailable && typeof MediaRecorder.isTypeSupported === "function";

        const supportChecks: Array<{ type: string; supported: boolean }> = [];
        let supportedMimeType: string | null = null;

        if (!canProbeTypeSupport) {
          supportedMimeType = normalizedMimeTypes[0] ?? null;
        } else {
          for (const type of normalizedMimeTypes) {
            const supported = MediaRecorder.isTypeSupported(type);
            supportChecks.push({ type, supported });
            if (supported) {
              supportedMimeType = type;
              break;
            }
          }
        }

        if (!supportedMimeType) {
          logger.error("No supported MIME type found for AudioInput", {
            requestedTypes: normalizedMimeTypes,
            checks: supportChecks,
            mediaRecorderAvailable,
            canProbeTypeSupport,
          });
          setError(true);
          setMimeSupportError(true);
          setRecording(false);
          return;
        }

        selectedMimeTypeRef.current = supportedMimeType;

        const audioContext =
          sampleRate != null ? new AudioContext({ sampleRate }) : new AudioContext();
        if (audioContext.state === "suspended") {
          await audioContext.resume();
        }
        if (cancelled) return;
        const source = audioContext.createMediaStreamSource(stream);

        let streamToRecord: MediaStream;
        if (sampleRate != null) {
          const destination = audioContext.createMediaStreamDestination();
          source.connect(destination);
          streamToRecord = destination.stream;
          const micRate = stream.getAudioTracks()[0]?.getSettings?.()?.sampleRate;
          logger.warn(
            `AudioInput: requested ${sampleRate} Hz, mic ${micRate ?? "?"} Hz - recording at ${audioContext.sampleRate} Hz (resampled)`,
          );
        } else {
          streamToRecord = stream;
          const micRate = stream.getAudioTracks()[0]?.getSettings?.()?.sampleRate;
          logger.warn(
            `AudioInput: no sample rate set, recording at ${micRate ?? audioContext.sampleRate} Hz (mic default)`,
          );
        }

        const mediaRecorder = new MediaRecorder(streamToRecord, {
          mimeType: supportedMimeType,
        });

        mediaRecorder.ondataavailable = async (event) => {
          if (event.data.size > 0) {
            await uploadChunk(event.data);
          }
        };

        mediaRecorder.start(chunkInterval);
        setRecordingStartedAt(Date.now());

        const analyser = audioContext.createAnalyser();
        analyser.fftSize = 256;
        source.connect(analyser);
        const dataArray = new Uint8Array(analyser.frequencyBinCount);
        const updateVolume = () => {
          if (cancelled) return;
          analyser.getByteFrequencyData(dataArray);
          const avg = dataArray.reduce((a, b) => a + b, 0) / dataArray.length;
          setVolume(avg); // Range: 0 - 255
          requestAnimationFrame(updateVolume);
        };
        updateVolume();

        onCancel = () => {
          mediaRecorder.stop();
          stream.getTracks().forEach((track) => track.stop());
          audioContext.close();
        };
      } catch (err) {
        logger.error("Error accessing microphone:", err);
        setError(true);
        setRecording(false);
      }
    })();
    return () => {
      cancelled = true;
      selectedMimeTypeRef.current = null;
      onCancel();
      setRecordingStoppedAt(Date.now());
    };
  }, [recording, chunkInterval, sampleRate, uploadChunk, normalizedMimeTypes]);

  const volumePercent = recording ? Math.min(volume / 255, 1) * 100 : 0;

  return (
    <div className="relative" style={{ ...getWidth(width) }}>
      <div
        className={cn(
          audioInputVariant({ density }),
          disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer",
        )}
        onClick={
          disabled
            ? undefined
            : (e) => {
                e.stopPropagation();
                if (recording) {
                  setRecording(false);
                } else {
                  setRecording(true);
                  setError(false);
                }
              }
        }
        role="button"
        tabIndex={disabled ? -1 : 0}
        onKeyDown={(e) => {
          if (disabled) return;
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            e.stopPropagation();
            if (recording) {
              setRecording(false);
            } else {
              setRecording(true);
              setError(false);
            }
          }
        }}
      >
        <div
          className="absolute bottom-0 left-0 w-full transition-all duration-100 ease-linear"
          style={{
            backgroundColor: "rgba(255,0,0,0.075)",
            height: `${volumePercent}%`,
            pointerEvents: "none",
            transition: "height 50ms",
          }}
        />
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className={"mt-2 h-6 w-fit z-10 mx-auto block"}
        >
          {recording ? (
            <Square className={iconSizeVariant({ density })} />
          ) : (
            <Mic className={iconSizeVariant({ density })} />
          )}
        </Button>
        <SecondsCounter start={recordingStartedAt} stopped={recordingStoppedAt} density={density} />
        {(label || recordingLabel) && (
          <p className={cn("text-center mt-1 text-muted-foreground", textSizeVariant({ density }))}>
            {recording ? recordingLabel : label}
          </p>
        )}
        {error && (
          <p className={cn("text-muted-foreground text-center", textSizeVariant({ density }))}>
            {mimeSupportError
              ? "Recording format not supported in this browser."
              : "Failed to record. Check your settings."}
          </p>
        )}
      </div>
    </div>
  );
};

function SecondsCounter(props: {
  start: number | null;
  stopped: number | null;
  density?: Densities;
}) {
  const [seconds, setSeconds] = useState(0);
  useEffect(() => {
    if (typeof props.start !== "number") {
      queueMicrotask(() => setSeconds(0));
      return;
    }
    if (typeof props.stopped === "number" && typeof props.start === "number") {
      const stopped = props.stopped;
      const start = props.start;
      queueMicrotask(() => {
        setSeconds(Math.floor((stopped - start) / 1000));
      });
      return;
    }
    const start = props.start;
    const interval = setInterval(() => {
      setSeconds(Math.floor((Date.now() - start) / 1000));
    }, 100);
    return () => {
      clearInterval(interval);
    };
  }, [props.start, props.stopped]);
  return (
    <p className={cn("text-center", timerSizeVariant({ density: props.density }))}>
      {Math.floor(seconds / 60)
        .toString()
        .padStart(2, "0")}
      :{(seconds % 60).toString().padStart(2, "0")}
    </p>
  );
}

export default AudioInputWidget;
