import React, { useEffect, useState } from 'react';

interface EmbedWidgetProps {
  url: string;
}

interface YouTubeEmbedProps {
  url: string;
  width?: string | number;
  height?: string | number;
}

interface TwitterEmbedProps {
  url: string;
}

interface FacebookEmbedProps {
  url: string;
}

interface InstagramEmbedProps {
  url: string;
}

interface TikTokEmbedProps {
  url: string;
}

interface LinkedInEmbedProps {
  url: string;
}

interface PinterestEmbedProps {
  url: string;
}

interface GitHubEmbedProps {
  url: string;
}

interface RedditEmbedProps {
  url: string;
}

const YouTubeEmbed: React.FC<YouTubeEmbedProps> = ({
  url,
  width = '100%',
  height = '100%',
}) => {
  const [videoId, setVideoId] = useState<string | null>(null);

  useEffect(() => {
    const extractVideoId = (ytUrl: string): string | null => {
      let match = ytUrl.match(/youtube\.com\/watch\?v=([^&]+)/);
      if (match) return match[1];
      match = ytUrl.match(/youtu\.be\/([^?]+)/);
      if (match) return match[1];
      match = ytUrl.match(/youtube\.com\/embed\/([^?]+)/);
      if (match) return match[1];

      return null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  if (!videoId) {
    return <div>Invalid YouTube URL.</div>;
  }

  const embedUrl = `https://www.youtube.com/embed/${videoId}`;

  return (
    <iframe
      src={embedUrl}
      width={width}
      height={height}
      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
      allowFullScreen
      title="YouTube"
      className="border-0"
    />
  );
};

const TwitterEmbed: React.FC<TwitterEmbedProps> = ({ url }) => {
  const [tweetId, setTweetId] = useState<string | null>(null);

  useEffect(() => {
    const extractTweetId = (twitterUrl: string): string | null => {
      const match = twitterUrl.match(/twitter\.com\/\w+\/status\/(\d+)/);
      return match ? match[1] : null;
    };

    setTweetId(extractTweetId(url));
  }, [url]);

  if (!tweetId) {
    return <div>Invalid Twitter/X URL.</div>;
  }

  return (
    <div className="twitter-embed">
      <blockquote className="twitter-tweet" data-theme="light" data-dnt="true">
        <a href={url}></a>
      </blockquote>
      <script
        async
        src="https://platform.twitter.com/widgets.js"
        charSet="utf-8"
      ></script>
    </div>
  );
};

const FacebookEmbed: React.FC<FacebookEmbedProps> = ({ url }) => {
  const extractPageName = (facebookUrl: string): string => {
    // Extract page name from various Facebook URL formats
    const match = facebookUrl.match(/facebook\.com\/([^/?]+)/);
    if (match) {
      return match[1];
    }
    return 'Facebook';
  };

  const pageName = extractPageName(url);

  // Show fallback card immediately since Facebook embeds are frequently blocked
  return (
    <div className="facebook-embed border border-gray-300 rounded-lg bg-white shadow-sm overflow-hidden">
      <div className="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50">
        <div className="flex items-center space-x-2">
          <svg
            className="w-5 h-5 text-blue-600"
            fill="currentColor"
            viewBox="0 0 24 24"
          >
            <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
          </svg>
          <span className="text-sm font-medium text-gray-700">
            Facebook Post
          </span>
        </div>
        <a
          href={url}
          target="_blank"
          rel="noopener noreferrer"
          className="text-sm text-blue-600 hover:text-blue-800"
        >
          View on Facebook
        </a>
      </div>
      <div className="p-4">
        <div className="flex items-center space-x-3 mb-4">
          <div className="w-10 h-10 bg-blue-600 rounded-full flex items-center justify-center">
            <svg
              className="w-6 h-6 text-white"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" />
            </svg>
          </div>
          <div>
            <div className="font-semibold text-gray-900">{pageName}</div>
            <div className="text-sm text-gray-500">Facebook</div>
          </div>
        </div>
        <div className="text-center py-6">
          <div className="text-gray-500 mb-4">
            This Facebook post cannot be embedded directly due to privacy
            settings.
          </div>
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
          >
            View Post on Facebook
          </a>
        </div>
      </div>
    </div>
  );
};

const InstagramEmbed: React.FC<InstagramEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);

  useEffect(() => {
    const extractPostId = (instagramUrl: string): string | null => {
      const match = instagramUrl.match(/instagram\.com\/p\/([^/]+)/);
      return match ? match[1] : null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  if (!postId) {
    return <div>Invalid Instagram URL.</div>;
  }

  return (
    <div className="instagram-embed">
      <blockquote
        className="instagram-media"
        data-instgrm-captioned
        data-instgrm-permalink={url}
        data-instgrm-version="14"
        style={{
          background: '#FFF',
          border: 0,
          borderRadius: '3px',
          boxShadow: '0 0 1px 0 rgba(0,0,0,0.5),0 1px 10px 0 rgba(0,0,0,0.15)',
          margin: '1px',
          maxWidth: '540px',
          minWidth: '326px',
          padding: 0,
          width: '99.375%',
        }}
      >
        <div style={{ padding: '16px' }}>
          <a
            href={url}
            style={{
              background: '#FFFFFF',
              lineHeight: 0,
              padding: '0 0',
              textAlign: 'center',
              textDecoration: 'none',
              width: '100%',
            }}
            target="_blank"
            rel="noopener noreferrer"
          >
            <div
              style={{
                display: 'flex',
                flexDirection: 'row',
                alignItems: 'center',
              }}
            >
              <div
                style={{
                  backgroundColor: '#F4F4F4',
                  borderRadius: '50%',
                  flexGrow: 0,
                  height: '40px',
                  marginRight: '14px',
                  width: '40px',
                }}
              />
              <div
                style={{
                  display: 'flex',
                  flexDirection: 'column',
                  flexGrow: 1,
                  justifyContent: 'center',
                }}
              >
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    borderRadius: '4px',
                    flexGrow: 0,
                    height: '14px',
                    marginBottom: '6px',
                    width: '100px',
                  }}
                />
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    borderRadius: '4px',
                    flexGrow: 0,
                    height: '14px',
                    width: '60px',
                  }}
                />
              </div>
            </div>
            <div style={{ padding: '19% 0' }} />
            <div
              style={{
                display: 'block',
                height: '50px',
                margin: '0 auto 12px',
                width: '50px',
              }}
            >
              <svg
                width="50px"
                height="50px"
                viewBox="0 0 60 60"
                version="1.1"
                xmlns="https://www.w3.org/2000/svg"
                xmlnsXlink="https://www.w3.org/1999/xlink"
              >
                <g stroke="none" strokeWidth="1" fill="none" fillRule="evenodd">
                  <g
                    transform="translate(-511.000000, -20.000000)"
                    fill="#000000"
                  >
                    <g>
                      <path d="M556.869,30.41 C554.814,30.41 553.148,32.076 553.148,34.131 C553.148,36.186 554.814,37.852 556.869,37.852 C558.924,37.852 560.59,36.186 560.59,34.131 C560.59,32.076 558.924,30.41 556.869,30.41 M541,60.657 C535.114,60.657 530.342,55.887 530.342,50 C530.342,44.114 535.114,39.342 541,39.342 C546.887,39.342 551.658,44.114 551.658,50 C551.658,55.887 546.887,60.657 541,60.657 M541,33.886 C532.1,33.886 524.886,41.1 524.886,50 C524.886,58.899 532.1,66.113 541,66.113 C549.9,66.113 557.115,58.899 557.115,50 C557.115,41.1 549.9,33.886 541,33.886 M565.378,62.101 C565.244,65.022 564.756,66.606 564.346,67.663 C563.803,69.06 563.154,70.057 562.106,71.106 C561.058,72.155 560.06,72.803 558.662,73.347 C557.607,73.757 556.021,74.244 553.102,74.378 C549.944,74.521 548.997,74.552 541,74.552 C533.003,74.552 532.056,74.521 528.898,74.378 C525.979,74.244 524.393,73.757 523.338,73.347 C521.94,72.803 520.942,72.155 519.894,71.106 C518.846,70.057 518.197,69.06 517.654,67.663 C517.244,66.606 516.755,65.022 516.623,62.101 C516.479,58.943 516.448,57.996 516.448,50 C516.448,42.003 516.479,41.056 516.623,37.899 C516.755,34.978 517.244,33.391 517.654,32.338 C518.197,30.938 518.846,29.942 519.894,28.894 C520.942,27.846 521.94,27.196 523.338,26.654 C524.393,26.244 525.979,25.756 528.898,25.623 C532.057,25.479 533.004,25.448 541,25.448 C548.997,25.448 549.943,25.479 553.102,25.623 C556.021,25.756 557.607,26.244 558.662,26.654 C560.06,27.196 561.058,27.846 562.106,28.894 C563.154,29.942 563.803,30.938 564.346,32.338 C564.756,33.391 565.244,34.978 565.378,37.899 C565.522,41.056 565.552,42.003 565.552,50 C565.552,57.996 565.522,58.943 565.378,62.101 M570.82,37.631 C570.674,34.438 570.167,32.258 569.425,30.349 C568.659,28.377 567.633,26.702 565.965,25.035 C564.297,23.368 562.623,22.342 560.652,21.575 C558.743,20.834 556.562,20.326 553.369,20.18 C550.169,20.033 549.148,20 541,20 C532.853,20 531.831,20.033 528.631,20.18 C525.438,20.326 523.257,20.834 521.349,21.575 C519.376,22.342 517.703,23.368 516.035,25.035 C514.368,26.702 513.342,28.377 512.574,30.349 C511.834,32.258 511.326,34.438 511.181,37.631 C511.035,40.831 511,41.851 511,50 C511,58.147 511.035,59.17 511.181,62.369 C511.326,65.562 511.834,67.743 512.574,69.651 C513.342,71.625 514.368,73.296 516.035,74.965 C517.703,76.634 519.376,77.658 521.349,78.425 C523.257,79.167 525.438,79.673 528.631,79.82 C531.831,79.965 532.853,80.001 541,80.001 C549.148,80.001 550.169,79.965 553.369,79.82 C556.562,79.673 558.743,79.167 560.652,78.425 C562.623,77.658 564.297,76.634 565.965,74.965 C567.633,73.296 568.659,71.625 569.425,69.651 C570.167,67.743 570.674,65.562 570.82,62.369 C570.966,59.17 571,58.147 571,50 C571,41.851 570.966,40.831 570.82,37.631" />
                    </g>
                  </g>
                </g>
              </svg>
            </div>
            <div style={{ paddingTop: '8px' }}>
              <div
                style={{
                  color: '#3897f0',
                  fontFamily: 'Arial,sans-serif',
                  fontSize: '14px',
                  fontStyle: 'normal',
                  fontWeight: 550,
                  lineHeight: '18px',
                }}
              >
                View this post on Instagram
              </div>
            </div>
            <div style={{ padding: '12.5% 0' }} />
            <div
              style={{
                display: 'flex',
                flexDirection: 'row',
                marginBottom: '14px',
                alignItems: 'center',
              }}
            >
              <div>
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    borderRadius: '50%',
                    height: '12.5px',
                    width: '12.5px',
                    transform: 'translateX(0px) translateY(7px)',
                  }}
                />
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    height: '12.5px',
                    transform: 'rotate(-45deg) translateX(3px) translateY(1px)',
                    width: '12.5px',
                    flexGrow: 0,
                    marginRight: '14px',
                    marginLeft: '2px',
                  }}
                />
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    borderRadius: '50%',
                    height: '12.5px',
                    width: '12.5px',
                    transform: 'translateX(9px) translateY(-18px)',
                  }}
                />
              </div>
              <div style={{ marginLeft: '8px' }}>
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    borderRadius: '50%',
                    flexGrow: 0,
                    height: '20px',
                    width: '20px',
                  }}
                />
                <div
                  style={{
                    width: 0,
                    height: 0,
                    borderTop: '2px solid transparent',
                    borderLeft: '6px solid #f4f4f4',
                    borderBottom: '2px solid transparent',
                    transform:
                      'translateX(16px) translateY(-4px) rotate(30deg)',
                  }}
                />
              </div>
              <div style={{ marginLeft: 'auto' }}>
                <div
                  style={{
                    width: '0px',
                    borderTop: '8px solid #F4F4F4',
                    borderRight: '8px solid transparent',
                    transform: 'translateY(16px)',
                  }}
                />
                <div
                  style={{
                    backgroundColor: '#F4F4F4',
                    flexGrow: 0,
                    height: '12px',
                    width: '16px',
                    transform: 'translateY(-4px)',
                  }}
                />
                <div
                  style={{
                    width: 0,
                    height: 0,
                    borderTop: '8px solid #F4F4F4',
                    borderLeft: '8px solid transparent',
                    transform: 'translateY(-4px) translateX(8px)',
                  }}
                />
              </div>
            </div>
            <div
              style={{
                display: 'flex',
                flexDirection: 'column',
                flexGrow: 1,
                justifyContent: 'center',
                marginBottom: '24px',
              }}
            >
              <div
                style={{
                  backgroundColor: '#F4F4F4',
                  borderRadius: '4px',
                  flexGrow: 0,
                  height: '14px',
                  marginBottom: '6px',
                  width: '224px',
                }}
              />
              <div
                style={{
                  backgroundColor: '#F4F4F4',
                  borderRadius: '4px',
                  flexGrow: 0,
                  height: '14px',
                  width: '144px',
                }}
              />
            </div>
          </a>
          <p
            style={{
              color: '#c9c8cd',
              fontFamily: 'Arial,sans-serif',
              fontSize: '14px',
              lineHeight: '17px',
              marginBottom: 0,
              marginTop: '8px',
              overflow: 'hidden',
              padding: '8px 0 7px',
              textAlign: 'center',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
          >
            <a
              href={url}
              style={{
                color: '#c9c8cd',
                fontFamily: 'Arial,sans-serif',
                fontSize: '14px',
                fontStyle: 'normal',
                fontWeight: 'normal',
                lineHeight: '17px',
                textDecoration: 'none',
              }}
              target="_blank"
              rel="noopener noreferrer"
            >
              A post shared by Instagram
            </a>
          </p>
        </div>
      </blockquote>
      <script async src="//www.instagram.com/embed.js"></script>
    </div>
  );
};

const TikTokEmbed: React.FC<TikTokEmbedProps> = ({ url }) => {
  const [videoId, setVideoId] = useState<string | null>(null);

  useEffect(() => {
    const extractVideoId = (tiktokUrl: string): string | null => {
      const match = tiktokUrl.match(/tiktok\.com\/@[\w.-]+\/video\/(\d+)/);
      return match ? match[1] : null;
    };

    setVideoId(extractVideoId(url));
  }, [url]);

  if (!videoId) {
    return <div>Invalid TikTok URL.</div>;
  }

  return (
    <div className="tiktok-embed">
      <blockquote className="tiktok-embed" cite={url} data-video-id={videoId}>
        <section></section>
      </blockquote>
      <script async src="https://www.tiktok.com/embed.js"></script>
    </div>
  );
};

const LinkedInEmbed: React.FC<LinkedInEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);

  useEffect(() => {
    const extractPostId = (linkedinUrl: string): string | null => {
      // LinkedIn post URL: https://www.linkedin.com/posts/activity-1234567890-abcdefgh/
      let match = linkedinUrl.match(/linkedin\.com\/posts\/activity-(\d+)/);
      if (match) return match[1];

      // LinkedIn post URL: https://www.linkedin.com/feed/update/urn:li:activity:1234567890/
      match = linkedinUrl.match(
        /linkedin\.com\/feed\/update\/urn:li:activity:(\d+)/
      );
      if (match) return match[1];

      // LinkedIn post URL: https://www.linkedin.com/posts/username-activity-1234567890/
      match = linkedinUrl.match(/linkedin\.com\/posts\/[^/]+-activity-(\d+)/);
      if (match) return match[1];

      // Any LinkedIn URL that contains "posts" or "feed"
      match = linkedinUrl.match(/linkedin\.com\/(posts|feed)/);
      if (match) return 'linkedin-post';

      return null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  if (!postId) {
    return <div>Invalid LinkedIn URL.</div>;
  }

  return (
    <div className="linkedin-embed">
      <div
        className="linkedin-embed-container"
        data-linkedin-url={url}
        style={{
          width: '100%',
          maxWidth: '550px',
          margin: '0 auto',
        }}
      >
        {/* LinkedIn Official Embed - Using the correct iframe format */}
        <iframe
          src={`https://www.linkedin.com/embed/feed/update/urn:li:activity:${postId}`}
          height="600"
          width="100%"
          frameBorder="0"
          allowFullScreen
          title="Embedded post"
          style={{
            border: 'none',
            borderRadius: '8px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
            maxWidth: '504px',
          }}
        />
      </div>

      {/* LinkedIn Embed Script */}
      <script
        src="https://platform.linkedin.com/in.js"
        type="text/javascript"
        async
      />
    </div>
  );
};

const PinterestEmbed: React.FC<PinterestEmbedProps> = ({ url }) => {
  return (
    <div className="pinterest-embed">
      <a
        data-pin-do="embedPin"
        data-pin-width="medium"
        href={url}
        style={{
          display: 'flex',
          width: '345px',
          height: '714px',
          border: '1px solid #ddd',
          borderRadius: '8px',
          backgroundColor: '#f8f9fa',
          alignItems: 'center',
          justifyContent: 'center',
          textDecoration: 'none',
          color: '#333',
        }}
      >
        <div style={{ textAlign: 'center' }}>
          <div style={{ fontSize: '24px', marginBottom: '8px' }}>📌</div>
          <div style={{ fontSize: '14px', fontWeight: 'bold' }}>
            Pinterest Pin
          </div>
          <div style={{ fontSize: '12px', marginTop: '4px' }}>
            Click to view on Pinterest
          </div>
        </div>
      </a>
      <script async defer src="//assets.pinterest.com/js/pinit.js"></script>
    </div>
  );
};

const GitHubEmbed: React.FC<GitHubEmbedProps> = ({ url }) => {
  const [embedData, setEmbedData] = useState<{
    type:
      | 'repository'
      | 'issue'
      | 'issues'
      | 'pull'
      | 'pulls'
      | 'gist'
      | 'file'
      | 'unknown';
    owner?: string;
    repo?: string;
    number?: string;
    gistId?: string;
  } | null>(null);

  const [fileContent, setFileContent] = useState<string>('');
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [issueData, setIssueData] = useState<{
    number: number;
    title: string;
    state: string;
    created_at: string;
    user: { login: string };
    labels: Array<{ id: number; name: string; color: string }>;
    body: string;
  } | null>(null);

  const [issuesList, setIssuesList] = useState<
    Array<{
      number: number;
      title: string;
      state: string;
      created_at: string;
      user: { login: string };
      labels: Array<{ id: number; name: string; color: string }>;
      body: string;
    }>
  >([]);

  const [pullsList, setPullsList] = useState<
    Array<{
      number: number;
      title: string;
      state: string;
      created_at: string;
      user: { login: string };
      labels: Array<{ id: number; name: string; color: string }>;
      body: string;
    }>
  >([]);

  useEffect(() => {
    const parseGitHubUrl = (githubUrl: string) => {
      // Repository: https://github.com/owner/repo
      let match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)(?:\/)?$/);
      if (match) {
        return {
          type: 'repository' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      // File: https://github.com/owner/repo/blob/branch/path/to/file
      match = githubUrl.match(
        /github\.com\/([^/]+)\/([^/]+)\/blob\/([^/]+)\/(.+)/
      );
      if (match) {
        return {
          type: 'file' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      // Issue: https://github.com/owner/repo/issues/123
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)\/issues\/(\d+)/);
      if (match) {
        return {
          type: 'issue' as const,
          owner: match[1],
          repo: match[2],
          number: match[3],
        };
      }

      // Issues list: https://github.com/owner/repo/issues
      match = githubUrl.match(
        /github\.com\/([^/]+)\/([^/]+)\/issues(?:\?.*)?$/
      );
      if (match) {
        return {
          type: 'issues' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      // Pull Request: https://github.com/owner/repo/pull/123
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)\/pull\/(\d+)/);
      if (match) {
        return {
          type: 'pull' as const,
          owner: match[1],
          repo: match[2],
          number: match[3],
        };
      }

      // Pull Requests list: https://github.com/owner/repo/pulls
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)\/pulls(?:\?.*)?$/);
      if (match) {
        return {
          type: 'pulls' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      // Gist: https://gist.github.com/username/gistId
      match = githubUrl.match(/gist\.github\.com\/([^/]+)\/([^/]+)/);
      if (match) {
        return {
          type: 'gist' as const,
          owner: match[1],
          gistId: match[2],
        };
      }

      // Any other GitHub URL (commits, releases, etc.)
      match = githubUrl.match(/github\.com\/([^/]+)\/([^/]+)/);
      if (match) {
        return {
          type: 'repository' as const,
          owner: match[1],
          repo: match[2],
        };
      }

      return { type: 'unknown' as const };
    };

    setEmbedData(parseGitHubUrl(url));
  }, [url]);

  useEffect(() => {
    if (embedData?.type === 'file') {
      const fetchFileContent = async () => {
        try {
          setIsLoading(true);
          setError(null);

          // Convert GitHub URL to raw content URL
          const rawUrl = url
            .replace('github.com', 'raw.githubusercontent.com')
            .replace('/blob/', '/');

          const response = await fetch(rawUrl);
          if (!response.ok) {
            throw new Error(`Failed to fetch file: ${response.statusText}`);
          }

          const content = await response.text();
          setFileContent(content);
        } catch (err) {
          setError(err instanceof Error ? err.message : 'Failed to load file');
        } finally {
          setIsLoading(false);
        }
      };

      fetchFileContent();
    } else if (
      embedData?.type === 'issue' &&
      embedData.owner &&
      embedData.repo &&
      embedData.number
    ) {
      const fetchIssueData = async () => {
        try {
          setIsLoading(true);
          setError(null);

          const apiUrl = `https://api.github.com/repos/${embedData.owner}/${embedData.repo}/issues/${embedData.number}`;
          const response = await fetch(apiUrl, {
            headers: {
              Accept: 'application/vnd.github.v3+json',
            },
          });
          if (!response.ok) {
            if (response.status === 403) {
              // Rate limit exceeded - show fallback card
              setError(
                'Rate limit exceeded. Please try again later or view on GitHub.'
              );
              return;
            }
            const errorText = await response.text();
            throw new Error(
              `Failed to fetch issue: ${response.status} ${response.statusText} - ${errorText}`
            );
          }

          const issue = await response.json();
          setIssueData(issue);
        } catch (err) {
          setError(err instanceof Error ? err.message : 'Failed to load issue');
        } finally {
          setIsLoading(false);
        }
      };

      fetchIssueData();
    } else if (
      embedData?.type === 'issues' &&
      embedData.owner &&
      embedData.repo
    ) {
      const fetchIssuesList = async () => {
        try {
          setIsLoading(true);
          setError(null);

          // Extract query parameters from the original URL
          const urlObj = new URL(url);
          const params = new URLSearchParams(urlObj.search);

          // Build API URL with query parameters
          const apiUrl = `https://api.github.com/repos/${embedData.owner}/${embedData.repo}/issues?${params.toString()}`;
          const response = await fetch(apiUrl, {
            headers: {
              Accept: 'application/vnd.github.v3+json',
            },
          });
          if (!response.ok) {
            if (response.status === 403) {
              // Rate limit exceeded - show fallback card
              setError(
                'Rate limit exceeded. Please try again later or view on GitHub.'
              );
              return;
            }
            const errorText = await response.text();
            throw new Error(
              `Failed to fetch issues: ${response.status} ${response.statusText} - ${errorText}`
            );
          }

          const issues = await response.json();
          setIssuesList(issues);
        } catch (err) {
          setError(
            err instanceof Error ? err.message : 'Failed to load issues'
          );
        } finally {
          setIsLoading(false);
        }
      };

      fetchIssuesList();
    } else if (
      embedData?.type === 'pulls' &&
      embedData.owner &&
      embedData.repo
    ) {
      const fetchPullsList = async () => {
        try {
          setIsLoading(true);
          setError(null);

          // Extract query parameters from the original URL
          const urlObj = new URL(url);
          const params = new URLSearchParams(urlObj.search);

          // Build API URL with query parameters
          const apiUrl = `https://api.github.com/repos/${embedData.owner}/${embedData.repo}/pulls?${params.toString()}`;
          const response = await fetch(apiUrl, {
            headers: {
              Accept: 'application/vnd.github.v3+json',
            },
          });
          if (!response.ok) {
            if (response.status === 403) {
              // Rate limit exceeded - show fallback card
              setError(
                'Rate limit exceeded. Please try again later or view on GitHub.'
              );
              return;
            }
            const errorText = await response.text();
            throw new Error(
              `Failed to fetch pull requests: ${response.status} ${response.statusText} - ${errorText}`
            );
          }

          const pulls = await response.json();
          setPullsList(pulls);
        } catch (err) {
          setError(
            err instanceof Error ? err.message : 'Failed to load pull requests'
          );
        } finally {
          setIsLoading(false);
        }
      };

      fetchPullsList();
    } else {
      setIsLoading(false);
    }
  }, [
    url,
    embedData?.type,
    embedData?.owner,
    embedData?.repo,
    embedData?.number,
  ]);

  if (!embedData || embedData.type === 'unknown') {
    return <div>Invalid GitHub URL.</div>;
  }

  const { type, owner, repo, number, gistId } = embedData;

  if (type === 'gist') {
    return (
      <div className="github-embed">
        <script src={`https://gist.github.com/${owner}/${gistId}.js`}></script>
      </div>
    );
  }

  if (type === 'file') {
    const getFileExtension = (url: string): string => {
      const match = url.match(/\.([^.]+)$/);
      return match ? match[1] : 'txt';
    };

    const getLanguageFromExtension = (ext: string): string => {
      const languageMap: { [key: string]: string } = {
        cs: 'csharp',
        js: 'javascript',
        ts: 'typescript',
        tsx: 'tsx',
        jsx: 'jsx',
        py: 'python',
        java: 'java',
        cpp: 'cpp',
        c: 'c',
        h: 'c',
        hpp: 'cpp',
        php: 'php',
        rb: 'ruby',
        go: 'go',
        rs: 'rust',
        swift: 'swift',
        kt: 'kotlin',
        scala: 'scala',
        sh: 'bash',
        bash: 'bash',
        zsh: 'bash',
        fish: 'bash',
        ps1: 'powershell',
        sql: 'sql',
        html: 'html',
        css: 'css',
        scss: 'scss',
        sass: 'sass',
        less: 'less',
        xml: 'xml',
        json: 'json',
        yaml: 'yaml',
        yml: 'yaml',
        md: 'markdown',
        txt: 'text',
        log: 'text',
      };
      return languageMap[ext.toLowerCase()] || 'text';
    };

    if (isLoading) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600">Loading file...</span>
          </div>
        </div>
      );
    }

    if (error) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center space-x-2">
              <svg
                className="w-5 h-5 text-gray-700"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
              </svg>
              <span className="text-sm font-medium text-gray-700">
                GitHub File
              </span>
            </div>
            <a
              href={url}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-blue-600 hover:text-blue-800"
            >
              View on GitHub
            </a>
          </div>
          <div className="text-red-600 text-sm">{error}</div>
        </div>
      );
    }

    const fileExt = getFileExtension(url);
    const language = getLanguageFromExtension(fileExt);

    return (
      <div className="github-embed border border-gray-300 rounded-lg bg-white shadow-sm overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center space-x-2">
            <svg
              className="w-5 h-5 text-gray-700"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
            </svg>
            <span className="text-sm font-medium text-gray-700">
              GitHub File
            </span>
            <span className="text-xs text-gray-500 bg-gray-200 px-2 py-1 rounded">
              {fileExt}
            </span>
          </div>
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-blue-600 hover:text-blue-800"
          >
            View on GitHub
          </a>
        </div>
        <div className="relative">
          <pre className="p-4 overflow-auto max-h-96 bg-gray-900 text-gray-100 text-sm">
            <code className={`language-${language}`}>{fileContent}</code>
          </pre>
        </div>
      </div>
    );
  }

  if (type === 'issue') {
    if (isLoading) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600">Loading issue...</span>
          </div>
        </div>
      );
    }

    if (error) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center space-x-2">
              <svg
                className="w-5 h-5 text-gray-700"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
              </svg>
              <span className="text-sm font-medium text-gray-700">
                GitHub Issue
              </span>
            </div>
            <a
              href={url}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-blue-600 hover:text-blue-800"
            >
              View on GitHub
            </a>
          </div>
          <div className="text-red-600 text-sm">{error}</div>
        </div>
      );
    }

    if (!issueData) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="text-gray-600 text-sm">No issue data available</div>
        </div>
      );
    }

    const formatDate = (dateString: string) => {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    };

    const getStateColor = (state: string) => {
      return state === 'open'
        ? 'bg-green-100 text-green-800'
        : 'bg-gray-100 text-gray-800';
    };

    return (
      <div className="github-embed border border-gray-300 rounded-lg bg-white shadow-sm overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center space-x-2">
            <svg
              className="w-5 h-5 text-gray-700"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
            </svg>
            <span className="text-sm font-medium text-gray-700">
              GitHub Issue
            </span>
            <span
              className={`text-xs px-2 py-1 rounded ${getStateColor(issueData.state)}`}
            >
              {issueData.state}
            </span>
          </div>
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-blue-600 hover:text-blue-800"
          >
            View on GitHub
          </a>
        </div>
        <div className="p-4">
          <div className="mb-3">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">
              #{issueData.number} - {issueData.title}
            </h3>
            <div className="flex items-center space-x-4 text-sm text-gray-600">
              <span>Created: {formatDate(issueData.created_at)}</span>
              {issueData.user && <span>by {issueData.user.login}</span>}
            </div>
          </div>

          {issueData.labels && issueData.labels.length > 0 && (
            <div className="mb-3">
              <div className="flex flex-wrap gap-1">
                {issueData.labels.map(label => (
                  <span
                    key={label.id}
                    className="text-xs px-2 py-1 rounded"
                    style={{
                      backgroundColor: `#${label.color}`,
                      color: '#fff',
                    }}
                  >
                    {label.name}
                  </span>
                ))}
              </div>
            </div>
          )}

          {issueData.body && (
            <div className="prose prose-sm max-w-none">
              <div className="whitespace-pre-wrap text-gray-700">
                {issueData.body}
              </div>
            </div>
          )}
        </div>
      </div>
    );
  }

  if (type === 'issues') {
    if (isLoading) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600">Loading issues...</span>
          </div>
        </div>
      );
    }

    if (error) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center space-x-2">
              <svg
                className="w-5 h-5 text-gray-700"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
              </svg>
              <span className="text-sm font-medium text-gray-700">
                GitHub Issues
              </span>
            </div>
            <a
              href={url}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-blue-600 hover:text-blue-800"
            >
              View on GitHub
            </a>
          </div>
          <div className="text-red-600 text-sm">{error}</div>
        </div>
      );
    }

    const formatDate = (dateString: string) => {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    };

    const getStateColor = (state: string) => {
      return state === 'open'
        ? 'bg-green-100 text-green-800'
        : 'bg-gray-100 text-gray-800';
    };

    return (
      <div className="github-embed border border-gray-300 rounded-lg bg-white shadow-sm overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center space-x-2">
            <svg
              className="w-5 h-5 text-gray-700"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
            </svg>
            <span className="text-sm font-medium text-gray-700">
              GitHub Issues
            </span>
            <span className="text-xs text-gray-500 bg-gray-200 px-2 py-1 rounded">
              {issuesList.length} found
            </span>
          </div>
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-blue-600 hover:text-blue-800"
          >
            View on GitHub
          </a>
        </div>
        <div className="p-4 max-h-96 overflow-y-auto">
          {issuesList.length === 0 ? (
            <div className="text-gray-500 text-center py-4">
              No issues found
            </div>
          ) : (
            <div className="space-y-4">
              {issuesList.map(issue => (
                <div
                  key={issue.number}
                  className="border border-gray-200 rounded-lg p-3 hover:bg-gray-50"
                >
                  <div className="flex items-start justify-between mb-2">
                    <h4 className="font-semibold text-gray-900 text-sm">
                      #{issue.number} - {issue.title}
                    </h4>
                    <span
                      className={`text-xs px-2 py-1 rounded ${getStateColor(issue.state)}`}
                    >
                      {issue.state}
                    </span>
                  </div>
                  <div className="text-xs text-gray-600 mb-2">
                    Created: {formatDate(issue.created_at)} by{' '}
                    {issue.user.login}
                  </div>
                  {issue.labels && issue.labels.length > 0 && (
                    <div className="flex flex-wrap gap-1 mb-2">
                      {issue.labels.map(label => (
                        <span
                          key={label.id}
                          className="text-xs px-2 py-1 rounded"
                          style={{
                            backgroundColor: `#${label.color}`,
                            color: '#fff',
                          }}
                        >
                          {label.name}
                        </span>
                      ))}
                    </div>
                  )}
                  {issue.body && (
                    <div className="text-sm text-gray-700 line-clamp-3">
                      {issue.body.substring(0, 200)}
                      {issue.body.length > 200 && '...'}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    );
  }

  if (type === 'pulls') {
    if (isLoading) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-center py-8">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
            <span className="ml-2 text-gray-600">Loading pull requests...</span>
          </div>
        </div>
      );
    }

    if (error) {
      return (
        <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <div className="flex items-center space-x-2">
              <svg
                className="w-5 h-5 text-gray-700"
                fill="currentColor"
                viewBox="0 0 24 24"
              >
                <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
              </svg>
              <span className="text-sm font-medium text-gray-700">
                GitHub Pull Requests
              </span>
            </div>
            <a
              href={url}
              target="_blank"
              rel="noopener noreferrer"
              className="text-sm text-blue-600 hover:text-blue-800"
            >
              View on GitHub
            </a>
          </div>
          <div className="text-red-600 text-sm">{error}</div>
        </div>
      );
    }

    const formatDate = (dateString: string) => {
      return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });
    };

    const getStateColor = (state: string) => {
      return state === 'open'
        ? 'bg-green-100 text-green-800'
        : 'bg-gray-100 text-gray-800';
    };

    return (
      <div className="github-embed border border-gray-300 rounded-lg bg-white shadow-sm overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b border-gray-200 bg-gray-50">
          <div className="flex items-center space-x-2">
            <svg
              className="w-5 h-5 text-gray-700"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
            </svg>
            <span className="text-sm font-medium text-gray-700">
              GitHub Pull Requests
            </span>
            <span className="text-xs text-gray-500 bg-gray-200 px-2 py-1 rounded">
              {pullsList.length} found
            </span>
          </div>
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="text-sm text-blue-600 hover:text-blue-800"
          >
            View on GitHub
          </a>
        </div>
        <div className="p-4 max-h-96 overflow-y-auto">
          {pullsList.length === 0 ? (
            <div className="text-gray-500 text-center py-4">
              No pull requests found
            </div>
          ) : (
            <div className="space-y-4">
              {pullsList.map(pull => (
                <div
                  key={pull.number}
                  className="border border-gray-200 rounded-lg p-3 hover:bg-gray-50"
                >
                  <div className="flex items-start justify-between mb-2">
                    <h4 className="font-semibold text-gray-900 text-sm">
                      #{pull.number} - {pull.title}
                    </h4>
                    <span
                      className={`text-xs px-2 py-1 rounded ${getStateColor(pull.state)}`}
                    >
                      {pull.state}
                    </span>
                  </div>
                  <div className="text-xs text-gray-600 mb-2">
                    Created: {formatDate(pull.created_at)} by {pull.user.login}
                  </div>
                  {pull.labels && pull.labels.length > 0 && (
                    <div className="flex flex-wrap gap-1 mb-2">
                      {pull.labels.map(label => (
                        <span
                          key={label.id}
                          className="text-xs px-2 py-1 rounded"
                          style={{
                            backgroundColor: `#${label.color}`,
                            color: '#fff',
                          }}
                        >
                          {label.name}
                        </span>
                      ))}
                    </div>
                  )}
                  {pull.body && (
                    <div className="text-sm text-gray-700 line-clamp-3">
                      {pull.body.substring(0, 200)}
                      {pull.body.length > 200 && '...'}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    );
  }

  // For repositories and pull requests, create a custom card instead of iframe
  const getTitle = () => {
    switch (type) {
      case 'repository':
        return `${owner}/${repo}`;
      case 'pull':
        return `${owner}/${repo} PR #${number}`;
      default:
        return 'GitHub';
    }
  };

  const getDescription = () => {
    switch (type) {
      case 'repository':
        return 'View repository on GitHub';
      case 'pull':
        return 'View pull request on GitHub';
      default:
        return 'View on GitHub';
    }
  };

  return (
    <div className="github-embed border border-gray-300 rounded-lg p-4 bg-white shadow-sm">
      <div className="flex items-center space-x-3">
        <div className="flex-shrink-0">
          <svg
            className="w-8 h-8 text-gray-700"
            fill="currentColor"
            viewBox="0 0 24 24"
          >
            <path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z" />
          </svg>
        </div>
        <div className="flex-1 min-w-0">
          <h3 className="text-lg font-semibold text-gray-900 truncate">
            {getTitle()}
          </h3>
          <p className="text-sm text-gray-600">{getDescription()}</p>
        </div>
        <div className="flex-shrink-0">
          <a
            href={url}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm leading-4 font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
          >
            View on GitHub
          </a>
        </div>
      </div>
    </div>
  );
};

const RedditEmbed: React.FC<RedditEmbedProps> = ({ url }) => {
  const [postId, setPostId] = useState<string | null>(null);

  useEffect(() => {
    const extractPostId = (redditUrl: string): string | null => {
      // Reddit post URL: https://www.reddit.com/r/subreddit/comments/postId/title/
      const match = redditUrl.match(/reddit\.com\/r\/[^/]+\/comments\/([^/]+)/);
      return match ? match[1] : null;
    };

    setPostId(extractPostId(url));
  }, [url]);

  if (!postId) {
    return <div>Invalid Reddit URL.</div>;
  }

  return (
    <div className="redditwrapper">
      <blockquote className="reddit-card" data-card-created={Date.now()}>
        <a href={url}>Reddit Post</a>
      </blockquote>
      <script
        src="https://embed.redditmedia.com/widgets/platform.js"
        async
      ></script>
      <style
        dangerouslySetInnerHTML={{
          __html: `
            .redditwrapper {
              background: #fff;
              position: relative;
            }
            .redditwrapper iframe {
              border: 0;
              position: relative;
              z-index: 2;
            }
            .redditwrapper a {
              color: rgba(0, 0, 0, 0);
              position: absolute;
              left: 0;
              top: 0;
              z-index: 0;
            }
          `,
        }}
      />
    </div>
  );
};

const EmbedWidget: React.FC<EmbedWidgetProps> = ({ url }) => {
  if (url.includes('facebook.com')) {
    return <FacebookEmbed url={url} />;
  }
  if (url.includes('instagram.com')) {
    return <InstagramEmbed url={url} />;
  }
  if (url.includes('tiktok.com')) {
    return <TikTokEmbed url={url} />;
  }
  if (url.includes('twitter.com') || url.includes('x.com')) {
    return <TwitterEmbed url={url} />;
  }
  if (url.includes('youtube.com') || url.includes('youtu.be')) {
    return (
      <div className="relative w-full pt-[56.25%]">
        <div className="absolute top-0 left-0 w-full h-full">
          <YouTubeEmbed url={url} width="100%" height="100%" />
        </div>
      </div>
    );
  }
  if (url.includes('linkedin.com')) {
    return <LinkedInEmbed url={url} />;
  }
  if (url.includes('pinterest.com') || url.includes('pin.it')) {
    return <PinterestEmbed url={url} />;
  }
  if (url.includes('github.com') || url.includes('gist.github.com')) {
    return <GitHubEmbed url={url} />;
  }
  if (url.includes('reddit.com')) {
    return <RedditEmbed url={url} />;
  }

  return <p>The provided URL is not supported for embedding.</p>;
};

export default EmbedWidget;
