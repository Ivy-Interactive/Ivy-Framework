import { Loading } from "./Loading";
import { useEffect, useState } from "react";

export const LoadingScreen = () => {
  const [showAnimation, setShowAnimation] = useState(false);

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowAnimation(true);
    }, 500);

    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-white/80 z-50">
      {showAnimation && <Loading />}
    </div>
  );
};
