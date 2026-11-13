import { redirect } from "next/navigation";
import { Suspense } from "react";
import { SessaoService } from "@/services/sessao.service";
import DisciplinasClient from "./DisciplinasClient";

export default async function DisciplinasPage() {
    const sessaoService = new SessaoService();
    const sessao = await sessaoService.obterSessao();

    if (!sessao) {
        redirect("/login");
    }

    return (
        <Suspense fallback={null}>
            <DisciplinasClient usuarioNome={sessao.nome} tipoUsuario={sessao.tipo} />
        </Suspense>
    );
}