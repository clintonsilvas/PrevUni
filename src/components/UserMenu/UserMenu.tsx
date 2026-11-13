"use client";

import { useEffect, useRef, useState } from "react";
import Link from "next/link";

import styles from "./UserMenu.module.css";

export default function UserMenu() {

  const [aberto, setAberto] = useState(false);

  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {

    function fechar(event: MouseEvent) {

      if (
        menuRef.current &&
        !menuRef.current.contains(event.target as Node)
      ) {
        setAberto(false);
      }

    }

    document.addEventListener("mousedown", fechar);

    return () => {
      document.removeEventListener("mousedown", fechar);
    };

  }, []);

  async function logout() {
    await fetch("/api/logout", {
      method: "POST",
    });
    window.location.href = "/login";

  }

  return (

    <div
      className={styles.container}
      ref={menuRef}
    >
      <button
        className={styles.avatarButton}
        onClick={() => setAberto(!aberto)}
      >
        <img
          src="https://i.pravatar.cc/150"
          alt="Usuário"
          className={styles.avatar}
        />

      </button>

      {aberto && (

        <div className={styles.menu}>

          <div className={styles.userInfo}>

            <strong>João Silva</strong>

            <span>Coordenador</span>

          </div>

          <hr />

          <Link href="/meus-dados">
            Meus Dados
          </Link>

          <button onClick={logout}>
            Sair
          </button>

        </div>

      )}

    </div>

  );

}