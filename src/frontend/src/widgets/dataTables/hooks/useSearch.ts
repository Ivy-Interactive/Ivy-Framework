import { useEffect, useState } from 'react';

/**
 * Hook to manage search visibility and keyboard shortcuts
 */
export const useSearch = (showSearchConfig: boolean) => {
  const [showSearch, setShowSearch] = useState(false);

  useEffect(() => {
    if (!showSearchConfig) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.code === 'KeyF') {
        setShowSearch(current => !current);
        event.stopPropagation();
        event.preventDefault();
      }
    };

    window.addEventListener('keydown', handleKeyDown, true);

    return () => {
      window.removeEventListener('keydown', handleKeyDown, true);
    };
  }, [showSearchConfig]);

  return {
    showSearch,
    setShowSearch,
  };
};
