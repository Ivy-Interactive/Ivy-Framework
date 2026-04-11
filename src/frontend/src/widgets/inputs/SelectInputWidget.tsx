import React from "react";
import { useEventHandler } from "@/components/event-handler";
import { Densities } from "@/types/density";
import { SelectInputWidgetProps } from "./select-types";
import {
  ToggleVariant,
  RadioVariant,
  CheckboxVariant,
  SliderVariant,
  SelectVariant,
} from "./SelectInputWidget/variants";

export const SelectInputWidget: React.FC<SelectInputWidgetProps> = (props) => {
  const eventHandler = useEventHandler();

  // Normalize undefined to null when nullable
  const normalizedProps = {
    ...props,
    value: props.nullable && props.value === undefined ? null : props.value,
    density: props.density ?? Densities.Medium,
    variant: props.variant ?? "Select",
    separator: props.separator ?? ";",
    selectMany: props.selectMany ?? false,
    maxSelections: props.maxSelections,
    minSelections: props.minSelections,
    searchable: props.searchable ?? false,
    searchMode: props.searchMode ?? "CaseInsensitive",
    emptyMessage: props.emptyMessage,
    loading: props.loading ?? false,
    ghost: props.ghost ?? false,
    showActions: props.showActions ?? false,
  };

  switch (normalizedProps.variant) {
    case "List":
      return normalizedProps.selectMany ? (
        <CheckboxVariant {...normalizedProps} eventHandler={eventHandler} />
      ) : (
        <RadioVariant {...normalizedProps} eventHandler={eventHandler} />
      );
    case "Radio":
      return <RadioVariant {...normalizedProps} eventHandler={eventHandler} />;
    case "Toggle":
      return <ToggleVariant {...normalizedProps} eventHandler={eventHandler} />;
    case "Slider":
      return <SliderVariant {...normalizedProps} eventHandler={eventHandler} />;
    default:
      return <SelectVariant {...normalizedProps} eventHandler={eventHandler} />;
  }
};

export default SelectInputWidget;
