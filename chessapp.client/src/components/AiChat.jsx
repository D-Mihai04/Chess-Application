import React, { useState, useRef, useEffect } from 'react';
import './AiChat.css';

export default function AiChat({ onSendMessage, disabled }) {
  const [input,    setInput]    = useState('');
  const [messages, setMessages] = useState([
    { role: 'ai', text: '♟ Hello! I\'m your chess assistant. Tell me a move like "move pawn to e4" or "knight to f3" and I\'ll execute it for you!' }
  ]);
  const [loading, setLoading] = useState(false);
  const bottomRef = useRef(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const send = async () => {
    if (!input.trim() || loading || disabled) return;
    const userMsg = input.trim();
    setInput('');
    setMessages(m => [...m, { role: 'user', text: userMsg }]);
    setLoading(true);
    try {
      const result = await onSendMessage(userMsg);
      const reply = result?.reply || 'Move processed!';
      const extra = result?.moveResult?.valid === false ? ` (${result.moveResult.message})` : '';
      setMessages(m => [...m, { role: 'ai', text: reply + extra }]);
    } catch {
      setMessages(m => [...m, { role: 'ai', text: 'Sorry, I had trouble processing that.' }]);
    } finally {
      setLoading(false);
    }
  };

  const onKey = (e) => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); send(); } };

  return (
    <div className="chat">
      <div className="chat-header">
        <span className="chat-title">♜ AI Assistant</span>
        <span className="chat-hint">Ask me to make moves</span>
      </div>
      <div className="chat-messages">
        {messages.map((m, i) => (
          <div key={i} className={`chat-msg chat-msg-${m.role}`}>
            {m.role === 'ai' && <span className="chat-avatar">✦</span>}
            <span className="chat-text">{m.text}</span>
          </div>
        ))}
        {loading && (
          <div className="chat-msg chat-msg-ai">
            <span className="chat-avatar">✦</span>
            <span className="chat-thinking">
              <span/>
              <span/>
              <span/>
            </span>
          </div>
        )}
        <div ref={bottomRef} />
      </div>
      <div className="chat-input-row">
        <input
          className="chat-input"
          placeholder={disabled ? 'Start a game first…' : 'e.g. "move pawn to e4"'}
          value={input}
          onChange={e => setInput(e.target.value)}
          onKeyDown={onKey}
          disabled={disabled || loading}
        />
        <button className="chat-send" onClick={send} disabled={disabled || loading || !input.trim()}>
          ➤
        </button>
      </div>
    </div>
  );
}
