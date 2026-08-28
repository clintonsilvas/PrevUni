import { NextResponse } from "next/server";
import { SessaoService } from "@/services/sessao.service";
import { DisciplinaService } from "@/services/disciplina.service";

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

        const disciplinaService = new DisciplinaService();
        const disciplinas = await disciplinaService.listarPorSessao(sessao);

        return NextResponse.json({
            success: true,
            data: disciplinas,
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