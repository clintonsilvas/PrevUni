import { NextResponse } from "next/server";
import { SyncService } from "@/services/sync.service";
import { InstituicaoService } from "@/services/instituicao.service";


export async function POST() {
  try {   
    const validas = await new InstituicaoService().buscarTodasComApiValida(); 
    const sync = new SyncService();

    await sync.sincronizarvarias(validas);

    return NextResponse.json({
      success: true,
      message: "Sincronizações realizadas com sucesso.",
    });

  } catch (error) {
    console.error(
      "Erro ao sincronizar dados das instituições:",
      error
    );

    return NextResponse.json(
      {
        success: false,
        message: "Erro ao sincronizar",
      },
      { status: 500 }
    );
  }
}