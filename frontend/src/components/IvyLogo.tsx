import { ComponentPropsWithoutRef } from 'react';

export default function IvyLogo({
  className,
  ...props
}: ComponentPropsWithoutRef<'svg'>) {
  return (
    <svg
      viewBox="0 0 372 296"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={className}
      {...props}
    >
      <path
        d="M74.5411 0H0.141078V74.035C41.1772 74.035 74.5411 40.9048 74.5411 0Z"
        fill="currentColor"
      />
      <path
        d="M372 221.965V73.9651C330.964 73.9651 297.6 107.095 297.6 148C297.6 107.165 264.307 73.9651 223.2 73.9651C182.164 73.9651 148.8 107.095 148.8 148C148.8 107.165 115.507 73.9651 74.4 73.9651H0V221.965H74.4V147.93C74.4 188.765 107.693 221.965 148.8 221.965C189.836 221.965 223.13 188.835 223.2 148C223.2 188.835 256.494 221.965 297.53 221.965H223.2C223.2 262.8 256.493 296 297.6 296C338.636 296 372 262.87 372 221.965Z"
        fill="currentColor"
      />
    </svg>
  );
}
