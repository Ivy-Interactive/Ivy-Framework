import { useEffect, useState } from 'react';
import { Loading } from './Loading';

export const LoadingScreen = () => {
  const [showAnimation, setShowAnimation] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowAnimation(true);
    }, 500);

    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="flex items-center justify-center h-screen">
      {showAnimation && <Loading />}
    </div>
  );
};
