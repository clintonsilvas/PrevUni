import { NextResponse } from "next/server";
import { SyncService } from "@/services/sync.service";
import { verificarAdminApi } from "@/lib/admin-auth-api";

interface Params {
  params: Promise<{
    idInstituicao: string;
  }>;
}

export async function POST(
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

    if (!idInstituicao) {
      return NextResponse.json(
        {
          success: false,
          message: "Instituição inválida.",
        },
        { status: 400 }
      );
    }

    const sync = new SyncService();

    await sync.sincronizar(idInstituicao);

    return NextResponse.json({
      success: true,
      message: "Sincronização realizada com sucesso.",
    });

  } catch (error) {
    console.error(
      "Erro ao sincronizar dados da instituição:",
      error
    );

    return NextResponse.json(
      {
        success: false,
        message: "Erro ao sincronizar dados da instituição.",
      },
      { status: 500 }
    );
  }
}