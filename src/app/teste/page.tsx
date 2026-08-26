'use client';

import { useState } from 'react';
import { consultarRiscoAlunos, RespostaPredicao } from '@/services/api-evasao';

export default function Home() {
  const [resultados, setResultados] = useState<RespostaPredicao[]>([]);
  const [carregando, setCarregando] = useState(false);

  const alunosParaTeste = [
    {
      id: 'ALUNO-01',
      media_notas: 9.2,
      taxa_aprovacao: 1.0,
      total_ch_cumprida: 450,
      total_reprovacoes_pendencias: 0,
      percentual_conclusao_curso: 0.6
    },
    {
      id: 'ALUNO-02',
      media_notas: 6.5,
      taxa_aprovacao: 0.7,
      total_ch_cumprida: 200,
      total_reprovacoes_pendencias: 1,
      percentual_conclusao_curso: 0.9
    },
    {
      id: 'ALUNO-03',
      media_notas: 4.5,
      taxa_aprovacao: 0.4,
      total_ch_cumprida: 120,
      total_reprovacoes_pendencias: 3,
      percentual_conclusao_curso: 0.25
    },
    {
      id: 'ALUNO-04',
      media_notas: 2.8,
      taxa_aprovacao: 0.2,
      total_ch_cumprida: 60,
      total_reprovacoes_pendencias: 6,
      percentual_conclusao_curso: 0.15
    }
  ];

  const testarPredicao = async () => {
    setCarregando(true);
    try {
      const res = await consultarRiscoAlunos(alunosParaTeste);
      setResultados(res);
    } catch (error) {
      alert('Erro ao consultar o modelo de IA');
      console.error(error);
    } finally {
      setCarregando(false);
    }
  };

  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-8 bg-gray-50">
      <h1 className="text-3xl font-bold mb-6 text-gray-800">
        Previsão de Evasão Escolar
      </h1>

      <button
        onClick={testarPredicao}
        disabled={carregando}
        className="bg-blue-600 hover:bg-blue-700 text-white font-bold py-3 px-6 rounded-lg shadow transition-colors"
      >
        {carregando ? 'Calculando...' : 'Calcular risco dos alunos'}
      </button>

      {resultados.length > 0 && (
        <div className="mt-8 grid grid-cols-1 md:grid-cols-2 gap-4 w-full max-w-4xl">
          {resultados.map((res) => (
            <div
              key={res.id}
              className="p-5 bg-white border rounded-xl shadow-sm flex flex-col justify-between"
            >
              <div>
                <h3 className="font-bold text-lg text-gray-700">{res.id}</h3>
                <p className="text-2xl font-extrabold mt-2 text-gray-900">
                  {res.probabilidade_evasao_percent}%
                  <span className="text-xs font-normal text-gray-500 ml-1">
                    chance de evasão
                  </span>
                </p>
              </div>

              <div className="mt-4">
                <span
                  className={`inline-block px-3 py-1 rounded-full text-xs font-bold text-white ${
                    res.risco_alto ? 'bg-red-500' : 'bg-green-500'
                  }`}
                >
                  {res.risco_alto ? 'Risco Alto' : 'Risco Baixo'}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </main>
  );
}