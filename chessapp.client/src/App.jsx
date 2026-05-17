import React, { useEffect, useState } from 'react';
import ChessBoard   from './components/ChessBoard.jsx';
import Timer        from './components/Timer.jsx';
import AiChat       from './components/AiChat.jsx';
import MoveHistory  from './components/MoveHistory.jsx';
import CapturedPieces from './components/CapturedPieces.jsx';
import { useChessGame } from './hooks/useChessGame.js';
import { useTimer }     from './hooks/useTimer.js';
import './App.css';

export default function App() {
  const game = useChessGame();
  const [selectedTime, setSelectedTime] = useState(600);
  const timer = useTimer(selectedTime);

  useEffect(() => {
    if (game.status === 'playing' || game.status === 'check') {
      timer.switchTurn(game.currentTurn);
      timer.start();
    } else if (game.status === 'checkmate' || game.status === 'stalemate') {
      timer.pause();
    }
  }, [game.currentTurn, game.status]);

  useEffect(() => {
    if (timer.timedOut) {
      timer.pause();
    }
  }, [timer.timedOut]);

  const handleNewGame = async () => {
    await game.startGame();
    timer.reset(selectedTime);
  };

  const statusBanner = () => {
    if (!game.gameId) return null;
    if (timer.timedOut) {
      const loser = timer.whiteTime === 0 ? 'White' : 'Black';
      return `⏱ Time's up! ${loser} loses!`;
    }
    const s = game.status;
    if (s === 'checkmate') return `♚ Checkmate! ${game.currentTurn === 'white' ? 'Black' : 'White'} wins!`;
    if (s === 'stalemate') return '½ Stalemate – Draw!';
    if (s === 'check')     return `⚠ ${game.currentTurn.charAt(0).toUpperCase() + game.currentTurn.slice(1)} is in check!`;
    return null;
  };

  const banner = statusBanner();

  return (
    <div className="app">
      <header className="app-header">
        <h1 className="app-title">♔ Chess <span>App</span></h1>
        <div className="app-controls">
          <select
            className="time-select"
            value={selectedTime}
            onChange={e => setSelectedTime(Number(e.target.value))}
          >
            <option value={60}>1 min — Bullet</option>
            <option value={180}>3 min — Blitz</option>
            <option value={300}>5 min — Blitz</option>
            <option value={600}>10 min — Rapid</option>
          </select>
          <button
            className="btn-new"
            onClick={handleNewGame}
            disabled={game.loading}
          >
            {game.loading ? '…' : game.gameId ? '↺ New Game' : '▶ Start Game'}
          </button>
        </div>
      </header>

      {game.error && (
        <div className="error-banner">⚠ {game.error}</div>
      )}

      {banner && (
        <div className={`status-banner ${timer.timedOut ? 'checkmate' : game.status}`}>{banner}</div>
      )}

      <main className="app-main">
        <aside className="side-panel">
          <Timer label="Black ♚" time={timer.blackTime} active={timer.active && game.currentTurn === 'black'} fmt={timer.fmt} />
          <CapturedPieces pieces={game.whiteCaptured} label="White Captured" />
          <MoveHistory moves={game.moveHistory} />
          <CapturedPieces pieces={game.blackCaptured} label="Black Captured" />
          <Timer label="White ♔" time={timer.whiteTime} active={timer.active && game.currentTurn === 'white'} fmt={timer.fmt} />
        </aside>

        <section className="board-section">
          {!game.gameId ? (
            <div className="board-placeholder">
              <div className="placeholder-icon">♛</div>
              <p>Press <strong>Start Game</strong> to begin</p>
            </div>
          ) : (
            <>
              <div className="turn-indicator">
                <span className={`turn-dot ${game.currentTurn}`} />
                {game.currentTurn.charAt(0).toUpperCase() + game.currentTurn.slice(1)}'s turn
              </div>
              <ChessBoard
                pieces={game.pieces}
                selectedSq={game.selectedSq}
                validMoves={game.validMoves}
                lastMove={game.lastMove}
                status={game.status}
                currentTurn={game.currentTurn}
                onSquareClick={timer.timedOut ? () => {} : game.selectSquare}
              />
            </>
          )}
        </section>

        <aside className="side-panel">
          <AiChat onSendMessage={game.sendAiMessage} disabled={!game.gameId} />
        </aside>
      </main>

      <footer className="app-footer">Chess App</footer>
    </div>
  );
}