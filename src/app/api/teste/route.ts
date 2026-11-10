import { NextResponse } from "next/server";
import { migrarInstituicao } from "@/services/temp.service";

export async function GET() {
  try {
    await migrarInstituicao();

    return NextResponse.json({
      sucesso: true,
      mensagem: "Migração executada com sucesso.",
    });
  } catch (error) {
    console.error(error);

    return NextResponse.json(
      {
        sucesso: false,
        erro: "Erro ao migrar instituição.",
      },
      { status: 500 }
    );
  }
}