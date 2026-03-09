/// <reference types="vite/client" />

import 'react';

declare module 'react' {
  namespace JSX {
    interface IntrinsicElements {
      'ivy-widget': React.DetailedHTMLProps<React.HTMLAttributes<HTMLElement>, HTMLElement> & {
        type?: string;
      };
    }
  }
}
