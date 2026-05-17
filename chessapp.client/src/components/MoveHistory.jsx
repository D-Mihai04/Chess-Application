import React, { useRef, useEffect } from 'react';
import './MoveHistory.css';

export default function MoveHistory({ moves }) {
  const bottomRef = useRef(null);
  useEffect(() => { bottomRef.current?.scrollIntoView({ behavior: 'smooth' }); }, [moves]);

  // Group into pairs (white, black)
  const pairs = [];
  for (let i = 0; i < moves.length; i += 2) {
    pairs.push({ num: Math.floor(i/2)+1, white: moves[i], black: moves[i+1] });
  }

  return (
    <div className="history">
      <div className="history-header">Move History</div>
      <div className="history-body">
        {pairs.length === 0 && <span className="history-empty">No moves yet</span>}
        {pairs.map(p => (
          <div key={p.num} className="history-row">
            <span className="history-num">{p.num}.</span>
            <span className="history-move">{p.white}</span>
            <span className="history-move history-move-black">{p.black ?? ''}</span>
          </div>
        ))}
        <div ref={bottomRef} />
      </div>
    </div>
  );
}
