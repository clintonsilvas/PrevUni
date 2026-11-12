import { FirestoreService } from "./firestore.service";
import {
  DadosEvasao,
  Evasao,
  Aluno,
  Curso,
  Disciplina,
  Matricula,
} from "@/interfaces/types";
import { Timestamp } from "firebase/firestore";

interface ResultadoRNA {
  id: string;
  probabilidade_evasao_percent: number;
  nivel_risco: "baixo" | "medio" | "alto";
}

export class EvasaoService {
  private firestore = new FirestoreService();

  private urlRNA = process.env.RNA_API_URL!;

  async preverEvasao(
    instituicaoId: string,
    dados: DadosEvasao[]
  ): Promise<Evasao[]> {
    // 1. Envia os dados para a RNA
    const response = await fetch(`${this.urlRNA}/prever-evasao`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(dados),
    });

    if (!response.ok) {
      throw new Error(`Erro na API da RNA: ${response.status}`);
    }

    const resultados: ResultadoRNA[] = await response.json();

    // Aviso se a RNA devolveu menos (ou mais) resultados do que o enviado
    if (resultados.length !== dados.length) {
      console.warn(
        `RNA devolveu ${resultados.length} resultados para ${dados.length} alunos enviados.`
      );
    }

    // Map por id (evita o find dentro do map, que é O(n²))
    const entradaPorId = new Map(dados.map((d) => [String(d.id), d]));

    const registros: Evasao[] = resultados.map((resultado) => {
      const entrada = entradaPorId.get(String(resultado.id));

      if (!entrada) {
        console.warn(`RNA devolveu id desconhecido: ${resultado.id}`);
      }

      return {
        id: crypto.randomUUID(),
        alunoId: resultado.id,
        probabilidadeEvasaoPercent: resultado.probabilidade_evasao_percent,
        nivelRisco: resultado.nivel_risco,
        data: Timestamp.now(),
        dadosEntrada: {
          media_notas: entrada?.media_notas ?? 0,
          taxa_aprovacao: entrada?.taxa_aprovacao ?? 0,
          total_ch_cumprida: entrada?.total_ch_cumprida ?? 0,
          total_reprovacoes_pendencias:
            entrada?.total_reprovacoes_pendencias ?? 0,
          percentual_conclusao_curso: entrada?.percentual_conclusao_curso ?? 0,
        },
      };
    });

    // Salva em lote (respeitando o limite de 500 do Firestore)
    await this.firestore.salvarLote<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      registros
    );

    return registros;
  }

  async listarHistoricoAluno(
    instituicaoId: string,
    alunoId: string
  ): Promise<Evasao[]> {
    const registros = await this.firestore.buscarPorCampo<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      "alunoId",
      alunoId
    );

    return registros.sort(
      (a, b) => this.toMillis(b.data) - this.toMillis(a.data)
    );
  }

  /**
   * "curso" pode ser null: o aluno ainda é avaliado, só que o
   * percentual_conclusao_curso fica 0 (não dá para calcular sem o curso).
   */
  async calcularDadosAluno(
    aluno: Aluno,
    curso: Curso | null,
    disciplinas: Disciplina[],
    matriculas: Matricula[]
  ): Promise<DadosEvasao> {
    // CORREÇÃO: comparação por String() para não falhar por diferença de tipo
    const matriculasDoAluno = matriculas.filter(
      (m) => String(m.alunoId) === String(aluno.id)
    );

    const disciplinasDoCurso = curso
      ? disciplinas.filter((d) => String(d.cursoId) === String(curso.id))
      : [];

    // 1. Média das notas não nulas (CORREÇÃO: != null cobre undefined também)
    const notasValidas = matriculasDoAluno
      .filter((m) => m.nota != null)
      .map((m) => Number(m.nota));

    const mediaNotas =
      notasValidas.length > 0
        ? notasValidas.reduce((soma, nota) => soma + nota, 0) /
        notasValidas.length
        : 0;

    const matriculasConcluidas = matriculasDoAluno.filter(
      (m) => m.situacao === "aprovado" || m.situacao === "reprovado"
    );

    const totalAprovacoes = matriculasDoAluno.filter(
      (m) => m.situacao === "aprovado"
    ).length;

    const taxaAprovacao =
      matriculasConcluidas.length > 0
        ? totalAprovacoes / matriculasConcluidas.length
        : 0;

    const totalChCumprida = matriculasDoAluno
      .filter((m) => m.situacao === "aprovado")
      .reduce((soma, m) => soma + (m.cargaHorariaCumprida ?? 0), 0);

    const totalReprovacoesPendencias = matriculasDoAluno.filter(
      (m) => m.situacao === "reprovado" || m.situacao === "pendente"
    ).length;

    const cargaHorariaTotalCurso = disciplinasDoCurso.reduce(
      (soma, d) => soma + (d.cargaHoraria ?? 0),
      0
    );

    const percentualConclusaoCurso =
      cargaHorariaTotalCurso > 0 ? totalChCumprida / cargaHorariaTotalCurso : 0;

    return {
      id: aluno.id,
      media_notas: Number(mediaNotas.toFixed(2)),
      taxa_aprovacao: Number(taxaAprovacao.toFixed(4)),
      total_ch_cumprida: totalChCumprida,
      total_reprovacoes_pendencias: totalReprovacoesPendencias,
      percentual_conclusao_curso: Number(percentualConclusaoCurso.toFixed(4)),
    };
  }

  async buscarUltimaEvasao(
    instituicaoId: string,
    alunoId: string
  ): Promise<Evasao | null> {
    return this.firestore.buscarUltimo<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      "alunoId",
      alunoId
    );
  }

  private toMillis(data: any): number {
    if (!data) return 0;
    if (typeof data.toMillis === "function") return data.toMillis();
    return new Date(data).getTime() || 0;
  }
}