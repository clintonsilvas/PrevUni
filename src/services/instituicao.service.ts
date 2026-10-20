import { FirestoreService } from "./firestore.service";
import { Instituicao } from "@/interfaces/types"

export class InstituicaoService {
  private firestore = new FirestoreService();
  private colecao = "instituicoes";

  async buscarPorDominio(dominio: string) {
    const resultado = await this.firestore.buscarPorCampo<Instituicao>(
      this.colecao,
      "dominio",
      dominio
    );
    return resultado.length ? resultado[0] : null;
  }

  async buscarPorId(id: string): Promise<Instituicao | null> {
    return await this.firestore.buscar<Instituicao>(this.colecao, id);
  }

  async listar(): Promise<Instituicao[]> {
    return await this.firestore.listar<Instituicao>(this.colecao);
  } 

  async criar(
    dados: Omit<Instituicao, "id">
  ): Promise<Instituicao> {

    const id = crypto.randomUUID();

    const instituicao: Instituicao = {
      id,
      ...dados,
    };

    await this.firestore.salvar(
      this.colecao,
      id,
      instituicao
    );

    return instituicao;
  }

  async atualizar(
    id: string,
    dados: Partial<Omit<Instituicao, "id">>
  ): Promise<Instituicao> {

    const instituicaoExistente =
      await this.buscarPorId(id);

    if (!instituicaoExistente) {
      throw new Error("Instituição não encontrada.");
    }

    const instituicaoAtualizada: Instituicao = {
      ...instituicaoExistente,
      ...dados,
      id,
    };

    await this.firestore.salvar(
      this.colecao,
      id,
      instituicaoAtualizada
    );

    return instituicaoAtualizada;
  }
}
