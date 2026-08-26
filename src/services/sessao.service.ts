import { cookies } from "next/headers";
import { JwtService } from "./jwt.service";
import { FirestoreService } from "./firestore.service";
import { Coordernador, Professor, Admin } from "@/interfaces/types";

export interface Sessao {
  id: string;
  instituicaoId: string;
  tipo: "coordenador" | "professor" | "admin";
  nome: string;
  email: string;
}

export class SessaoService {
  private firestore = new FirestoreService();

  async obterSessao(): Promise<Sessao | null> {
    const token = (await cookies()).get("prevuni_token")?.value;

    if (!token) {
      return null;
    }

    const jwt = new JwtService();
    const payload = await jwt.validarToken(token);

    if (!payload) {
      return null;
    }

    const { id, instituicaoId, tipo } = payload as {
      id: string;
      instituicaoId: string;
      tipo: "coordenador" | "professor" | "admin";
    };

    const colecao =
      tipo === "admin"
        ? "admins"
        : tipo === "coordenador"
          ? "coordenadores"
          : "professores";

    const usuario = await this.firestore.buscar<
      Coordernador | Professor | Admin
    >(
      `instituicoes/${instituicaoId}/${colecao}`,
      id
    );

    if (!usuario) {
      return null;
    }

    return {
      id,
      instituicaoId,
      tipo,
      nome: usuario.nome,
      email: usuario.email,
    };
  }
}