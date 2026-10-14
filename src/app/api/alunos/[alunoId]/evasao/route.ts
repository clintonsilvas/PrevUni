import { NextResponse } from "next/server";
import { EvasaoService } from "@/services/evasao.service";
import { SessaoService } from "@/services/sessao.service";

export async function GET(
  request: Request,
  { params }: { params: Promise<{ alunoId: string }> }
) {
  try {
    const sessao = await new SessaoService().obterSessao();

    if (!sessao) {
      return NextResponse.json(
        { erro: "Não autenticado" },
        { status: 401 }
      );
    }

    const { alunoId } = await params;

    const evasaoService = new EvasaoService();

    const evasao = await evasaoService.buscarUltimaEvasao(
      sessao.instituicaoId,
      alunoId
    );

    if (!evasao) {
      return NextResponse.json(
        { erro: "Previsão não encontrada" },
        { status: 404 }
      );
    }

    return NextResponse.json(evasao);
  } catch (error){
    console.error("Erro ao buscar última evasão:", error);

    return NextResponse.json(
      { erro: "Erro interno do servidor" },
      { status: 500 }
    );
  }
}