import { useMemo, useEffect, useReducer } from 'react';

const SHAPES = [
  undefined,
  'M0 0 H28 V28 H0 Z', // Square
  'M28 0H0V28C15.46 28 28 15.46 28 0Z', // Perfect quarter circle
];
const SHAPE_LIKELIHOODS = [0, 0.4, 0.6];
const COLORS = ['#00CC92', '#0D4A2F', '#80E6C9'];
const COLOR_LIKELIHOODS = [0.6, 0.2, 0.2];
const STEP_INTERVAL_MS = 200;
const CELLS_TO_MUTATE_PER_STEP = 2;
const MUTATION_STEPS_FORWARD = 6;
const PAUSE_DURATION_MS = 1000;

type LoadingState = {
  current: number[][];
  history: number[][][];
  forwardStepCount: number;
  isReverting: boolean;
  isPaused: boolean;
  lastMutatedIndices: Set<number>;
};

type LoadingAction = { type: 'TICK' } | { type: 'PAUSE_COMPLETE' };

const reducer = (state: LoadingState, action: LoadingAction): LoadingState => {
  switch (action.type) {
    case 'PAUSE_COMPLETE':
      return { ...state, isPaused: false };
    case 'TICK': {
      if (!state.isReverting) {
        if (state.forwardStepCount < MUTATION_STEPS_FORWARD) {
          const mutableCellIndices = state.current
            .map((cell, index) => (cell[0] !== 0 ? index : -1))
            .filter(index => index !== -1);

          const availableForMutation = mutableCellIndices.filter(
            idx => !state.lastMutatedIndices.has(idx)
          );

          if (
            availableForMutation.length === 0 &&
            mutableCellIndices.length > 0
          ) {
            return {
              ...state,
              history: [...state.history, state.current],
              forwardStepCount: state.forwardStepCount + 1,
              lastMutatedIndices: new Set<number>(),
            };
          }

          const cellsToMutateCount = Math.min(
            CELLS_TO_MUTATE_PER_STEP,
            availableForMutation.length
          );
          const mutatedThisStep = new Set<number>();
          const pickable = [...availableForMutation];

          while (
            mutatedThisStep.size < cellsToMutateCount &&
            pickable.length > 0
          ) {
            const rIdx = Math.floor(Math.random() * pickable.length);
            mutatedThisStep.add(pickable[rIdx]);
            pickable.splice(rIdx, 1);
          }

          if (mutatedThisStep.size > 0) {
            const nextMatrix = state.current.map((cell, index) => {
              if (mutatedThisStep.has(index)) {
                return [
                  getWeightedRandomIndex(SHAPE_LIKELIHOODS),
                  getWeightedRandomIndex(COLOR_LIKELIHOODS),
                  Math.floor(Math.random() * 4),
                ];
              }
              return cell;
            });
            return {
              ...state,
              current: nextMatrix,
              history: [...state.history, nextMatrix],
              forwardStepCount: state.forwardStepCount + 1,
              lastMutatedIndices: mutatedThisStep,
            };
          }
          return { ...state, forwardStepCount: state.forwardStepCount + 1 };
        }
        return {
          ...state,
          isReverting: true,
          lastMutatedIndices: new Set<number>(),
        };
      } else {
        if (state.history.length > 1) {
          const newHistory = state.history.slice(0, -1);
          const nextCurrent = newHistory[newHistory.length - 1];
          if (newHistory.length === 1) {
            return {
              ...state,
              current: nextCurrent,
              history: newHistory,
              isReverting: false,
              forwardStepCount: 0,
              isPaused: true,
              lastMutatedIndices: new Set<number>(),
            };
          }
          return { ...state, current: nextCurrent, history: newHistory };
        }
        return state;
      }
    }
    default:
      return state;
  }
};

export function Loading() {
  const initialMatrix = useMemo(
    () => [
      [2, 0, 0],
      [0, 0, 0],
      [0, 0, 0],
      [0, 0, 0],
      [0, 0, 0],
      [1, 0, 0],
      [2, 0, 3],
      [2, 0, 2],
      [2, 0, 3],
      [2, 0, 2],
      [1, 0, 0],
      [2, 0, 1],
      [2, 0, 0],
      [2, 0, 1],
      [1, 0, 0],
      [0, 0, 0],
      [0, 0, 0],
      [0, 0, 0],
      [2, 0, 1],
      [2, 0, 0],
    ],
    []
  );

  const [state, dispatch] = useReducer(reducer, {
    current: initialMatrix,
    history: [initialMatrix],
    forwardStepCount: 0,
    isReverting: false,
    isPaused: true,
    lastMutatedIndices: new Set<number>(),
  });

  useEffect(() => {
    if (state.isPaused) {
      const id = setTimeout(
        () => dispatch({ type: 'PAUSE_COMPLETE' }),
        PAUSE_DURATION_MS
      );
      return () => clearTimeout(id);
    } else {
      const id = setInterval(
        () => dispatch({ type: 'TICK' }),
        STEP_INTERVAL_MS
      );
      return () => clearInterval(id);
    }
  }, [state.isPaused]);

  return (
    <div className="grid grid-cols-5 gap-0 w-fit">
      {state.current.map((item: number[], index: number) => {
        const [shapeIndex, colorIndex, rotation] = item;
        const shape = SHAPES[shapeIndex];
        const color = COLORS[colorIndex] || COLORS[0];

        return (
          <div
            key={`loading-cell-${index}`}
            className="w-7 h-7"
            style={{ margin: '-0.5px' }}
          >
            {shape && (
              <svg
                viewBox="0 0 28 28"
                className="w-full h-full"
                style={{
                  transform: `rotate(${rotation * 90}deg)`,
                  transition:
                    'transform 0.2s ease-in-out, fill 0.3s ease-in-out',
                  display: 'block',
                }}
              >
                <path d={shape} fill={color} />
              </svg>
            )}
          </div>
        );
      })}
    </div>
  );
}

const getWeightedRandomIndex = (likelihoods: number[]): number => {
  const sumOfLikelihoods = likelihoods.reduce((sum, L) => sum + L, 0);
  let randomNum = Math.random() * sumOfLikelihoods;

  for (let i = 0; i < likelihoods.length; i++) {
    if (randomNum < likelihoods[i]) {
      return i;
    } else {
      randomNum -= likelihoods[i];
    }
  }
  return likelihoods.length - 1;
};
