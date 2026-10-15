"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useState } from "react";
import { MenuIcon, DotsIcon, SearchButtonIcon } from "@/components/icons/icons";
import "./header.style.css";

interface HeaderProps {
  usuarioNome: string;
  tipoUsuario?: "coordenador" | "professor" | "admin";
}

const LINKS = [
  { label: "Cursos", href: "/cursos" },
  { label: "Disciplinas", href: "/disciplinas" },
  { label: "Alunos", href: "/alunos" },
  {
    label: "Coordenadores",
    href: "/coordenadores",
    visivelPara: ["admin"],
  },
  /*
{
    label: "Professores",
    href: "/professores",
    visivelPara: ["admin", "coordenador"],
  },
*/ 
];

function Header({ usuarioNome, tipoUsuario }: HeaderProps) {
  const pathname = usePathname();
  const router = useRouter();

  const links = LINKS.filter(
    (link) => !link.visivelPara || link.visivelPara.includes(tipoUsuario ?? "")
  );

  const [menuAberto, setMenuAberto] = useState(false);
  const [opcoesAbertas, setOpcoesAbertas] = useState(false);

  const iniciais = usuarioNome
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((parte) => parte[0])
    .join("")
    .toUpperCase();

  const primeirosNomes = usuarioNome.split(" ").slice(0, 2).join(" ");

  async function sair() {
    await fetch("/api/logout", { method: "POST" });
    router.push("/login");
  }

  return (
    <header className="app-header">
      <div className="app-header-top">
        <Link href="/cursos" className="app-header-logo">
          <img src="/logo.svg" alt="PrevUni" />
        </Link>

        <nav className="app-header-nav">
          {links.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={
                pathname?.startsWith(link.href)
                  ? "app-header-link app-header-link-active"
                  : "app-header-link"
              }
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="app-header-search">
          <input placeholder="O que você procura?" />
          <button aria-label="Buscar">
            <SearchButtonIcon size={36} />
          </button>
        </div>

        <button
          className="app-header-avatar"
          onClick={() => setOpcoesAbertas((estado) => !estado)}
        >
          {iniciais || "U"}
        </button>

        <button
          className="app-header-menu-button"
          onClick={() => setMenuAberto((estado) => !estado)}
          aria-label="Abrir menu"
        >
          <MenuIcon />
        </button>

        {opcoesAbertas && (
          <div className="app-header-options">
            <Link href="/meus-dados" onClick={() => setOpcoesAbertas(false)}>
              Meus dados
            </Link>
            <button onClick={sair}>Sair</button>
          </div>
        )}
      </div>

      {menuAberto && (
        <nav className="app-header-mobile-nav">
          {links.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              onClick={() => setMenuAberto(false)}
              className={
                pathname?.startsWith(link.href)
                  ? "app-header-link app-header-link-active"
                  : "app-header-link"
              }
            >
              {link.label}
            </Link>
          ))}
          <button className="app-header-mobile-logout" onClick={sair}>
            Sair
          </button>
        </nav>
      )}

      <div className="app-header-greeting">
        <h1>Olá, {primeirosNomes}...</h1>

        <div className="app-header-greeting-actions">
          <button
            onClick={() => setOpcoesAbertas((estado) => !estado)}
            aria-label="Mais opções"
          >
            <DotsIcon />
          </button>

          {opcoesAbertas && (
            <div className="app-header-options app-header-options-mobile">
              <Link href="/meus-dados" onClick={() => setOpcoesAbertas(false)}>
                Meus dados
              </Link>
              <button onClick={sair}>Sair</button>
            </div>
          )}
        </div>
      </div>
    </header>
  );
}

export default Header;