'use client';

import { useState } from 'react';
import { consultarRiscoAluno, RespostaPredicao } from '@/services/api-evasao';

export default function Home() {
  const [resultado, setResultado] = useState<RespostaPredicao | null>(null);
  const [carregando, setCarregando] = useState(false);

  const testarPredicao = async () => {
    setCarregando(true);
    try {
      const dados = {
        media_notas: 5.5,
        taxa_aprovacao: 0.6,
        total_ch_cumprida: 120,
        total_reprovacoes_pendencias: 3,
        percentual_conclusao_curso: 0.3
      };

      const res = await consultarRiscoAluno(dados);
      setResultado(res);
    } catch (error) {
      alert('Erro ao consultar o modelo de IA');
      console.error(error);
    } finally {
      setCarregando(false);
    }
  };

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-24">
      <h1 className="text-3xl font-bold mb-6">Previsão de Evasão Escolar</h1>

      <button
        onClick={testarPredicao}
        disabled={carregando}
        className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"
      >
        {carregando ? 'Calculando...' : 'Calcular risco do aluno'}
      </button>

      {resultado && (
        <div className="mt-6 p-4 border rounded shadow-lg text-center">
          <p className="text-xl">
            Probabilidade de Evasão: <strong>{resultado.probabilidade_evasao_percent}%</strong>
          </p>
          <span className={`inline-block mt-2 px-3 py-1 rounded text-white font-semibold ${
            resultado.risco_alto ? 'bg-red-500' : 'bg-green-500'
          }`}>
            {resultado.risco_alto ? 'Risco Alto' : 'Risco Baixo'}
          </span>
        </div>
      )}
    </main>
  );
}