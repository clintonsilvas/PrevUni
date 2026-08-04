"use client";

import { useMemo, useState } from "react";
import Header from "@/components/header/header.component";
import CourseCard from "@/components/courseCard/courseCard.component";
import Button from "@/components/button/button.component";
import {
  LupaIcon,
  SearchButtonIcon,
  ChevronDownIcon,
} from "@/components/icons/icons";
import { CursoResumo } from "@/services/curso.service";
import "./cursos.style.css";

interface CursosClientProps {
  usuarioNome: string;
  cursos: CursoResumo[];
}

type Ordenacao =
  | "az"
  | "mais-alunos"
  | "engajamento-alto"
  | "engajamento-medio"
  | "engajamento-baixo";

const OPCOES_ORDENACAO: { valor: Ordenacao; label: string }[] = [
  { valor: "az", label: "(A-Z)" },
  { valor: "mais-alunos", label: "Mais Alunos" },
  { valor: "engajamento-alto", label: "Engajamento Alto" },
  { valor: "engajamento-medio", label: "Engajamento Médio" },
  { valor: "engajamento-baixo", label: "Engajamento Baixo" },
];

function CursosClient({ usuarioNome, cursos }: CursosClientProps) {
  const [busca, setBusca] = useState("");
  const [ordenacao, setOrdenacao] = useState<Ordenacao>("az");

  const cursosFiltrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();

    let resultado = cursos.filter((curso) => {
      if (!termo) return true;
      return (
        curso.nome.toLowerCase().includes(termo) ||
        curso.coordenadorNome.toLowerCase().includes(termo)
      );
    });

    if (ordenacao === "engajamento-alto") {
      resultado = resultado.filter((curso) => curso.engajamento >= 70);
    } else if (ordenacao === "engajamento-medio") {
      resultado = resultado.filter(
        (curso) => curso.engajamento >= 40 && curso.engajamento < 70
      );
    } else if (ordenacao === "engajamento-baixo") {
      resultado = resultado.filter((curso) => curso.engajamento < 40);
    }

    resultado = [...resultado].sort((a, b) => {
      if (ordenacao === "mais-alunos") {
        return b.quantidadeAlunos - a.quantidadeAlunos;
      }
      if (ordenacao.startsWith("engajamento")) {
        return b.engajamento - a.engajamento;
      }
      return a.nome.localeCompare(b.nome, "pt-BR");
    });

    return resultado;
  }, [busca, cursos, ordenacao]);

  return (
    <>
      <Header usuarioNome={usuarioNome} />

      <main className="cursos-page">
        <h1 className="cursos-title">Cursos</h1>

        <div className="cursos-search">
          <div className="cursos-search-input">
            <LupaIcon size={20} />
            <input
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="O que você procura?"
            />
          </div>

          <div className="cursos-search-button">
            <Button
              style="primary"
              content="Pesquisar"
              onClickButton={() => { }}
            />
          </div>

          <button
            type="button"
            className="cursos-search-submit-mobile"
            aria-label="Pesquisar"
          >
            <SearchButtonIcon size={40} />
          </button>
        </div>

        <div className="cursos-toolbar">
          <span className="cursos-count">
            {cursosFiltrados.length}{" "}
            {cursosFiltrados.length === 1 ? "Curso" : "Cursos"}
          </span>

          <div className="cursos-sort-desktop">
            <span>Ordenar por:</span>
            {OPCOES_ORDENACAO.map((opcao) => (
              <button
                key={opcao.valor}
                onClick={() => setOrdenacao(opcao.valor)}
                className={
                  ordenacao === opcao.valor
                    ? "cursos-sort-pill cursos-sort-pill-active"
                    : "cursos-sort-pill"
                }
              >
                {opcao.label}
              </button>
            ))}
          </div>

          <label className="cursos-sort-mobile">
            <span>Ordenar por:</span>
            <div className="cursos-sort-mobile-select">
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

        <div className="cursos-grid">
          {cursosFiltrados.map((curso) => (
            <CourseCard key={curso.id} curso={curso} />
          ))}
        </div>

        {cursosFiltrados.length === 0 && (
          <p className="cursos-empty">
            Nenhum curso encontrado para essa busca.
          </p>
        )}
      </main>

      <footer className="cursos-footer">
        <p>2026 PrevUni - Todos os direitos reservados</p>
        <div className="cursos-footer-logo">
          <img src="/logo-prevuni.svg" alt="PrevUni" />
        </div>
      </footer>
    </>
  );
}

export default CursosClient;