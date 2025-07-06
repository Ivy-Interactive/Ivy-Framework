import { useState, useEffect, useMemo, useCallback } from 'react';

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

  const [current, setCurrent] = useState(initialMatrix);
  const [history, setHistory] = useState([initialMatrix]);
  const [forwardStepCount, setForwardStepCount] = useState(0);
  const [isReverting, setIsReverting] = useState(false);
  const [isPaused, setIsPaused] = useState(true); // Start paused
  const [lastMutatedIndices, setLastMutatedIndices] = useState(
    new Set<number>()
  );

  const mutableCellIndices = useMemo(() => {
    return initialMatrix
      .map((cell, index) => (cell[0] !== 0 ? index : -1))
      .filter(index => index !== -1);
  }, [initialMatrix]);

  const performStep = useCallback(() => {
    if (!isReverting) {
      // MUTATING FORWARD
      if (forwardStepCount < MUTATION_STEPS_FORWARD) {
        const availableForMutationThisStep = mutableCellIndices.filter(
          idx => !lastMutatedIndices.has(idx)
        );

        if (
          availableForMutationThisStep.length === 0 &&
          mutableCellIndices.length > 0
        ) {
          // All mutable cells were mutated in the last step. Skip mutation this turn.
          setCurrent(prevCurrent => {
            setHistory(prevHistory => [...prevHistory, prevCurrent]); // Add current (unchanged) state to history
            return prevCurrent; // No change to current
          });
          setLastMutatedIndices(new Set()); // Reset for the next step, allowing any mutable cell
          setForwardStepCount(prevCount => prevCount + 1);
          return; // Exit performStep for this tick
        }

        const cellsToMutateCount = Math.min(
          CELLS_TO_MUTATE_PER_STEP,
          availableForMutationThisStep.length
        );

        const indicesMutatedThisStep = new Set<number>();
        const pickableIndices = [...availableForMutationThisStep];

        while (
          indicesMutatedThisStep.size < cellsToMutateCount &&
          pickableIndices.length > 0
        ) {
          const randomIndexInPickable = Math.floor(
            Math.random() * pickableIndices.length
          );
          const actualCellIndex = pickableIndices[randomIndexInPickable];
          indicesMutatedThisStep.add(actualCellIndex);
          pickableIndices.splice(randomIndexInPickable, 1); // Ensure unique picks for this step
        }

        if (indicesMutatedThisStep.size > 0) {
          setCurrent(prevCurrent => {
            const nextMatrix = prevCurrent.map((cell, index) => {
              if (indicesMutatedThisStep.has(index)) {
                const newShapeIndex = getWeightedRandomIndex(SHAPE_LIKELIHOODS);
                const newColorIndex = getWeightedRandomIndex(COLOR_LIKELIHOODS);
                const newRotation = Math.floor(Math.random() * 4);
                return [newShapeIndex, newColorIndex, newRotation];
              }
              return cell;
            });
            setHistory(prevHistory => [...prevHistory, nextMatrix]);
            return nextMatrix;
          });
          setLastMutatedIndices(indicesMutatedThisStep);
        } else {
          // No cells were actually mutated (e.g., if availableForMutationThisStep was empty initially and mutableCellIndices was also empty)
          setCurrent(prevCurrent => {
            setHistory(prevHistory => [...prevHistory, prevCurrent]);
            return prevCurrent;
          });
          setLastMutatedIndices(new Set());
        }
        setForwardStepCount(prevCount => prevCount + 1);
      } else {
        // Reached MUTATION_STEPS_FORWARD forward steps, switch to reverting
        setIsReverting(true);
        setLastMutatedIndices(new Set()); // Reset for when/if we go forward again
      }
    } else {
      // REVERTING
      if (history.length > 1) {
        const newHistory = history.slice(0, -1);
        setHistory(newHistory);
        setCurrent(newHistory[newHistory.length - 1]);

        if (newHistory.length === 1) {
          // Just reverted to initialMatrix
          setIsReverting(false);
          setForwardStepCount(0);
          setIsPaused(true); // Trigger pause
          setLastMutatedIndices(new Set()); // Reset for the new cycle
        }
      }
    }
  }, [
    isReverting,
    forwardStepCount,
    history,
    mutableCellIndices,
    lastMutatedIndices,
    setIsReverting,
    setForwardStepCount,
    setIsPaused,
    setHistory,
    setCurrent,
    setLastMutatedIndices,
  ]);

  useEffect(() => {
    if (isPaused) {
      const timeoutId = setTimeout(() => {
        setIsPaused(false);
      }, PAUSE_DURATION_MS);
      return () => clearTimeout(timeoutId);
    } else {
      const intervalId = setInterval(performStep, STEP_INTERVAL_MS);
      return () => clearInterval(intervalId);
    }
  }, [isPaused, performStep]); // STEP_INTERVAL_MS is a const, not needed here if defined outside component scope

  return (
    <div className="grid grid-cols-5 gap-0 w-fit">
      {current.map((item, index) => {
        const [shapeIndex, colorIndex, rotation] = item;
        const shape = SHAPES[shapeIndex];
        const color = COLORS[colorIndex] || COLORS[0];

        return (
          <div key={index} className="w-6 h-6">
            {shape && (
              <svg
                viewBox="0 0 28 28"
                style={{
                  transform: `rotate(${rotation * 90}deg)`,
                  transitionProperty: 'transform',
                  transitionDuration: '0.2s',
                  transitionTimingFunction: 'ease-in-out',
                }}
              >
                <path
                  d={shape}
                  fill={color}
                  style={{
                    transitionProperty: 'fill',
                    transitionDuration: '0.3s',
                    transitionTimingFunction: 'ease-in-out',
                  }}
                />
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
