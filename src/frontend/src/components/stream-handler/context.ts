import { createContext } from "react";
import { StreamHandlerContextProps } from "./types";

export const StreamHandlerContext = createContext<StreamHandlerContextProps | undefined>(undefined);
