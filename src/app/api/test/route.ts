import { NextResponse } from "next/server";

import { AlunoService } from "@/services/aluno.service";
import { SessaoService } from "@/services/sessao.service";

export async function GET(
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

    const resultado = new AlunoService().buscarDadosAluno(
      sessao.instituicaoId,
      alunoId
    );    


    return NextResponse.json(resultado);
  } catch (error) {
    console.error(
      "Erro na buscar dados aluno",
      error
    );

    return NextResponse.json(
      {
        sucesso: false,
        erro: "Erro:",
      },
      { status: 500 }
    );
  }
}