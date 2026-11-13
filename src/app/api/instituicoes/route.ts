import { NextRequest, NextResponse } from "next/server";
import { InstituicaoService } from "@/services/instituicao.service";
import { verificarAdminApi } from "@/lib/admin-auth-api";

export async function GET() {
  try {
    const admin = await verificarAdminApi();

    if (!admin) {
      return NextResponse.json(
        { success: false, message: "Não autenticado." },
        { status: 401 }
      );
    }

    const service = new InstituicaoService();
    const instituicoes = await service.listar();

    return NextResponse.json({
      success: true,
      data: instituicoes,
    });
  } catch (error) {
    console.error("Erro ao listar instituições:", error);

    return NextResponse.json(
      { success: false, message: "Erro ao listar instituições." },
      { status: 500 }
    );
  }
}

export async function POST(request: NextRequest) {
  try {
    const admin = await verificarAdminApi();

    if (!admin) {
      return NextResponse.json(
        {
          success: false,
          message: "Não autenticado.",
        },
        { status: 401 }
      );
    }

    const dados = await request.json();

    const instituicaoService = new InstituicaoService();

    const existente = await instituicaoService.buscarPorDominio(
      dados.dominio
    );

    if (existente) {
      return NextResponse.json(
        {
          success: false,
          message: "Já existe uma instituição com este domínio.",
        },
        { status: 409 }
      );
    }

    const instituicao = await instituicaoService.criar(dados);

    return NextResponse.json(
      {
        success: true,
        data: instituicao,
      },
      { status: 201 }
    );

  } catch (error: any) {
    console.error("Erro ao criar instituição:", error);

    return NextResponse.json(
      {
        success: false,
        message: error.message || "Erro ao criar instituição.",
      },
      { status: 500 }
    );
  }
}