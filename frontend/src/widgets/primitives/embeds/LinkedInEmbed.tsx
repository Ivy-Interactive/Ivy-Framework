import React, { useEffect, useState } from 'react';
import { sanitizeUrl, loadScript } from './shared';
import EmbedErrorFallback from './EmbedErrorFallback';

interface LinkedInEmbedProps {
  url: string;
}

const LinkedInEmbed: React.FC<LinkedInEmbedProps> = ({ url }) => {
  const [scriptError, setScriptError] = useState(false);

  const postId = React.useMemo(() => {
    // First try to find activity ID in the URL
    let match = url.match(/activity-(\d+)/);
    if (match) {
      return match[1];
    }

    // Try to find URN format
    match = url.match(/urn:li:activity:(\d+)/);
    if (match) {
      return match[1];
    }

    // Try to extract from posts URL
    match = url.match(/linkedin\.com\/posts\/[^/]+-activity-(\d+)/);
    if (match) {
      return match[1];
    }

    return null;
  }, [url]);

  useEffect(() => {
    if (postId) {
      loadScript('https://platform.linkedin.com/in.js')
        .then(() => {
          // LinkedIn script loaded
        })
        .catch(() => {
          setScriptError(true);
        });
    }
  }, [postId]);

  const sanitizedUrl = sanitizeUrl(url);
  if (!postId || scriptError || !sanitizedUrl) {
    return <EmbedErrorFallback url={url} platform="LinkedIn" />;
  }

  return (
    <div className="linkedin-embed w-full">
      <div className="linkedin-embed-container w-full">
        <iframe
          src={`https://www.linkedin.com/embed/feed/update/urn:li:activity:${postId}`}
          className="w-full h-96 sm:h-[32rem] md:h-[40rem] border-0 rounded-lg shadow-md"
          allowFullScreen
          title="Embedded LinkedIn post"
        />
      </div>
    </div>
  );
};

export default LinkedInEmbed;
