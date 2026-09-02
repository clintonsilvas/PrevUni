import { redirect } from "next/navigation";
import { SessaoService } from "@/services/sessao.service";
import { InstituicaoService } from "@/services/instituicao.service";
import { CursoService } from "@/services/curso.service";
import { DisciplinaService } from "@/services/disciplina.service";
import Header from "@/components/header/header.component";
import { LivroIcon, DisciplinaIcon, AlunosIcon } from "@/components/icons/icons";
import "./meus-dados.style.css";
import Footer from "@/components/Footer/Footer";

const LABEL_PAPEL: Record<string, string> = {
    coordenador: "Coordenador(a)",
    professor: "Professor(a)",
    admin: "Administrador(a)",
};

const LABEL_STATS: Record<
    string,
    { cursos: string; disciplinas: string; alunos: string }
> = {
    coordenador: {
        cursos: "Cursos coordenados",
        disciplinas: "Disciplinas nos seus cursos",
        alunos: "Alunos nos seus cursos",
    },
    professor: {
        cursos: "Cursos em que leciona",
        disciplinas: "Disciplinas que leciona",
        alunos: "Alunos que leciona",
    },
    admin: {
        cursos: "Cursos na instituição",
        disciplinas: "Disciplinas na instituição",
        alunos: "Alunos na instituição",
    },
};

export default async function MeusDadosPage() {
    const sessaoService = new SessaoService();
    const sessao = await sessaoService.obterSessao();

    if (!sessao) {
        redirect("/login");
    }

    const instituicaoService = new InstituicaoService();
    const cursoService = new CursoService();
    const disciplinaService = new DisciplinaService();

    const [instituicao, cursos, disciplinas] = await Promise.all([
        instituicaoService.buscarPorId(sessao.instituicaoId),
        cursoService.listarPorSessao(sessao),
        disciplinaService.listarPorSessao(sessao),
    ]);

    const totalAlunos = cursos.reduce(
        (soma, curso) => soma + curso.quantidadeAlunos,
        0
    );

    const iniciais = sessao.nome
        .split(" ")
        .filter(Boolean)
        .slice(0, 2)
        .map((parte) => parte[0])
        .join("")
        .toUpperCase();

    const labelsStats = LABEL_STATS[sessao.tipo] ?? LABEL_STATS.admin;

    return (
        <>
            <Header usuarioNome={sessao.nome} tipoUsuario={sessao.tipo} />

            <main className="meus-dados-page">
                <h1 className="meus-dados-title">Meus dados</h1>

                <div className="meus-dados-grid">
                    <section className="meus-dados-card meus-dados-perfil">
                        <div className="meus-dados-avatar">{iniciais || "U"}</div>

                        <div className="meus-dados-cabecalho">
                            <h2>{sessao.nome}</h2>
                            <span className="meus-dados-badge">
                                {LABEL_PAPEL[sessao.tipo] ?? sessao.tipo}
                            </span>
                        </div>

                        <dl className="meus-dados-lista">
                            <div className="meus-dados-item">
                                <dt>E-mail</dt>
                                <dd>{sessao.email}</dd>
                            </div>
                        </dl>
                    </section>

                    <section className="meus-dados-card meus-dados-instituicao">
                        <h3 className="meus-dados-card-titulo">Instituição</h3>

                        {instituicao ? (
                            <dl className="meus-dados-lista">
                                <div className="meus-dados-item">
                                    <dt>Nome</dt>
                                    <dd>
                                        {instituicao.nome}{" "}
                                        <span className="meus-dados-sigla">
                                            {instituicao.sigla}
                                        </span>
                                    </dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Localização</dt>
                                    <dd>
                                        {instituicao.cidade} - {instituicao.estado}
                                    </dd>
                                </div>

                                <div className="meus-dados-item">
                                    <dt>Domínio de acesso</dt>
                                    <dd>{instituicao.dominio}</dd>
                                </div>
                            </dl>
                        ) : (
                            <p className="meus-dados-vazio">Instituição não encontrada.</p>
                        )}
                    </section>
                </div>

                <div className="meus-dados-stats">
                    <div className="meus-dados-stat">
                        <span className="meus-dados-stat-icone meus-dados-stat-icone-verde">
                            <DisciplinaIcon size={20} />
                        </span>
                        <div>
                            <strong>{cursos.length}</strong>
                            <span>{labelsStats.cursos}</span>
                        </div>
                    </div>

                    <div className="meus-dados-stat">
                        <span className="meus-dados-stat-icone meus-dados-stat-icone-azul">
                            <LivroIcon size={22} />
                        </span>
                        <div>
                            <strong>{disciplinas.length}</strong>
                            <span>{labelsStats.disciplinas}</span>
                        </div>
                    </div>

                    <div className="meus-dados-stat">
                        <span className="meus-dados-stat-icone meus-dados-stat-icone-roxo">
                            <AlunosIcon size={20} />
                        </span>
                        <div>
                            <strong>{totalAlunos}</strong>
                            <span>{labelsStats.alunos}</span>
                        </div>
                    </div>
                </div>
            </main>
            <Footer />
        </>
    );
}