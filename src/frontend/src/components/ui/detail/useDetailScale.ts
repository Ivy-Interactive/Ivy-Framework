import { useContext } from "react";
import { DetailContext } from "./DetailContext";

export const useDetailScale = () => {
  const context = useContext(DetailContext);
  return context.density;
};
