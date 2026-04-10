# YouTube Transcript Extraction

When asked to mine a YouTube video for ideas, use the `youtube-transcript-api` Python package to fetch the transcript:

```bash
pip install youtube-transcript-api
python -c "
from youtube_transcript_api import YouTubeTranscriptApi
ytt_api = YouTubeTranscriptApi()
transcript = ytt_api.fetch(video_id='VIDEO_ID')
for entry in transcript:
    print(entry.text)
"
```

The package is already installed on this system. Extract the video ID from the URL parameter `v=`.

For video metadata (title, channel), use the oembed endpoint:
`https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v=VIDEO_ID&format=json`
