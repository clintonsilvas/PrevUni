import { FirestoreService } from "./firestore.service";
import { Evasao, Aluno } from "@/interfaces/types";

export class PrevisaoService {
  private firestore = new FirestoreService();

  async buscarHistoricoEvasao(
    instituicaoId: string,
    alunoId: string
  ): Promise<Evasao[]> {
    return this.firestore.buscarPorCampo<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      `alunoId`,
      alunoId
    );
  }

  async buscarHistoricoEvasaoCurso(
    instituicaoId: string,
    cursoId: string
  ): Promise<Evasao[]> {
    const alunos = await this.firestore.buscarPorCampo<Aluno>(
      `instituicoes/${instituicaoId}/alunos`,
      `cursoId`,
      cursoId
    );

    const alunoIds = alunos.map((a) => a.id);

    const evasoes = await this.firestore.buscarPorLista<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      `alunoId`,
      alunoIds
    );

    return this.ultimaEvasaoPorAluno(evasoes);
  }

  async buscarHistoricoEvasaoAlunos(instituicaoId: string): Promise<Evasao[]> {
    const alunos = await this.firestore.listar<Aluno>(
      `instituicoes/${instituicaoId}/alunos`
    );

    const alunoIds = alunos.map((a) => a.id);

    const evasoes = await this.firestore.buscarPorLista<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      `alunoId`,
      alunoIds
    );

    const ultimas = this.ultimaEvasaoPorAluno(evasoes);

    if (ultimas.length < alunos.length) {
      console.warn(
        `${alunos.length - ultimas.length} de ${alunos.length} alunos sem registro de evasão.`
      );
    }

    return ultimas;
  }

  // CORREÇÃO: não quebra se "data" estiver ausente ou não for Timestamp
  private toMillis(data: any): number {
    if (!data) return 0;
    if (typeof data.toMillis === "function") return data.toMillis();
    return new Date(data).getTime() || 0;
  }

  private ultimaEvasaoPorAluno(evasoes: Evasao[]): Evasao[] {
    const mapa = new Map<string, Evasao>();

    for (const evasao of evasoes) {
      const atual = mapa.get(evasao.alunoId);

      const dataNova = this.toMillis(evasao.data);
      const dataAtual = atual ? this.toMillis(atual.data) : 0;

      if (!atual || dataNova > dataAtual) {
        mapa.set(evasao.alunoId, evasao);
      }
    }

    return Array.from(mapa.values());
  }
}