import Link from "next/link";
import { redirect } from "next/navigation";
import { cookies } from "next/headers";

import { AdminLoginService } from "@/services/admin-login.service";

import styles from "./login.module.css";

export default function AdminLogin() {

  async function fazerLogin(formData: FormData) {
    "use server";

    const email = formData.get("email") as string;
    const senha = formData.get("senha") as string;

    if (!email || !senha) {
      throw new Error(
        "Email e senha são obrigatórios."
      );
    }

    const adminLoginService =
      new AdminLoginService();

    const resultado =
      await adminLoginService.login(
        email,
        senha
      );

    const cookieStore = await cookies();

    cookieStore.set(
      "prevuni_admin_token",
      resultado.token,
      {
        httpOnly: true,
        secure: process.env.NODE_ENV === "production",
        sameSite: "lax",
        path: "/",
        maxAge: 60 * 60 * 8,
      }
    );

    redirect("/admin/dashboard");
  }

  return (
    <main className={styles.container}>

      <section className={styles.left}>

        <div className={styles.content}>

          <img
            src="/logo.svg"
            alt="PrevUni"
            className={styles.logo}
          />

          <div className={styles.header}>

            <h1>
              Acesso administrativo
            </h1>

            <p>
              Entre no painel administrativo do PrevUni.
            </p>

          </div>

          <form
            action={fazerLogin}
            className={styles.form}
          >

            <div className={styles.field}>

              <label htmlFor="email">
                Email
              </label>

              <input
                id="email"
                name="email"
                type="email"
                placeholder="Digite seu email"
                required
              />

            </div>

            <div className={styles.field}>

              <label htmlFor="senha">
                Senha
              </label>

              <input
                id="senha"
                name="senha"
                type="password"
                placeholder="Digite sua senha"
                required
              />

            </div>

            <button
              type="submit"
              className={styles.loginButton}
            >
              Entrar
            </button>

          </form>

          <Link
            href="/"
            className={styles.back}
          >
            ← Voltar para o PrevUni
          </Link>

        </div>

      </section>

      <section className={styles.right}>

        <div className={styles.rightContent}>

          <span className={styles.badge}>
            ADMINISTRAÇÃO
          </span>

          <h2>
            Gerencie as instituições
            conectadas ao PrevUni.
          </h2>

          <p>
            Cadastre instituições, configure domínios,
            acompanhe integrações e gerencie a
            infraestrutura da plataforma.
          </p>

        </div>

      </section>

    </main>
  );
}