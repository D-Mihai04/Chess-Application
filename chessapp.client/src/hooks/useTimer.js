import { useState, useEffect, useRef, useCallback } from 'react';

export function useTimer(initialSeconds = 600) {
  const [whiteTime, setWhiteTime] = useState(initialSeconds);
  const [blackTime, setBlackTime] = useState(initialSeconds);
  const [active,    setActive]    = useState(false);
  const [turn,      setTurn]      = useState('white');
  const [timedOut,  setTimedOut]  = useState(false);
  const intervalRef = useRef(null);

  const tick = useCallback(() => {
    if (turn === 'white') {
      setWhiteTime(t => {
        if (t <= 1) { setActive(false); setTimedOut(true); return 0; }
        return t - 1;
      });
    } else {
      setBlackTime(t => {
        if (t <= 1) { setActive(false); setTimedOut(true); return 0; }
        return t - 1;
      });
    }
  }, [turn]);

  useEffect(() => {
    if (active) {
      intervalRef.current = setInterval(tick, 1000);
    } else {
      clearInterval(intervalRef.current);
    }
    return () => clearInterval(intervalRef.current);
  }, [active, tick]);

  const start  = useCallback(() => setActive(true),  []);
  const pause  = useCallback(() => setActive(false), []);
  const reset  = useCallback((newTime) => {
    setActive(false);
    setTimedOut(false);
    setWhiteTime(newTime ?? initialSeconds);
    setBlackTime(newTime ?? initialSeconds);
    setTurn('white');
  }, [initialSeconds]);

  const switchTurn = useCallback((newTurn) => setTurn(newTurn), []);

  const fmt = (s) => {
    const m = Math.floor(s / 60).toString().padStart(2, '0');
    const sec = (s % 60).toString().padStart(2, '0');
    return `${m}:${sec}`;
  };

  return { whiteTime, blackTime, active, fmt, start, pause, reset, switchTurn, timedOut };
}