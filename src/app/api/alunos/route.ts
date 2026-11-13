import { NextResponse } from "next/server";
import { SessaoService } from "@/services/sessao.service";
import { AlunoService } from "@/services/aluno.service";

export async function GET() {
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
    const alunoService = new AlunoService();
    const alunos = await alunoService.listarPorSessao(sessao);
    return NextResponse.json({
        success: true,
        data: alunos,
    });
    } catch (error: any) {
      return NextResponse.json(
          {
              success: false,
              message: error.message,
          },
          { status: 500 }
      );
  }
}