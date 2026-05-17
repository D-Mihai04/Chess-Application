import { useState, useCallback } from 'react';
import { api } from '../api.js';

export function useChessGame() {
  const [gameId,       setGameId]       = useState(null);
  const [pieces,       setPieces]       = useState([]);
  const [currentTurn,  setCurrentTurn]  = useState('white');
  const [status,       setStatus]       = useState('idle'); // idle | playing | check | checkmate | stalemate
  const [selectedSq,   setSelectedSq]   = useState(null);
  const [validMoves,   setValidMoves]   = useState([]);
  const [moveHistory,  setMoveHistory]  = useState([]);
  const [lastMove,     setLastMove]     = useState(null);
  const [loading,      setLoading]      = useState(false);
  const [error,        setError]        = useState(null);
  const [whiteCaptured, setWhiteCaptured] = useState([]);
  const [blackCaptured, setBlackCaptured] = useState([]);

  const applyState = useCallback((state) => {
    setPieces(state.pieces);
    setCurrentTurn(state.currentTurn);
    setStatus(state.status);
    setMoveHistory(state.moveHistory);
    setGameId(state.gameId);
    setWhiteCaptured(state.whiteCaptured || []);
    setBlackCaptured(state.blackCaptured || []);
    console.log('whiteCaptured:', state.whiteCaptured);
    console.log('blackCaptured:', state.blackCaptured);
  }, []);

  const startGame = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const state = await api.newGame();
      applyState(state);
      console.log('new game whiteCaptured length:', state.whiteCaptured?.length);
console.log('new game blackCaptured length:', state.blackCaptured?.length);
      setSelectedSq(null);
      setValidMoves([]);
      setLastMove(null);
    } catch (e) {
      setError('Could not connect to server. Is the backend running?');
    } finally {
      setLoading(false);
    }
  }, [applyState]);

  const selectSquare = useCallback(async (row, col) => {
    if (!gameId || status === 'checkmate' || status === 'stalemate') return;

    const clickedPiece = pieces.find(p => p.row === row && p.col === col);

    // If a piece is already selected
    if (selectedSq) {
      const isValidTarget = validMoves.some(m => m.row === row && m.col === col);

      if (isValidTarget) {
        // Execute move
        setLoading(true);
        try {
          const result = await api.makeMove({
            gameId,
            fromRow: selectedSq.row,
            fromCol: selectedSq.col,
            toRow: row,
            toCol: col,
          });
          if (result.valid) {
            setLastMove({ fromRow: selectedSq.row, fromCol: selectedSq.col, toRow: row, toCol: col });
            applyState(result.newState);
          }
        } finally {
          setLoading(false);
          setSelectedSq(null);
          setValidMoves([]);
        }
        return;
      }

      // Clicked own piece, switch selection
      if (clickedPiece && clickedPiece.color === currentTurn) {
        await fetchValidMoves(row, col);
        return;
      }

      // Clicked empty or enemy without valid move, deselect
      setSelectedSq(null);
      setValidMoves([]);
      return;
    }

    // Nothing selected yet, select own piece
    if (clickedPiece && clickedPiece.color === currentTurn) {
      await fetchValidMoves(row, col);
    }
  }, [gameId, selectedSq, validMoves, pieces, currentTurn, status, applyState]);

  async function fetchValidMoves(row, col) {
    setSelectedSq({ row, col });
    try {
      const result = await api.getValidMoves({ gameId, row, col });
      setValidMoves(result.moves.map(m => ({ row: m[0], col: m[1] })));
    } catch {
      setValidMoves([]);
    }
  }

  const sendAiMessage = useCallback(async (message) => {
    if (!gameId) return null;
    const result = await api.aiMove({ gameId, message });
    if (result.moveResult?.valid && result.moveResult.newState) {
      applyState(result.moveResult.newState);
      setSelectedSq(null);
      setValidMoves([]);
    }
    return result;
  }, [gameId, applyState]);

  return {
    gameId, pieces, currentTurn, status,
    selectedSq, validMoves, moveHistory, lastMove,
    loading, error,
    startGame, selectSquare, sendAiMessage,whiteCaptured, blackCaptured,
  };
}
