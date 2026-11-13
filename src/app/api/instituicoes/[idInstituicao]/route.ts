import { NextRequest, NextResponse } from "next/server";
import { InstituicaoService } from "@/services/instituicao.service";
import { obterAdminAutenticado } from "@/lib/admin-auth";

interface Params {
  params: Promise<{
    idInstituicao: string;
  }>;
}

export async function PUT(
  request: NextRequest,
  { params }: Params
) {
  try {
    const admin = await obterAdminAutenticado();

    if (!admin) {
      return NextResponse.json(
        {
          success: false,
          message: "Não autenticado.",
        },
        { status: 401 }
      );
    }

    const { idInstituicao } = await params;

    const dados = await request.json();

    const instituicaoService = new InstituicaoService();

    const existente = await instituicaoService.buscarPorId(idInstituicao);

    if (!existente) {
      return NextResponse.json(
        {
          success: false,
          message: "Instituição não encontrada.",
        },
        { status: 404 }
      );
    }

    if (dados.dominio && dados.dominio !== existente.dominio) {
      const dominioExistente =
        await instituicaoService.buscarPorDominio(dados.dominio);

      if (
        dominioExistente &&
        dominioExistente.id !== idInstituicao
      ) {
        return NextResponse.json(
          {
            success: false,
            message: "Já existe uma instituição com este domínio.",
          },
          { status: 409 }
        );
      }
    }

    const instituicao =
      await instituicaoService.atualizar(idInstituicao, dados);

    return NextResponse.json({
      success: true,
      data: instituicao,
    });

  } catch (error: any) {
    console.error("Erro ao atualizar instituição:", error);

    return NextResponse.json(
      {
        success: false,
        message: error.message || "Erro ao atualizar instituição.",
      },
      { status: 500 }
    );
  }
}