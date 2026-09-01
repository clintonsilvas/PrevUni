"use client";

import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import Header from "@/components/header/header.component";
import DisciplinaCard from "@/components/disciplinaCard/disciplinaCard.component";
import Button from "@/components/button/button.component";
import ExportMenu from "@/components/exportMenu/exportMenu.component";
import {
    LupaIcon,
    SearchButtonIcon,
    ChevronDownIcon,
} from "@/components/icons/icons";
import { DisciplinaResumo } from "@/services/disciplina.service";
import "./disciplinas.style.css";
import Footer from "@/components/Footer/Footer";

interface DisciplinasClientProps {
    usuarioNome: string;
    tipoUsuario?: "coordenador" | "professor" | "admin";
}

type Ordenacao = "az" | "mais-alunos" | "periodo";

const OPCOES_ORDENACAO: { valor: Ordenacao; label: string }[] = [
    { valor: "az", label: "(A-Z)" },
    { valor: "mais-alunos", label: "Quantidade de Alunos" },
    { valor: "periodo", label: "Período" },
];

function DisciplinasClient({ usuarioNome, tipoUsuario }: DisciplinasClientProps) {
    const router = useRouter();
    const searchParams = useSearchParams();
    const cursoIdFiltro = searchParams.get("cursoId");

    const [disciplinas, setDisciplinas] = useState<DisciplinaResumo[]>([]);
    const [carregando, setCarregando] = useState(true);
    const [erro, setErro] = useState<string | null>(null);
    const [busca, setBusca] = useState("");
    const [ordenacao, setOrdenacao] = useState<Ordenacao>("az");

    useEffect(() => {
        async function buscarDisciplinas() {
            setCarregando(true);
            setErro(null);

            try {
                const resposta = await fetch("/api/disciplinas");

                if (resposta.status === 401) {
                    router.push("/login");
                    return;
                }

                const json = await resposta.json();

                if (!resposta.ok || !json.success) {
                    throw new Error(
                        json.message ?? "Não foi possível carregar as disciplinas."
                    );
                }

                setDisciplinas(json.data);
            } catch {
                setErro(
                    "Não foi possível carregar as disciplinas. Tente novamente em instantes."
                );
            } finally {
                setCarregando(false);
            }
        }

        buscarDisciplinas();
    }, [router]);

    const disciplinasFiltradas = useMemo(() => {
        const termo = busca.trim().toLowerCase();

        const resultado = disciplinas.filter((disciplina) => {
            if (cursoIdFiltro && disciplina.cursoId !== cursoIdFiltro) {
                return false;
            }

            if (!termo) return true;

            return (
                disciplina.nome.toLowerCase().includes(termo) ||
                disciplina.professorNome.toLowerCase().includes(termo) ||
                disciplina.cursoNome.toLowerCase().includes(termo)
            );
        });

        return [...resultado].sort((a, b) => {
            if (ordenacao === "mais-alunos") {
                return b.quantidadeAlunos - a.quantidadeAlunos;
            }
            if (ordenacao === "periodo") {
                return a.periodo - b.periodo;
            }
            return a.nome.localeCompare(b.nome, "pt-BR");
        });
    }, [busca, disciplinas, ordenacao, cursoIdFiltro]);

    const cursoNomeFiltro = useMemo(() => {
        if (!cursoIdFiltro) return null;
        const encontrada = disciplinas.find(
            (disciplina) => disciplina.cursoId === cursoIdFiltro
        );
        return encontrada?.cursoNome ?? null;
    }, [cursoIdFiltro, disciplinas]);

    const colunasExport = useMemo(
        () => [
            { chave: "nome", label: "Disciplina" },
            { chave: "cursoNome", label: "Curso" },
            { chave: "professorNome", label: "Professor" },
            { chave: "periodo", label: "Período" },
            { chave: "cargaHoraria", label: "Carga Horária" },
            { chave: "quantidadeAlunos", label: "Alunos" },
        ],
        []
    );

    const dadosExport = useMemo(
        () =>
            disciplinasFiltradas.map((disciplina) => ({
                nome: disciplina.nome,
                cursoNome: disciplina.cursoNome,
                professorNome: disciplina.professorNome,
                periodo: disciplina.periodo,
                cargaHoraria: disciplina.cargaHoraria,
                quantidadeAlunos: disciplina.quantidadeAlunos,
            })),
        [disciplinasFiltradas]
    );

    return (
        <>
            <Header usuarioNome={usuarioNome} tipoUsuario={tipoUsuario} />

            <main className="disciplinas-page">
                <div className="disciplinas-title-row">
                    <h1 className="disciplinas-title">Disciplinas</h1>

                    <ExportMenu
                        nomeArquivo="disciplinas"
                        colunas={colunasExport}
                        dados={dadosExport}
                    />
                </div>

                {cursoIdFiltro && (
                    <div className="disciplinas-filtro-ativo">
                        <span>
                            Mostrando disciplinas de:{" "}
                            <strong>{cursoNomeFiltro ?? "carregando..."}</strong>
                        </span>
                        <Link href="/disciplinas">Limpar filtro</Link>
                    </div>
                )}

                <div className="disciplinas-search">
                    <div className="disciplinas-search-input">
                        <LupaIcon size={20} />
                        <input
                            value={busca}
                            onChange={(e) => setBusca(e.target.value)}
                            placeholder="O que você procura?"
                        />
                    </div>

                    <div className="disciplinas-search-button">
                        <Button
                            style="primary"
                            content="Pesquisar"
                            onClickButton={() => { }}
                        />
                    </div>

                    <button
                        type="button"
                        className="disciplinas-search-submit-mobile"
                        aria-label="Pesquisar"
                    >
                        <SearchButtonIcon size={40} />
                    </button>
                </div>

                <div className="disciplinas-toolbar">
                    <span className="disciplinas-count">
                        {disciplinasFiltradas.length}{" "}
                        {disciplinasFiltradas.length === 1 ? "Disciplina" : "Disciplinas"}
                    </span>

                    <div className="disciplinas-sort-desktop">
                        <span>Ordenar por:</span>
                        {OPCOES_ORDENACAO.map((opcao) => (
                            <button
                                key={opcao.valor}
                                onClick={() => setOrdenacao(opcao.valor)}
                                className={
                                    ordenacao === opcao.valor
                                        ? "disciplinas-sort-pill disciplinas-sort-pill-active"
                                        : "disciplinas-sort-pill"
                                }
                            >
                                {opcao.label}
                            </button>
                        ))}
                    </div>

                    <label className="disciplinas-sort-mobile">
                        <span>Ordenar por:</span>
                        <div className="disciplinas-sort-mobile-select">
                            <select
                                value={ordenacao}
                                onChange={(e) => setOrdenacao(e.target.value as Ordenacao)}
                            >
                                {OPCOES_ORDENACAO.map((opcao) => (
                                    <option key={opcao.valor} value={opcao.valor}>
                                        {opcao.label}
                                    </option>
                                ))}
                            </select>
                            <ChevronDownIcon size={14} />
                        </div>
                    </label>
                </div>

                {carregando && (
                    <p className="disciplinas-status">Carregando disciplinas...</p>
                )}

                {!carregando && erro && (
                    <p className="disciplinas-status disciplinas-status-erro">{erro}</p>
                )}

                {!carregando && !erro && (
                    <>
                        <div className="disciplinas-grid">
                            {disciplinasFiltradas.map((disciplina) => (
                                <DisciplinaCard key={disciplina.id} disciplina={disciplina} />
                            ))}
                        </div>

                        {disciplinasFiltradas.length === 0 && (
                            <p className="disciplinas-empty">
                                Nenhuma disciplina encontrada para essa busca.
                            </p>
                        )}
                    </>
                )}
            </main>

            <Footer />
        </>
    );
}

export default DisciplinasClient;