import { NextRequest, NextResponse } from "next/server";
import { AlunoService } from "@/services/aluno.service";
import { SessaoService } from "@/services/sessao.service";

const alunoService = new AlunoService();

export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ alunoId: string }> }
) {
  try {
    const sessaoService = new SessaoService(); 
    const sessao = await sessaoService.obterSessao();

    if (!sessao) {
      return NextResponse.json(
        {
          success: false,
          message: "Não autenticado.",
        },
        { status: 401 }
      );
    }  

    const { alunoId } = await params;
    const instituicaoId = sessao.instituicaoId

    if (!instituicaoId || !alunoId) {
      return NextResponse.json(
        { erro: "instituicaoId e alunoId são obrigatórios." },
        { status: 400 }
      );
    }

    const aluno = await alunoService.buscarAlunoCompletoPorId(
      instituicaoId,
      alunoId
    );

    if (!aluno) {
      return NextResponse.json(
        { erro: "Aluno não encontrado." },
        { status: 404 }
      );
    }

    return NextResponse.json(aluno);
  } catch (error) {
    console.error("Erro ao buscar aluno completo:", error);

    return NextResponse.json(
      { erro: "Erro interno ao buscar aluno." },
      { status: 500 }
    );
  }
}