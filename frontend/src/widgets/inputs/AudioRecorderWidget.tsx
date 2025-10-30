import React, { useCallback, useEffect, useState } from 'react';
import { Mic, Square } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { logger } from '@/lib/logger';
import { Sizes } from '@/types/sizes';
import {
  audioRecorderVariants,
  textSizeVariants,
  timerSizeVariants,
  iconSizeVariants,
} from '@/components/ui/input/audio-recorder-variants';

interface AudioRecorderWidgetProps {
  label?: string;
  recordingLabel?: string;
  mimeType: string;
  disabled: boolean;
  events: string[];
  width?: string;
  uploadUrl: string;
  chunkInterval: number;
  size?: Sizes;
}

export const AudioRecorderWidget: React.FC<AudioRecorderWidgetProps> = ({
  label,
  recordingLabel,
  mimeType,
  disabled,
  width,
  uploadUrl,
  chunkInterval,
  size = Sizes.Medium,
}) => {
  const uploadChunk = useCallback(
    async (chunk: Blob): Promise<void> => {
      if (!uploadUrl) return;

      // Get the correct host from meta tag or use relative URL
      const getUploadUrl = () => {
        const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
        if (ivyHostMeta) {
          const host = ivyHostMeta.getAttribute('content');
          return host + uploadUrl;
        }
        // If no meta tag, use relative URL (should work in production)
        return uploadUrl;
      };

      const formData = new FormData();
      formData.append('file', chunk);

      try {
        const response = await fetch(getUploadUrl(), {
          method: 'POST',
          body: formData,
        });

        if (!response.ok) {
          throw new Error(`Upload failed: ${response.statusText}`);
        }
      } catch (error) {
        logger.error('File upload error:', error);
      }
    },
    [uploadUrl]
  );

  const [recording, setRecording] = useState(false);
  const [error, setError] = useState(false);

  const [recordingStartedAt, setRecordingStartedAt] = useState<number | null>(
    null
  );
  const [recordingStoppedAt, setRecordingStoppedAt] = useState<number | null>(
    null
  );

  const [volume, setVolume] = useState(0);

  useEffect(() => {
    if (!recording) {
      return;
    }
    setRecordingStartedAt(null);
    setRecordingStoppedAt(null);

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
        const mediaRecorder = new MediaRecorder(stream, {
          mimeType,
        });

        mediaRecorder.ondataavailable = async event => {
          if (event.data.size > 0) {
            await uploadChunk(event.data);
          }
        };

        mediaRecorder.start(chunkInterval);
        setRecordingStartedAt(Date.now());

        const audioContext = new AudioContext();
        const source = audioContext.createMediaStreamSource(stream);
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
          stream.getTracks().forEach(track => track.stop());
          audioContext.close();
        };
      } catch (err) {
        logger.error('Error accessing microphone:', err);
        setError(true);
        setRecording(false);
      }
    })();
    return () => {
      cancelled = true;
      onCancel();
      setRecordingStoppedAt(Date.now());
    };
  }, [recording, chunkInterval, uploadChunk, mimeType]);

  const volumePercent = recording ? Math.min(volume / 255, 1) * 100 : 0;

  return (
    <div className="relative" style={{ ...getWidth(width) }}>
      <div
        className={cn(
          audioRecorderVariants({ size }),
          disabled ? 'opacity-50 cursor-not-allowed' : 'cursor-pointer'
        )}
        onClick={
          disabled
            ? undefined
            : e => {
                e.stopPropagation();
                if (recording) {
                  setRecording(false);
                } else {
                  setRecording(true);
                  setError(false);
                }
              }
        }
      >
        <div
          className="absolute bottom-0 left-0 w-full transition-all duration-100 ease-linear"
          style={{
            backgroundColor: 'rgba(255,0,0,0.075)',
            height: `${volumePercent}%`,
            pointerEvents: 'none',
            transition: 'height 50ms',
          }}
        />
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className={'mt-2 h-6 w-fit z-10 mx-auto block'}
        >
          {recording ? (
            <Square className={iconSizeVariants({ size })} />
          ) : (
            <Mic className={iconSizeVariants({ size })} />
          )}
        </Button>
        <SecondsCounter
          start={recordingStartedAt}
          stopped={recordingStoppedAt}
          size={size}
        />
        {(label || recordingLabel) && (
          <p
            className={cn(
              'text-center mt-1 text-muted-foreground',
              textSizeVariants({ size })
            )}
          >
            {recording ? recordingLabel : label}
          </p>
        )}
        {error && (
          <p
            className={cn(
              'text-muted-foreground text-center',
              textSizeVariants({ size })
            )}
          >
            Failed to record. Check your settings.
          </p>
        )}
      </div>
    </div>
  );
};

function SecondsCounter(props: {
  start: number | null;
  stopped: number | null;
  size: Sizes;
}) {
  const [seconds, setSeconds] = useState(0);
  useEffect(() => {
    if (typeof props.start !== 'number') {
      queueMicrotask(() => setSeconds(0));
      return;
    }
    if (typeof props.stopped === 'number' && typeof props.start === 'number') {
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
    <p className={cn('text-center', timerSizeVariants({ size: props.size }))}>
      {Math.floor(seconds / 60)
        .toString()
        .padStart(2, '0')}
      :{(seconds % 60).toString().padStart(2, '0')}
    </p>
  );
}

export default AudioRecorderWidget;
