"use client";

import { useState } from "react";

export default function Login() {
  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    console.log({ email, senha });
  };

  return (
    <div className="min-h-screen flex bg-gray-100">
      {/* Lado esquerdo */}
      <div className="hidden md:flex w-1/2 bg-blue-600 items-center justify-center">
        <img
          src="/logo.svg"
          alt="PrevUni"
          className="w-72 object-contain"
        />
      </div>

      {/* Lado direito */}
      <div className="w-full md:w-1/2 flex items-center justify-center px-6">
        <div className="bg-white shadow-lg rounded-xl p-10 w-full max-w-md">
          <h1 className="text-3xl font-bold text-center mb-2">Entrar</h1>
          <p className="text-gray-500 text-center mb-8">
            Acesse sua conta do PrevUni
          </p>

          <form onSubmit={handleLogin} className="space-y-5">
            <div>
              <label className="block mb-2 font-medium">Email</label>
              <input
                type="email"
                placeholder="email@exemplo.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full border rounded-lg px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-600"
                required
              />
            </div>

            <div>
              <label className="block mb-2 font-medium">Senha</label>
              <input
                type="password"
                placeholder="********"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                className="w-full border rounded-lg px-4 py-3 focus:outline-none focus:ring-2 focus:ring-blue-600"
                required
              />
            </div>

            <button
              type="submit"
              className="w-full bg-blue-600 hover:bg-blue-700 text-white py-3 rounded-lg font-semibold transition"
            >
              Entrar
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}