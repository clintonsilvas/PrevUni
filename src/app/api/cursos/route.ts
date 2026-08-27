import { NextResponse } from "next/server";
import { SessaoService } from "@/services/sessao.service";
import { CursoService } from "@/services/curso.service";

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

        const cursoService = new CursoService();
        const cursos = await cursoService.listarPorSessao(sessao);

        return NextResponse.json({
            success: true,
            data: cursos,
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