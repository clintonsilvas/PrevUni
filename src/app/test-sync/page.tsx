"use client";

export default function TestSync() {
  async function sincronizar() {    
    await fetch("/api/sync", {
        method:"POST"
    });      
  }

  return (
    <div style={{ padding: 20 }}>
      <button onClick={sincronizar}>
        Sincronizar
      </button>
    </div>
  );
}