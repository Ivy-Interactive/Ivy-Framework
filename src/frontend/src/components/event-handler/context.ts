import { createContext } from 'react';
import { EventHandlerContextProps } from './types';

export const EventHandlerContext = createContext<
  EventHandlerContextProps | undefined
>(undefined);
