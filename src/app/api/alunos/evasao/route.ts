import { NextResponse, NextRequest } from "next/server";
import { SessaoService } from "@/services/sessao.service";
import { PrevisaoService } from "@/services/previsao.service";

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

    const previsaoService = new PrevisaoService();
    const previsoesCurso = await previsaoService.buscarHistoricoEvasaoAlunos(
      sessao.instituicaoId
    );

    return NextResponse.json({
        success: true,
        data: previsoesCurso,
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