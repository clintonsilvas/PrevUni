import { ApiService } from "./api.service";
import { FirestoreService } from "./firestore.service";

export class SyncService {
  private api = new ApiService();
  private firestore = new FirestoreService();

  async sincronizar(instituicaoId: string): Promise<void> {
    try {
      const instituicao = await this.firestore.buscar(
        "instituicoes",
        instituicaoId
      );

      if (!instituicao) {
        throw new Error("Instituição não encontrada.");
      }

      const dados = await this.api.getDados(instituicao.api);

      const colecoes = {
        coordenadores: dados.coordenadores,
        professores: dados.professores,
        cursos: dados.cursos,
        disciplinas: dados.disciplinas,
        alunos: dados.alunos,
        matriculas: dados.matriculas,
      };
      
      await this.firestore.salvar("instituicoes", instituicaoId, {
        ...dados.instituicao,
        dominio: instituicao.dominio,
        api: instituicao.api,
        ultimaSincronizacao: new Date(),
        statusSincronizacao: "sucesso",
      });
      
      for (const [colecao, registros] of Object.entries(colecoes)) {
        await this.firestore.salvarLote(
          `instituicoes/${instituicaoId}/${colecao}`,
          registros
        );
      }

      console.log(`Instituição ${instituicao.sigla} sincronizada.`);
    } catch (error) {
      console.error(error);

      await this.firestore.salvar("instituicoes", instituicaoId, {
        statusSincronizacao: "erro",
        ultimaTentativaSincronizacao: new Date(),
      });

      throw error;
    }
  }
}