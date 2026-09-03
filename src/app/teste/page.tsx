'use client';

import { useState, ChangeEvent } from 'react';
import { consultarRiscoAlunos, DadosAlunoInput, RespostaPredicao } from '@/services/api-evasao';

export default function Home() {
  const [resultados, setResultados] = useState<RespostaPredicao[]>([]);
  const [carregando, setCarregando] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [jsonPreview, setJsonPreview] = useState<DadosAlunoInput[] | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    setErro(null);
    setResultados([]);

    const selectedFile = e.target.files?.[0];
    if (!selectedFile) {
      setFile(null);
      setJsonPreview(null);
      return;
    }

    if (selectedFile.type !== 'application/json' && !selectedFile.name.endsWith('.json')) {
      setErro('Por favor, selecione um arquivo no formato .json');
      setFile(null);
      setJsonPreview(null);
      return;
    }

    setFile(selectedFile);

    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const parsed = JSON.parse(event.target?.result as string);
        const alunosList = Array.isArray(parsed) ? parsed : [parsed];
        setJsonPreview(alunosList);
      } catch (err) {
        setErro('O arquivo selecionado contém um JSON inválido.');
        setJsonPreview(null);
      }
    };
    reader.readAsText(selectedFile);
  };

  const testarPredicao = async () => {
    if (!jsonPreview || jsonPreview.length === 0) {
      setErro('Nenhum dado válido encontrado no arquivo JSON.');
      return;
    }

    setCarregando(true);
    setErro(null);

    try {
      const res = await consultarRiscoAlunos(jsonPreview);
      setResultados(res);
    } catch (error: any) {
      setErro('Erro ao consultar o modelo de IA. Verifique se o servidor está rodando.');
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

      <div className="w-full max-w-xl p-6 bg-white border rounded-xl shadow-sm space-y-4 mb-6">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Carregar arquivo JSON com dados dos alunos:
          </label>
          <input
            type="file"
            accept=".json,application/json"
            onChange={handleFileChange}
            className="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100 cursor-pointer"
          />
        </div>

        {erro && (
          <p className="text-xs font-semibold text-red-600 bg-red-50 p-2 rounded border border-red-200">
            {erro}
          </p>
        )}

        {jsonPreview && (
          <div>
            <p className="text-xs font-semibold text-gray-600 mb-1">
              Conteúdo carregado ({jsonPreview.length} aluno(s)):
            </p>
            <pre className="text-xs bg-gray-900 text-green-400 p-3 rounded max-h-40 overflow-y-auto">
              {JSON.stringify(jsonPreview, null, 2)}
            </pre>
          </div>
        )}

        <button
          onClick={testarPredicao}
          disabled={carregando || !jsonPreview}
          className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300 text-white font-bold py-3 px-6 rounded-lg shadow transition-colors"
        >
          {carregando ? 'Calculando...' : 'Calcular risco dos alunos'}
        </button>
      </div>

      {resultados.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 w-full max-w-4xl">
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
                  className={`inline-block px-3 py-1 rounded-full text-xs font-bold text-white ${res.nivel_risco === 'alto'
                      ? 'bg-red-500'
                      : res.nivel_risco === 'moderado'
                        ? 'bg-yellow-500'
                        : 'bg-green-500'
                    }`}
                >
                  Risco {res.nivel_risco.charAt(0).toUpperCase() + res.nivel_risco.slice(1)}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </main>
  );
}