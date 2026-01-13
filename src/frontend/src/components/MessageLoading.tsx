export const MessageLoading = () => {
  return (
    <svg
      width="24"
      height="24"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
      className="text-primary"
    >
      <circle cx="4" cy="12" r="2" fill="currentColor">
        <animate
          attributeName="cy"
          calcMode="spline"
          dur="1s"
          values="12;6;12"
          keySplines=".33,.66,.66,1;.33,0,.66,.33"
          repeatCount="indefinite"
          begin="0s"
        />
      </circle>
      <circle cx="12" cy="12" r="2" fill="currentColor">
        <animate
          attributeName="cy"
          calcMode="spline"
          dur="1s"
          values="12;6;12"
          keySplines=".33,.66,.66,1;.33,0,.66,.33"
          repeatCount="indefinite"
          begin="0.1s"
        />
      </circle>
      <circle cx="20" cy="12" r="2" fill="currentColor">
        <animate
          attributeName="cy"
          calcMode="spline"
          dur="1s"
          values="12;6;12"
          keySplines=".33,.66,.66,1;.33,0,.66,.33"
          repeatCount="indefinite"
          begin="0.2s"
        />
      </circle>
    </svg>
  );
};
