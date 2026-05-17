import React from 'react';
import './CapturedPieces.css';

const SYMBOLS = {
  white: { king:'♔', queen:'♕', rook:'♖', bishop:'♗', knight:'♘', pawn:'♙' },
  black: { king:'♚', queen:'♛', rook:'♜', bishop:'♝', knight:'♞', pawn:'♟' },
};

export default function CapturedPieces({ pieces, label }) {
  return (
    <div className="captured">
      <span className="captured-label">{label}</span>
      <div className="captured-pieces">
        {pieces.length === 0 && <span className="captured-empty">none</span>}
        {pieces.map((p, i) => (
          <span key={i} className={`captured-piece captured-${p.color}`}>
            {SYMBOLS[p.color][p.type]}
          </span>
        ))}
      </div>
    </div>
  );
}