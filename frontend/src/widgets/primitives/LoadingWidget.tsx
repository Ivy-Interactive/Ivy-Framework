import { Loading } from '@/components/Loading';
import React, { useState, useEffect } from 'react';

export const LoadingWidget: React.FC = () => {
  const [showLoader, setShowLoader] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => setShowLoader(true), 200);
    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="fixed inset-0 z-100 flex items-center justify-center bg-black/30">
      {showLoader && <Loading />}
    </div>
  );
};
