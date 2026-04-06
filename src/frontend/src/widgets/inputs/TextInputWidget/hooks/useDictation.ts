import { useState, useCallback, useRef } from "react";
import { logger } from "@/lib/logger";
import { getFullUrl } from "@/lib/url";

interface UseDictationOptions {
  dictationUploadUrl?: string;
  onTranscription?: (text: string) => void;
}

interface UseDictationResult {
  isRecording: boolean;
  startRecording: () => void;
  stopRecording: () => void;
}

const supportedMimeTypes = [
  "audio/webm;codecs=opus",
  "audio/webm",
  "audio/mp4",
  "audio/ogg;codecs=opus",
  "audio/ogg",
  "audio/wav",
];

export function useDictation({
  dictationUploadUrl,
  onTranscription,
}: UseDictationOptions): UseDictationResult {
  const [isRecording, setIsRecording] = useState(false);
  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const streamRef = useRef<MediaStream | null>(null);
  const chunksRef = useRef<Blob[]>([]);

  const stopRecording = useCallback(() => {
    if (mediaRecorderRef.current && mediaRecorderRef.current.state !== "inactive") {
      mediaRecorderRef.current.stop();
    }
    if (streamRef.current) {
      streamRef.current.getTracks().forEach((track) => track.stop());
      streamRef.current = null;
    }
    setIsRecording(false);
  }, []);

  const startRecording = useCallback(async () => {
    if (!dictationUploadUrl) return;

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      streamRef.current = stream;

      let selectedMimeType: string | null = null;
      if (
        typeof MediaRecorder !== "undefined" &&
        typeof MediaRecorder.isTypeSupported === "function"
      ) {
        for (const type of supportedMimeTypes) {
          if (MediaRecorder.isTypeSupported(type)) {
            selectedMimeType = type;
            break;
          }
        }
      }
      if (!selectedMimeType) {
        selectedMimeType = supportedMimeTypes[0];
      }

      chunksRef.current = [];
      const mediaRecorder = new MediaRecorder(stream, { mimeType: selectedMimeType });
      mediaRecorderRef.current = mediaRecorder;

      mediaRecorder.ondataavailable = (event) => {
        if (event.data.size > 0) {
          chunksRef.current.push(event.data);
        }
      };

      mediaRecorder.onstop = async () => {
        const blob = new Blob(chunksRef.current, { type: selectedMimeType! });
        chunksRef.current = [];

        if (blob.size === 0) return;

        const formData = new FormData();
        formData.append("file", blob, "dictation.webm");
        formData.append("mimeType", selectedMimeType!);

        try {
          await fetch(getFullUrl(dictationUploadUrl), {
            method: "POST",
            body: formData,
          });
        } catch (error) {
          logger.error("Dictation upload error:", error);
        }
      };

      mediaRecorder.start();
      setIsRecording(true);
    } catch (err) {
      logger.error("Error accessing microphone for dictation:", err);
      setIsRecording(false);
    }
  }, [dictationUploadUrl, onTranscription]);

  return { isRecording, startRecording, stopRecording };
}
