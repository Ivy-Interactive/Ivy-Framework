import { useContext } from "react";
import { DetailContext } from "./DetailContext";

export const useDetailDensity = () => {
  const context = useContext(DetailContext);
  return context.density;
};
