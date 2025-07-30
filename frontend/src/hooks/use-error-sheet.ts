'use client';

import * as React from 'react';

export interface ErrorSheetData {
  id: string;
  title?: string | null;
  message?: string | null;
  stackTrace?: string | null;
  open?: boolean;
}

type ActionType = {
  SHOW_ERROR: 'SHOW_ERROR';
  HIDE_ERROR: 'HIDE_ERROR';
  CLEAR_ERROR: 'CLEAR_ERROR';
};

let count = 0;

function genId() {
  count = (count + 1) % Number.MAX_SAFE_INTEGER;
  return count.toString();
}

type Action =
  | {
      type: ActionType['SHOW_ERROR'];
      error: ErrorSheetData;
    }
  | {
      type: ActionType['HIDE_ERROR'];
      errorId: string;
    }
  | {
      type: ActionType['CLEAR_ERROR'];
      errorId: string;
    };

interface State {
  errors: ErrorSheetData[];
}

export const reducer = (state: State, action: Action): State => {
  switch (action.type) {
    case 'SHOW_ERROR':
      return {
        ...state,
        errors: [...state.errors, { ...action.error, open: true }],
      };

    case 'HIDE_ERROR':
      return {
        ...state,
        errors: state.errors.map(e =>
          e.id === action.errorId ? { ...e, open: false } : e
        ),
      };

    case 'CLEAR_ERROR':
      return {
        ...state,
        errors: state.errors.filter(e => e.id !== action.errorId),
      };
  }
};

const listeners: Array<(state: State) => void> = [];

let memoryState: State = { errors: [] };

function dispatch(action: Action) {
  memoryState = reducer(memoryState, action);
  listeners.forEach(listener => {
    listener(memoryState);
  });
}

type ErrorData = Omit<ErrorSheetData, 'id' | 'open'>;

function showError({ ...props }: ErrorData) {
  const id = genId();

  const hide = () => dispatch({ type: 'HIDE_ERROR', errorId: id });
  const clear = () => dispatch({ type: 'CLEAR_ERROR', errorId: id });

  dispatch({
    type: 'SHOW_ERROR',
    error: {
      ...props,
      id,
    },
  });

  return {
    id,
    hide,
    clear,
  };
}

function useErrorSheet() {
  const [state, setState] = React.useState<State>(memoryState);

  React.useEffect(() => {
    listeners.push(setState);

    return () => {
      const index = listeners.indexOf(setState);
      if (index > -1) {
        listeners.splice(index, 1);
      }
    };
  }, []);

  return {
    ...state,
    showError,
    hideError: (errorId: string) => dispatch({ type: 'HIDE_ERROR', errorId }),
    clearError: (errorId: string) => dispatch({ type: 'CLEAR_ERROR', errorId }),
  };
}

export { useErrorSheet, showError };
