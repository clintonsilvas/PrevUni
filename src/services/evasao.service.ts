import { FirestoreService } from "./firestore.service";
import { DadosEvasao, Evasao, Aluno, Curso, Disciplina, Matricula } from "@/interfaces/types";
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
    const response = await fetch(
      `${this.urlRNA}/prever-evasao`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(dados),
      }
    );

    if (!response.ok) {
      throw new Error(
        `Erro na API da RNA: ${response.status}`
      );
    }

    const resultados: ResultadoRNA[] =  await response.json();

    const registros: Evasao[] = resultados.map(
      (resultado) => {

        const entrada = dados.find(
          (item) => item.id === resultado.id
        );

        return {
          id: crypto.randomUUID(),
          alunoId: resultado.id,
          probabilidadeEvasaoPercent:  resultado.probabilidade_evasao_percent,
          nivelRisco: resultado.nivel_risco,
          data: Timestamp.now(),
          dadosEntrada: {
            media_notas: entrada?.media_notas ?? 0,
            taxa_aprovacao: entrada?.taxa_aprovacao ?? 0,
            total_ch_cumprida: entrada?.total_ch_cumprida ?? 0,
            total_reprovacoes_pendencias: entrada?.total_reprovacoes_pendencias ?? 0,
            percentual_conclusao_curso: entrada?.percentual_conclusao_curso ?? 0,
          },
        };
      }
    );

    await Promise.all(
      registros.map((registro) =>
        this.firestore.salvar<Evasao>(`instituicoes/${instituicaoId}/evasao`,
          registro.id,
          registro
        )
      )
    );

    return registros;
  }

  
  async listarHistoricoAluno(
    instituicaoId: string,
    alunoId: string
  ): Promise<Evasao[]> {

    const registros = await this.firestore.listar<Evasao>(
      `instituicoes/${instituicaoId}/evasao`
    );

    return registros
      .filter((registro) => registro.alunoId === alunoId)
      .sort(
    (a, b) => b.data.toMillis() - a.data.toMillis()
      );
  }

  async calcularDadosAluno(
    aluno: Aluno,
    curso: Curso,
    disciplinas: Disciplina[],
    matriculas: Matricula[]
  ): Promise<DadosEvasao> {
    // Matrículas do aluno
    const matriculasDoAluno = matriculas.filter(
      (matricula) => matricula.alunoId === aluno.id
    );

    // Disciplinas pertencentes ao curso do aluno
    const disciplinasDoCurso = disciplinas.filter(
      (disciplina) => disciplina.cursoId === curso.id
    );

    // 1. Média das notas não nulas
    const notasValidas = matriculasDoAluno
      .filter((matricula) => matricula.nota !== null)
      .map((matricula) => matricula.nota as number);

    const mediaNotas =
      notasValidas.length > 0
        ? notasValidas.reduce((soma, nota) => soma + nota, 0) /
          notasValidas.length
        : 0;

    
    const matriculasConcluidas = matriculasDoAluno.filter(
      (matricula) =>
        matricula.situacao === "aprovado" ||
        matricula.situacao === "reprovado"
    );

    const totalAprovacoes = matriculasDoAluno.filter(
      (matricula) => matricula.situacao === "aprovado"
    ).length;

    const taxaAprovacao =
      matriculasConcluidas.length > 0
        ? totalAprovacoes / matriculasConcluidas.length
        : 0;

    
    const totalChCumprida = matriculasDoAluno
      .filter((matricula) => matricula.situacao === "aprovado")
      .reduce(
        (soma, matricula) => soma + matricula.cargaHorariaCumprida,
        0
      );

    
    const totalReprovacoesPendencias = matriculasDoAluno.filter(
      (matricula) =>
        matricula.situacao === "reprovado" ||
        matricula.situacao === "pendente"
    ).length;

    
    const cargaHorariaTotalCurso = disciplinasDoCurso.reduce(
      (soma, disciplina) => soma + disciplina.cargaHoraria,
      0
    );

    
    const percentualConclusaoCurso =    cargaHorariaTotalCurso > 0   ? totalChCumprida / cargaHorariaTotalCurso : 0;

    return {
      id: aluno.id,
      media_notas: Number(mediaNotas.toFixed(2)),
      taxa_aprovacao: Number(taxaAprovacao.toFixed(4)),
      total_ch_cumprida: totalChCumprida,
      total_reprovacoes_pendencias: totalReprovacoesPendencias,
      percentual_conclusao_curso: Number(
        percentualConclusaoCurso.toFixed(4)
      ),
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
}
