import React from 'react';
import './ChessBoard.css';

const PIECE_IMAGES = {
  white: {
    king:   'https://lichess1.org/assets/piece/cburnett/wK.svg',
    queen:  'https://lichess1.org/assets/piece/cburnett/wQ.svg',
    rook:   'https://lichess1.org/assets/piece/cburnett/wR.svg',
    bishop: 'https://lichess1.org/assets/piece/cburnett/wB.svg',
    knight: 'https://lichess1.org/assets/piece/cburnett/wN.svg',
    pawn:   'https://lichess1.org/assets/piece/cburnett/wP.svg',
  },
  black: {
    king:   'https://lichess1.org/assets/piece/cburnett/bK.svg',
    queen:  'https://lichess1.org/assets/piece/cburnett/bQ.svg',
    rook:   'https://lichess1.org/assets/piece/cburnett/bR.svg',
    bishop: 'https://lichess1.org/assets/piece/cburnett/bB.svg',
    knight: 'https://lichess1.org/assets/piece/cburnett/bN.svg',
    pawn:   'https://lichess1.org/assets/piece/cburnett/bP.svg',
  },
};

const FILES = ['a','b','c','d','e','f','g','h'];
const RANKS = ['8','7','6','5','4','3','2','1'];


export default function ChessBoard({ pieces, selectedSq, validMoves, lastMove, status, currentTurn, onSquareClick }) {
  // Build lookup map for O(1) piece access
  const pieceMap = {};
  pieces.forEach(p => { pieceMap[`${p.row},${p.col}`] = p; });

  // Find king in check
  let checkKingPos = null;
  if (status === 'check' || status === 'checkmate') {
    const king = pieces.find(p => p.type === 'king' && p.color === currentTurn);
    if (king) checkKingPos = `${king.row},${king.col}`;
  }

  const isSelected  = (r,c) => selectedSq?.row === r && selectedSq?.col === c;
  const isValidMove = (r,c) => validMoves.some(m => m.row === r && m.col === c);
  const isLastMove  = (r,c) => lastMove && (
    (lastMove.fromRow===r && lastMove.fromCol===c) ||
    (lastMove.toRow  ===r && lastMove.toCol  ===c));
  const isCapture   = (r,c) => isValidMove(r,c) && !!pieceMap[`${r},${c}`];

  return (
    <div className="board-wrap">
      {/* Rank labels */}
      <div className="rank-labels">
        {RANKS.map(r => <span key={r}>{r}</span>)}
      </div>

      <div className="board">
        {Array.from({length:8}, (_,row) =>
          Array.from({length:8}, (_,col) => {
            const key = `${row},${col}`;
            const piece = pieceMap[key];
            const light = (row+col)%2===0;
            const classes = [
              'square',
              light ? 'light' : 'dark',
              isSelected(row,col)  ? 'selected'  : '',
              isLastMove(row,col)  ? 'last-move'  : '',
              checkKingPos===key   ? 'in-check'   : '',
              isCapture(row,col)   ? 'valid-cap'  : '',
            ].filter(Boolean).join(' ');

            return (
              <div key={key} className={classes} onClick={() => onSquareClick(row,col)}>
                {isValidMove(row,col) && !isCapture(row,col) && (
                  <div className="valid-dot" />
                )}
                {piece && (() => {
                    const isMoving = lastMove && lastMove.toRow === row && lastMove.toCol === col;
                    const dx = isMoving ? (lastMove.fromCol - lastMove.toCol) * 100 : 0;
                    const dy = isMoving ? (lastMove.fromRow - lastMove.toRow) * 100 : 0;
                    return (
                      <img
                        src={PIECE_IMAGES[piece.color][piece.type]}
                        className={`piece ${isSelected(row,col) ? 'piece-selected' : ''} ${isMoving ? 'piece-moving' : ''}`}
                        style={isMoving ? { '--dx': `${dx}%`, '--dy': `${dy}%` } : {}}
                        title={`${piece.color} ${piece.type}`}
                        draggable={false}
                      />
                    );
                  })()}
              </div>
            );
          })
        )}
      </div>

      {/* File labels */}
      <div className="file-labels">
        {FILES.map(f => <span key={f}>{f}</span>)}
      </div>
    </div>
  );
}
