import { describe, it, expect, beforeEach, afterEach } from "vitest";
import React, { act } from "react";
import { createRoot, Root } from "react-dom/client";
import { MultipleSelector, Option } from "./multiselect";
import { EventHandlerProvider } from "@/components/event-handler";

let container: HTMLDivElement;
let root: Root;

const testOptions: Option[] = [
  { label: "rorychatt", value: "rorychatt" },
  { label: "ArtemKhvorostianyi", value: "artem" },
  { label: "pavel", value: "pavel" },
  { label: "mikael", value: "mikael" },
  { label: "john", value: "john" },
  { label: "jane", value: "jane" },
  { label: "alice", value: "alice" },
  { label: "bob", value: "bob" },
];

function mount(element: React.ReactElement) {
  act(() => {
    root.render(<EventHandlerProvider eventHandler={() => {}}>{element}</EventHandlerProvider>);
  });
}

function getCommandInput(): HTMLInputElement | null {
  return document.body.querySelector("[cmdk-input]") as HTMLInputElement;
}

function getCommandItems(): HTMLElement[] {
  return Array.from(document.body.querySelectorAll("[cmdk-item]"));
}

function openPopover() {
  const input = getCommandInput();
  if (!input) throw new Error("Command input not found");
  act(() => {
    input.focus();
  });
}

function typeInInput(text: string) {
  const input = getCommandInput();
  if (!input) throw new Error("Command input not found");
  act(() => {
    const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
      window.HTMLInputElement.prototype,
      "value",
    )?.set;
    nativeInputValueSetter?.call(input, text);
    input.dispatchEvent(new Event("input", { bubbles: true }));
  });
}

beforeEach(() => {
  container = document.createElement("div");
  document.body.appendChild(container);
  root = createRoot(container);
});

afterEach(() => {
  act(() => {
    root.unmount();
  });
  container.remove();
});

describe("MultipleSelector filtering and highlighting", () => {
  it("renders all options when popover is opened with empty input", () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={true}
      />,
    );

    openPopover();

    const items = getCommandItems();
    expect(items).toHaveLength(8);
  });

  it("filters options by typed search term", () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={true}
      />,
    );

    openPopover();
    typeInInput("ror");

    const items = getCommandItems();
    expect(items).toHaveLength(1);
    expect(items[0].textContent).toContain("rorychatt");
  });

  it("highlights the first matching option when filtering", async () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={true}
      />,
    );

    openPopover();
    typeInInput("ror");

    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 50));
    });

    const items = getCommandItems();
    expect(items).toHaveLength(1);
    const highlighted =
      items[0].getAttribute("aria-selected") === "true" ||
      items[0].getAttribute("data-selected") === "true";
    expect(highlighted).toBe(true);
  });

  it("renders all options when searchable is false", () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={false}
      />,
    );

    openPopover();
    typeInInput("ror");

    const items = getCommandItems();
    expect(items).toHaveLength(8);
  });

  it("shows no matches message when search yields no results", () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={true}
      />,
    );

    openPopover();
    typeInInput("xyz123notfound");

    const items = getCommandItems();
    expect(items).toHaveLength(0);

    const noMatchesText = document.body.textContent;
    expect(noMatchesText).toContain("No matches");
  });

  it("respects searchMode CaseSensitive", () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={true}
        searchMode="CaseSensitive"
      />,
    );

    openPopover();
    typeInInput("ROR");

    const items = getCommandItems();
    expect(items).toHaveLength(0);
  });

  it("shows custom emptyIndicator when provided", () => {
    mount(
      <MultipleSelector
        value={[]}
        defaultOptions={testOptions}
        onValueChange={() => {}}
        searchable={true}
        emptyIndicator={<span>Custom empty message</span>}
      />,
    );

    openPopover();
    typeInInput("xyz123notfound");

    const emptyText = document.body.textContent;
    expect(emptyText).toContain("Custom empty message");
  });
});
