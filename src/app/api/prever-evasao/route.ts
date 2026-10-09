import { NextResponse } from "next/server";

import { executarRotinaEvasao } from "@/services/rotina-evasao.service";

export async function GET() {
  try {  
    const resultado = await executarRotinaEvasao();
    return NextResponse.json(resultado);
  } catch (error) {
    console.error(
      "Erro na rotina de previsão de evasão:",
      error
    );

    return NextResponse.json(
      {
        sucesso: false,
        erro: "Erro ao executar rotina de evasão",
      },
      { status: 500 }
    );
  }
}