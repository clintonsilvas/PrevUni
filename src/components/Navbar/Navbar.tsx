"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import UserMenu from "../UserMenu/UserMenu";

import styles from "./Navbar.module.css";

export default function Navbar() {

  const pathname = usePathname();

  const [menuAberto, setMenuAberto] = useState(false);

  const menu = [
    {
      href: "/dashboard",
      label: "Dashboard",
    },    
    {
      href: "/cursos",
      label: "Cursos",
    },
    {
      href: "/disciplinas",
      label: "Disciplinas",
    },
    {
      href: "/alunos",
      label: "Alunos",
    },
    {
      href: "/coordenadores",
      label: "Coordenadores",
    },
    {
      href: "/professores",
      label: "Professores",
    },
    {
      href: "/relatorios",
      label: "Relatórios",
    },
  ];

  return (

    <nav className={styles.navbar}>
      <div className={styles.left}>
        <img
          src="/logo.svg"
          alt="PrevUni"
          className={styles.logo}
        />

        <div className={styles.links}>

          {menu.map(item => (

            <Link
              key={item.href}
              href={item.href}
              className={
                pathname === item.href
                  ? styles.active
                  : ""
              }
            >
              {item.label}
            </Link>

          ))}

        </div>

      </div>


      <div className={styles.right}>
        <div className={styles.search}>
          <input
            placeholder="O que você procura?"
          />
          <button>
            <img src="/pesquisa.svg" />
          </button>
        </div>
        <UserMenu />
      </div>

      <button
        className={styles.menuButton}
        onClick={() => setMenuAberto(!menuAberto)}
      >
        ☰
      </button>


      <div
        className={`${styles.mobileMenu} ${
          menuAberto ? styles.open : ""
        }`}
      >

        {menu.map(item => (

          <Link
            key={item.href}
            href={item.href}
            onClick={() => setMenuAberto(false)}
          >
            {item.label}
          </Link>

        ))}

        <Link
          href="/meus-dados"
          onClick={() => setMenuAberto(false)}
        >
          Meus Dados
        </Link>
      </div>
    </nav>
  );
}