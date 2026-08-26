import { NextResponse } from "next/server";

import { SessaoService } from "@/services/sessao.service";
import { AlunoService } from "@/services/aluno.service";

export async function GET() {
  try {
    const sessaoService = new SessaoService();
    const alunoService = new AlunoService();
    
    const sessao = await sessaoService.obterSessao();

    if (!sessao) {
      return NextResponse.json(
        { erro: "Não autenticado" },
        { status: 401 }
      );
    }
    
    const alunos = await alunoService.listarPorSessao(sessao);
    return NextResponse.json(alunos);

  } catch (error) {
    console.error("Erro ao listar alunos:", error);

    return NextResponse.json(
      { erro: "Erro interno ao buscar alunos" },
      { status: 500 }
    );
  }
}