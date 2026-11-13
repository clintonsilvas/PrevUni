
"use client";

import { useEffect, useRef, useState } from "react";
import Header from "@/components/header/header.component";
import Footer from "@/components/Footer/Footer";

import "./meus-dados.style.css";

interface DadosMeusDados {
    usuario: {
        id: string;
        nome: string;
        email: string;
        tipo: "admin" | "coordenador" | "professor";
    };
    instituicao: {
        nome: string;
        sigla: string;
        cidade: string;
        estado: string;
        dominio: string;
    } | null;
    cursos: any[];
    disciplinas: any[];
    estatisticas: {
        totalCursos: number;
        totalDisciplinas: number;
        totalAlunos: number;
    };
}

export default function MeusDadosPage() {
    const [dados, setDados] = useState<DadosMeusDados | null>(null);
    const [carregando, setCarregando] = useState(true);
    const [erro, setErro] = useState("");

    const requisicaoIniciada = useRef(false);

    useEffect(() => {
        if (requisicaoIniciada.current) return;

        requisicaoIniciada.current = true;

        async function carregarDados() {
            try {
                console.log("[MEUS-DADOS] Enviando GET...");

                const resposta = await fetch("/api/meus-dados", {
                    method: "GET",
                    cache: "no-store",
                });

                if (resposta.status === 401) {
                    window.location.href = "/login";
                    return;
                }

                const resultado = await resposta.json();

                if (!resposta.ok || !resultado.sucesso) {
                    throw new Error(resultado.mensagem);
                }

                setDados(resultado.dados);
            } catch (erro) {
                console.error("[MEUS-DADOS] Erro:", erro);

                setErro(
                    erro instanceof Error
                        ? erro.message
                        : "Erro ao carregar os dados."
                );
            } finally {
                setCarregando(false);
            }
        }

        carregarDados();
    }, []);

    if (carregando) {
    return (
        <>
            <Header
                usuarioNome="Carregando..."
                tipoUsuario="professor"
            />

            <main className="meus-dados-page">
                <h1 className="meus-dados-title">Meus dados</h1>

                <div className="meus-dados-grid">
                    {[1, 2].map((item) => (
                        <section
                            className="meus-dados-card skeleton-card"
                            key={item}
                        >
                            <div className="skeleton skeleton-titulo" />

                            <div className="skeleton skeleton-avatar" />

                            <div className="skeleton skeleton-linha" />
                            <div className="skeleton skeleton-linha" />
                            <div className="skeleton skeleton-linha" />
                        </section>
                    ))}
                </div>

                <section className="meus-dados-stats">
                    {[1, 2, 3].map((item) => (
                        <div className="meus-dados-stat" key={item}>
                            <div className="skeleton skeleton-icone" />
                            <div className="skeleton-conteudo">
                                <div className="skeleton skeleton-numero" />
                                <div className="skeleton skeleton-texto" />
                            </div>
                        </div>
                    ))}
                </section>
            </main>

            <Footer />
        </>
    );
}

    if (erro || !dados) {
        return <p>{erro || "Não foi possível carregar os dados."}</p>;
    }

    const iniciais = dados.usuario.nome
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map((nome) => nome[0])
        .join("")
        .toUpperCase();

    const tiposUsuario = {
        admin: "Administrador",
        coordenador: "Coordenador",
        professor: "Professor",
    };

    return (
        <>
            <Header
                usuarioNome={dados.usuario.nome}
                tipoUsuario={dados.usuario.tipo}
            />

            <main className="meus-dados-page">
                <h1 className="meus-dados-title">Meus dados</h1>

                <div className="meus-dados-grid">
                    {/* Perfil */}
                    <section className="meus-dados-card">
                        <h2 className="meus-dados-card-titulo">
                            Informações pessoais
                        </h2>

                        <div className="meus-dados-perfil">
                            <div className="meus-dados-avatar">
                                {iniciais}
                            </div>

                            <div className="meus-dados-cabecalho">
                                <h2>{dados.usuario.nome}</h2>

                                <span className="meus-dados-badge">
                                    {tiposUsuario[dados.usuario.tipo]}
                                </span>
                            </div>

                            <dl className="meus-dados-lista">
                                <div className="meus-dados-item">
                                    <dt>Nome</dt>
                                    <dd>{dados.usuario.nome}</dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>E-mail</dt>
                                    <dd>{dados.usuario.email}</dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Identificador</dt>
                                    <dd>{dados.usuario.id}</dd>
                                </div>
                            </dl>
                        </div>
                    </section>

                    {/* Instituição */}
                    <section className="meus-dados-card">
                        <h2 className="meus-dados-card-titulo">
                            Instituição vinculada
                        </h2>

                        {dados.instituicao ? (
                            <dl className="meus-dados-lista">
                                <div className="meus-dados-item">
                                    <dt>Instituição</dt>
                                    <dd>{dados.instituicao.nome}</dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Sigla</dt>
                                    <dd className="meus-dados-sigla">
                                        {dados.instituicao.sigla}
                                    </dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Cidade</dt>
                                    <dd>{dados.instituicao.cidade}</dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Estado</dt>
                                    <dd>{dados.instituicao.estado}</dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Domínio</dt>
                                    <dd>{dados.instituicao.dominio}</dd>
                                </div>
                            </dl>
                        ) : (
                            <p className="meus-dados-vazio">
                                Instituição não encontrada.
                            </p>
                        )}
                    </section>
                </div>

                {/* Estatísticas */}
                <section className="meus-dados-stats">
                    <div className="meus-dados-stat">
                        <div className="meus-dados-stat-icone meus-dados-stat-icone-verde">
                            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" />
                                <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2Z" />
                            </svg>
                        </div>

                        <div>
                            <strong>{dados.estatisticas.totalCursos}</strong>
                            <span>Cursos</span>
                        </div>
                    </div>

                    <div className="meus-dados-stat">
                        <div className="meus-dados-stat-icone meus-dados-stat-icone-azul">
                            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M4 4h16v16H4z" />
                                <path d="M8 8h8M8 12h8M8 16h5" />
                            </svg>
                        </div>

                        <div>
                            <strong>{dados.estatisticas.totalDisciplinas}</strong>
                            <span>Disciplinas</span>
                        </div>
                    </div>

                    <div className="meus-dados-stat">
                        <div className="meus-dados-stat-icone meus-dados-stat-icone-roxo">
                            <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                                <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
                                <circle cx="9" cy="7" r="4" />
                                <path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
                            </svg>
                        </div>

                        <div>
                            <strong>{dados.estatisticas.totalAlunos}</strong>
                            <span>Alunos</span>
                        </div>
                    </div>
                </section>
            </main>

            <Footer />
        </>
    );
}
