import { FirestoreService } from "./firestore.service";
import {Instituicao } from "@/interfaces/types"

export class InstituicaoService {
  private firestore = new FirestoreService();

  async buscarPorDominio(dominio: string) {
    const resultado = await this.firestore.buscarPorCampo<Instituicao>(
      "instituicoes",
      "dominio",
      dominio
    );

    return resultado.length ? resultado[0] : null;
  }
}