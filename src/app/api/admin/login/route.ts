import { NextRequest, NextResponse } from "next/server";
import { AdminLoginService } from "@/services/admin-login.service";

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const { email, senha } = body;
    if (!email || !senha) {
      return NextResponse.json(
        {
          erro: "Email e senha são obrigatórios."
        },
        {
          status: 400
        }
      );
    }
    const adminLoginService =
      new AdminLoginService();

    const admin =
      await adminLoginService.login(
        email,
        senha
      );

    return NextResponse.json({
      sucesso: true,
      admin
    });

  } catch (error) {

    console.error("Erro no login administrativo:", error);

    return NextResponse.json(
      {
        sucesso: false,
        erro:
          error instanceof Error
            ? error.message
            : "Erro ao realizar login."
      },
      {
        status: 401
      }
    );
  }
}