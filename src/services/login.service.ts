import { FirestoreService } from "@/services/firestore.service";
import { Coordernador, Professor } from "@/interfaces/types";

export class LoginService {

  private firestore = new FirestoreService();

  async login(
    instituicaoId: string,
    email: string,
    senha: string
  ) {

    let tipo: "coordenador" | "professor" = "coordenador";

    let usuario = await this.firestore.buscarPorCampo<Coordernador>(
      `instituicoes/${instituicaoId}/coordenadores`,
      "email",
      email
    );

    if (!usuario.length) {

      tipo = "professor";

      usuario = await this.firestore.buscarPorCampo<Professor>(
        `instituicoes/${instituicaoId}/professores`,
        "email",
        email
      );

    }

    if (!usuario.length) {
      throw new Error("Usuário não encontrado.");
    }

    const dados = usuario[0];

    if (dados.matricula !== senha) {
      throw new Error("Senha inválida.");
    }

    return {
      id: dados.id,
      nome: dados.nome,
      email: dados.email,
      instituicaoId,
      tipo,
    };

  }

}