export interface DadosAlunoInput {
  media_notas: number;
  taxa_aprovacao: number;
  total_ch_cumprida: number;
  total_reprovacoes_pendencias: number;
  percentual_conclusao_curso: number;
}

export interface RespostaPredicao {
  probabilidade_evasao_percent: number;
  risco_alto: boolean;
}

export async function consultarRiscoAluno(aluno: DadosAlunoInput): Promise<RespostaPredicao> {
  const response = await fetch('http://localhost:8000/prever-evasao', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(aluno),
  });

  if (!response.ok) {
    throw new Error('Erro ao conectar com o serviço de IA');
  }

  return await response.json();
}