"use client";
import styles from "./Footer.module.css";

export default function Footer() {
  const anoAtual = new Date().getFullYear();

  return (
    <footer className={styles.footer}>
      <p>{anoAtual} PrevUni - Todos os direitos reservados</p>
      <div className={styles.footerLogo}>
        <img src="/logovetor.svg" alt="PrevUni" />
      </div>
    </footer>
  );
}