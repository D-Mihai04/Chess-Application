import React from 'react';
import './Timer.css';

export default function Timer({ label, time, active, fmt }) {
  const urgent = time <= 30;
  return (
    <div className={`timer ${active ? 'timer-active' : ''} ${urgent ? 'timer-urgent' : ''}`}>
      <span className="timer-label">{label}</span>
      <span className="timer-time">{fmt(time)}</span>
    </div>
  );
}
