import React, { useCallback, useEffect, useRef, useState } from 'react';
import { Camera, RotateCcw } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { getWidth } from '@/lib/styles';
import { logger } from '@/lib/logger';
import { toast } from '@/hooks/use-toast';
import { Densities } from '@/types/density';
import { cva } from 'class-variance-authority';

const containerVariant = cva(
  'relative rounded-field border-dashed transition-colors border-muted-foreground/25 overflow-hidden flex flex-col items-center justify-center',
  {
    variants: {
      density: {
        Small: 'p-2 min-h-32 border-2',
        Medium: 'p-3 min-h-48 border-2',
        Large: 'p-4 min-h-64 border-3',
      },
    },
    defaultVariants: { density: 'Medium' },
  }
);

const textVariant = cva('', {
  variants: {
    density: {
      Small: 'text-xs',
      Medium: 'text-sm',
      Large: 'text-base',
    },
  },
  defaultVariants: { density: 'Medium' },
});

const iconVariant = cva('', {
  variants: {
    density: {
      Small: '!size-4',
      Medium: '!size-5',
      Large: '!size-6',
    },
  },
  defaultVariants: { density: 'Medium' },
});

interface CameraInputWidgetProps {
  placeholder?: string;
  disabled: boolean;
  uploadUrl: string;
  facingMode: string;
  width?: string;
  density?: Densities;
}

type CameraState = 'idle' | 'active' | 'captured';

const CameraInputWidget: React.FC<CameraInputWidgetProps> = ({
  placeholder,
  disabled = false,
  uploadUrl,
  facingMode = 'user',
  width,
  density = Densities.Medium,
}) => {
  const [cameraState, setCameraState] = useState<CameraState>('idle');
  const [capturedImage, setCapturedImage] = useState<string | null>(null);
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const streamRef = useRef<MediaStream | null>(null);

  const stopStream = useCallback(() => {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach(track => track.stop());
      streamRef.current = null;
    }
  }, []);

  const getUploadUrl = useCallback(() => {
    const ivyHostMeta = document.querySelector('meta[name="ivy-host"]');
    if (ivyHostMeta) {
      const host = ivyHostMeta.getAttribute('content');
      return host + uploadUrl;
    }
    return uploadUrl;
  }, [uploadUrl]);

  const startCamera = useCallback(async () => {
    if (disabled) return;
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode },
      });
      streamRef.current = stream;
      setCameraState('active');
      setCapturedImage(null);
    } catch (err) {
      logger.error('Error accessing camera:', err);
      toast({
        title: 'Camera Error',
        description:
          err instanceof DOMException && err.name === 'NotAllowedError'
            ? 'Camera access was denied. Please allow camera access in your browser settings.'
            : 'Failed to access camera. Please check your device settings.',
        variant: 'destructive',
      });
    }
  }, [disabled, facingMode]);

  const capture = useCallback(async () => {
    const video = videoRef.current;
    const canvas = canvasRef.current;
    if (!video || !canvas) return;

    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    ctx.drawImage(video, 0, 0);
    stopStream();

    const dataUrl = canvas.toDataURL('image/png');
    setCapturedImage(dataUrl);
    setCameraState('captured');

    canvas.toBlob(async blob => {
      if (!blob || !uploadUrl) return;

      const formData = new FormData();
      formData.append('file', blob, 'capture.png');

      try {
        const response = await fetch(getUploadUrl(), {
          method: 'POST',
          body: formData,
        });
        if (!response.ok) {
          throw new Error(`Upload failed: ${response.statusText}`);
        }
      } catch (error) {
        logger.error('Photo upload error:', error);
        toast({
          title: 'Upload Error',
          description: 'Failed to upload the captured photo.',
          variant: 'destructive',
        });
      }
    }, 'image/png');
  }, [stopStream, uploadUrl, getUploadUrl]);

  const retake = useCallback(() => {
    setCapturedImage(null);
    startCamera();
  }, [startCamera]);

  useEffect(() => {
    if (cameraState === 'active' && videoRef.current && streamRef.current) {
      videoRef.current.srcObject = streamRef.current;
    }
  }, [cameraState]);

  useEffect(() => {
    return () => {
      stopStream();
    };
  }, [stopStream]);

  return (
    <div style={{ ...getWidth(width) }}>
      <div
        className={cn(
          containerVariant({ density }),
          disabled ? 'opacity-50 cursor-not-allowed' : ''
        )}
      >
        <canvas ref={canvasRef} className="hidden" />

        {cameraState === 'idle' && (
          <div
            className={cn(
              'flex flex-col items-center gap-2',
              !disabled && 'cursor-pointer'
            )}
            onClick={disabled ? undefined : startCamera}
            role="button"
            tabIndex={disabled ? -1 : 0}
            onKeyDown={e => {
              if (disabled) return;
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                startCamera();
              }
            }}
          >
            <Camera
              className={cn('text-muted-foreground', iconVariant({ density }))}
            />
            {placeholder && (
              <p
                className={cn(
                  'text-muted-foreground text-center',
                  textVariant({ density })
                )}
              >
                {placeholder}
              </p>
            )}
          </div>
        )}

        {cameraState === 'active' && (
          <div className="flex flex-col items-center gap-2 w-full">
            <video
              ref={videoRef}
              autoPlay
              playsInline
              muted
              className="w-full rounded-sm"
            />
            <Button type="button" variant="outline" size="sm" onClick={capture}>
              <Camera className={iconVariant({ density })} />
              <span className={textVariant({ density })}>Capture</span>
            </Button>
          </div>
        )}

        {cameraState === 'captured' && capturedImage && (
          <div className="flex flex-col items-center gap-2 w-full">
            <img
              src={capturedImage}
              alt="Captured photo"
              className="w-full rounded-sm"
            />
            <Button type="button" variant="outline" size="sm" onClick={retake}>
              <RotateCcw className={iconVariant({ density })} />
              <span className={textVariant({ density })}>Retake</span>
            </Button>
          </div>
        )}
      </div>
    </div>
  );
};

export default CameraInputWidget;
