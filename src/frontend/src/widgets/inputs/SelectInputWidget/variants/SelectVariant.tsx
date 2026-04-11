import React from "react";
import { SelectInputWidgetProps } from "../../select-types";
import { EventHandler } from "@/components/event-handler";
import { SelectMultiVariant } from "../../SelectMultiVariant";
import { SelectSingleVariant } from "../../SelectSingleVariant";

export const SelectVariant: React.FC<SelectInputWidgetProps & { eventHandler: EventHandler }> = (
  props,
) => {
  return props.selectMany ? <SelectMultiVariant {...props} /> : <SelectSingleVariant {...props} />;
};
