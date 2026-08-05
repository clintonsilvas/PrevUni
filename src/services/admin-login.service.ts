import { FirestoreService } from "@/services/firestore.service";
import { criarToken } from "@/lib/auth";

export interface Admin {
  id: string;
  nome: string;
  email: string;
  senha: string;
  ativo: boolean;
}

export class AdminLoginService {

  private firestore = new FirestoreService();

  async login(
    email: string,
    senha: string
  ) {

    const admins =
      await this.firestore.buscarPorCampo<Admin>(
        "admins",
        "email",
        email
      );

    if (!admins.length) {
      throw new Error("Administrador não encontrado.");
    }

    const admin = admins[0];

    if (!admin.ativo) {
      throw new Error("Administrador desativado.");
    }

    if (admin.senha !== senha) {
      throw new Error("Senha inválida.");
    }

    const token = await criarToken({
      id: admin.id,
      nome: admin.nome,
      email: admin.email,
      tipo: "admin",
    });

    return {
      admin: {
        id: admin.id,
        nome: admin.nome,
        email: admin.email,
      },
      token,
    };
  }
}