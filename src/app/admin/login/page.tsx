// app/admin/login/page.tsx
import Link from "next/link";
import { LoginForm } from "./LoginForm";
import styles from "./login.module.css";

export default function AdminLoginPage() {
  return (
    <main className={styles.container}>
      <section className={styles.left}>
        <div className={styles.content}>
          <img src="/logo.svg" alt="PrevUni" className={styles.logo} />

          <div className={styles.header}>
            <h1>Acesso administrativo</h1>
            <p>Entre no painel administrativo do PrevUni.</p>
          </div>
          <LoginForm />
          <Link href="/" className={styles.back}>
            ← Voltar para o PrevUni
          </Link>
        </div>
      </section>

      <section className={styles.right}>
        <div className={styles.rightContent}>
          <span className={styles.badge}>ADMINISTRAÇÃO</span>
          <h2>Gerencie as instituições conectadas ao PrevUni.</h2>
          <p>
            Cadastre instituições, configure domínios, acompanhe integrações e
            gerencie a infraestrutura da plataforma.
          </p>
        </div>
      </section>
    </main>
  );
}