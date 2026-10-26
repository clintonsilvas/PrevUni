import { NextResponse } from "next/server";
import { InstituicaoService } from "@/services/instituicao.service";
import { verificarAdminApi } from "@/lib/admin-auth-api";

interface Params {
  params: Promise<{
    idInstituicao: string;
  }>;
}

export async function GET(
  request: Request,
  { params }: Params
) {
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

    const { idInstituicao } = await params;

    const service = new InstituicaoService();

    const instituicao = await service.buscarPorId(idInstituicao);

    if (!instituicao) {
      return NextResponse.json(
        {
          success: false,
          message: "Instituição não encontrada.",
        },
        { status: 404 }
      );
    }

    if (!instituicao.api) {
      return NextResponse.json(
        {
          success: false,
          message: "Instituição não possui uma API configurada.",
        },
        { status: 400 }
      );
    }

    const resultado = await service.validarApi(
      instituicao.api
    );

    return NextResponse.json({
      success: true,
      data: resultado,
    });

  } catch (error) {
    console.error("Erro ao verificar API:", error);

    return NextResponse.json(
      {
        success: false,
        message: "Erro ao verificar API da instituição.",
      },
      { status: 500 }
    );
  }
}