const BASE = '/api/chess';

async function post(url, body) {
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

async function get(url) {
  const res = await fetch(url);
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.json();
}

export const api = {
  newGame:       ()      => post(`${BASE}/new`, {}),
  getGame:       (id)    => get(`${BASE}/${id}`),
  makeMove:      (body)  => post(`${BASE}/move`, body),
  getValidMoves: (body)  => post(`${BASE}/valid-moves`, body),
  aiMove:        (body)  => post(`${BASE}/ai-move`, body),
};
