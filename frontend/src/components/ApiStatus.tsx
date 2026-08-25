import { useState } from 'react';

export default function ApiStatus() {
  const [configured] = useState(!!import.meta.env.VITE_API_URL);

  if (!configured) {
    return (
      <div className="fixed bottom-4 right-4 z-50 bg-amber-900/80 text-amber-200 text-xs px-3 py-2 rounded-lg border border-amber-700/50">
        Using mock data. Set VITE_API_URL for live API.
      </div>
    );
  }
  return null;
}