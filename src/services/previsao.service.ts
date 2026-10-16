import { FirestoreService } from "./firestore.service";
import { Evasao, Aluno} from "@/interfaces/types";

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

    const alunoIds = alunos.map(a => a.id);

    const evasoes = await this.firestore.buscarPorLista<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      `alunoId`,
      alunoIds
    );

    return this.ultimaEvasaoPorAluno(evasoes);
  }
  async buscarHistoricoEvasaoAlunos(
    instituicaoId: string
  ): Promise<Evasao[]> {
    const alunos = await this.firestore.listar<Aluno>(
      `instituicoes/${instituicaoId}/alunos`
    );

    const alunoIds = alunos.map(a => a.id);

    const evasoes = await this.firestore.buscarPorLista<Evasao>(
      `instituicoes/${instituicaoId}/evasao`,
      `alunoId`,
      alunoIds
    );

    return this.ultimaEvasaoPorAluno(evasoes);
  }
  private ultimaEvasaoPorAluno(evasoes: Evasao[]): Evasao[] {
    const mapa = new Map<string, Evasao>();

    for (const evasao of evasoes) {
      const atual = mapa.get(evasao.alunoId);

      if (
        !atual ||
        new Date(evasao.data).getTime() > new Date(atual.data).getTime()
      ) {
        mapa.set(evasao.alunoId, evasao);
      }
    }

    return Array.from(mapa.values());
  }
  
}
