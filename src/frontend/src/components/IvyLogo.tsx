import { ComponentPropsWithoutRef } from "react";

export default function IvyLogo({ className, ...props }: ComponentPropsWithoutRef<"svg">) {
  return (
    <svg
      viewBox="0 0 2076 1661"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
      {...props}
    >
      <path d="M415.20 0H0.00V415.20C229.31 415.20 415.20 229.31 415.20 0Z" fill="currentColor" />
      <path
        d="M2076 415.19C1846.69 415.19 1660.8 601.08 1660.8 830.39C1660.8 601.08 1474.92 415.19 1245.61 415.19C1016.3 415.19 830.41 601.08 830.41 830.39C830.41 601.08 644.52 415.19 415.21 415.19H0V1245.61H415.19V830.40C415.19 1059.72 601.08 1245.61 830.39 1245.61C1059.69 1245.61 1245.57 1059.73 1245.59 830.43C1245.61 1059.71 1431.45 1245.56 1660.72 1245.6H1245.59C1245.59 1474.91 1431.47 1660.8 1660.78 1660.8C1890.09 1660.8 2075.98 1474.91 2075.98 1245.6V415.19H2076Z"
        fill="currentColor"
      />
    </svg>
  );
}
