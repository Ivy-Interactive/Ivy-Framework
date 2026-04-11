import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Mic, Square } from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { getWidth, inputStyles } from "@/lib/styles";
import { useEventHandler } from "@/components/event-handler";
import { logger } from "@/lib/logger";
import { Densities } from "@/types/density";
import {
  audioInputVariant,
  textSizeVariant,
  timerSizeVariant,
  iconSizeVariant,
} from "@/components/ui/input/audio-input-variant";
import { uploadFile } from "@/widgets/filePicker/shared";
import { EMPTY_ARRAY } from "@/lib/constants";

interface AudioInputWidgetProps {
  id: string;
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
  invalid?: string;
  autoFocus?: boolean;
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

type AudioState = {
  recording: boolean;
  error: boolean;
  mimeSupportError: boolean;
  recordingStartedAt: number | null;
  recordingStoppedAt: number | null;
  volume: number;
};

type AudioAction =
  | { type: "START_RECORDING" }
  | { type: "STOP_RECORDING" }
  | { type: "SET_ERROR"; mimeSupport?: boolean }
  | { type: "SET_VOLUME"; volume: number }
  | { type: "SET_STARTED_AT"; timestamp: number | null }
  | { type: "SET_STOPPED_AT"; timestamp: number | null }
  | { type: "RESET" };

const initialState: AudioState = {
  recording: false,
  error: false,
  mimeSupportError: false,
  recordingStartedAt: null,
  recordingStoppedAt: null,
  volume: 0,
};

function audioReducer(state: AudioState, action: AudioAction): AudioState {
  switch (action.type) {
    case "START_RECORDING":
      return {
        ...state,
        recording: true,
        error: false,
        mimeSupportError: false,
        recordingStartedAt: null,
        recordingStoppedAt: null,
      };
    case "STOP_RECORDING":
      return { ...state, recording: false };
    case "SET_ERROR":
      return { ...state, error: true, mimeSupportError: !!action.mimeSupport, recording: false };
    case "SET_VOLUME":
      return { ...state, volume: action.volume };
    case "SET_STARTED_AT":
      return { ...state, recordingStartedAt: action.timestamp };
    case "SET_STOPPED_AT":
      return { ...state, recordingStoppedAt: action.timestamp };
    case "RESET":
      return initialState;
    default:
      return state;
  }
}

export const AudioInputWidget: React.FC<AudioInputWidgetProps> = ({
  id,
  label,
  recordingLabel,
  mimeType = "audio/webm",
  disabled = false,
  width,
  uploadUrl,
  chunkInterval = 1000,
  sampleRate,
  density = Densities.Medium,
  events = EMPTY_ARRAY,
  invalid,
  autoFocus,
}) => {
  const eventHandler = useEventHandler();
  const hasAutoFocusedRef = useRef(false);
  const [state, dispatch] = React.useReducer(audioReducer, initialState);
  const { recording, error, mimeSupportError, recordingStartedAt, recordingStoppedAt, volume } =
    state;

  useEffect(() => {
    if (autoFocus && !disabled && !hasAutoFocusedRef.current) {
      hasAutoFocusedRef.current = true;
      dispatch({ type: "START_RECORDING" });
    }
  }, [autoFocus, disabled]);

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

      const selectedMime =
        selectedMimeTypeRef.current ?? normalizedMimeTypes[0] ?? supportedMimeTypes[0];

      try {
        await uploadFile(uploadUrl, chunk, {
          extraFields: { mimeType: selectedMime },
        });
      } catch (error) {
        logger.error("File upload error:", error);
      }
    },
    [uploadUrl, normalizedMimeTypes],
  );

  useEffect(() => {
    if (!recording) {
      return;
    }

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
          dispatch({ type: "SET_ERROR", mimeSupport: true });
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
        dispatch({ type: "SET_STARTED_AT", timestamp: Date.now() });

        const analyser = audioContext.createAnalyser();
        analyser.fftSize = 256;
        source.connect(analyser);
        const dataArray = new Uint8Array(analyser.frequencyBinCount);
        const updateVolume = () => {
          if (cancelled) return;
          analyser.getByteFrequencyData(dataArray);
          const avg = dataArray.reduce((a, b) => a + b, 0) / dataArray.length;
          dispatch({ type: "SET_VOLUME", volume: avg });
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
        dispatch({ type: "SET_ERROR" });
      }
    })();
    return () => {
      cancelled = true;
      selectedMimeTypeRef.current = null;
      onCancel();
      dispatch({ type: "SET_STOPPED_AT", timestamp: Date.now() });
    };
  }, [recording, chunkInterval, sampleRate, uploadChunk, normalizedMimeTypes]);

  const volumePercent = recording ? Math.min(volume / 255, 1) * 100 : 0;

  return (
    <div className="relative" style={{ ...getWidth(width) }}>
      <div
        className={cn(
          audioInputVariant({ density }),
          invalid && inputStyles.invalidInput,
          disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer",
        )}
        onClick={
          disabled
            ? undefined
            : (e) => {
                e.stopPropagation();
                if (recording) {
                  dispatch({ type: "STOP_RECORDING" });
                } else {
                  dispatch({ type: "START_RECORDING" });
                }
              }
        }
        role="button"
        aria-label={recording ? "Stop recording" : "Start recording"}
        tabIndex={disabled ? -1 : 0}
        onBlur={(e) => {
          if (disabled) return;
          if (!e.currentTarget.contains(e.relatedTarget)) {
            if (events?.includes("OnBlur")) eventHandler("OnBlur", id, []);
          }
        }}
        onFocus={(e) => {
          if (disabled) return;
          if (!e.currentTarget.contains(e.relatedTarget)) {
            if (events?.includes("OnFocus")) eventHandler("OnFocus", id, []);
          }
        }}
        onKeyDown={(e) => {
          if (disabled) return;
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            e.stopPropagation();
            if (recording) {
              dispatch({ type: "STOP_RECORDING" });
            } else {
              dispatch({ type: "START_RECORDING" });
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
  const prevStart = useRef(props.start);
  const prevStopped = useRef(props.stopped);

  useEffect(() => {
    if (props.start !== prevStart.current || props.stopped !== prevStopped.current) {
      prevStart.current = props.start;
      prevStopped.current = props.stopped;

      if (typeof props.start !== "number") {
        setSeconds(0);
        return;
      }
      if (typeof props.stopped === "number" && typeof props.start === "number") {
        setSeconds(Math.floor((props.stopped - props.start) / 1000));
        return;
      }
    }

    if (typeof props.start === "number" && typeof props.stopped !== "number") {
      const start = props.start;
      const interval = setInterval(() => {
        setSeconds(Math.floor((Date.now() - start) / 1000));
      }, 100);
      return () => clearInterval(interval);
    }
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
