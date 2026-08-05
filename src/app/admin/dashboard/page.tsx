import { obterAdminAutenticado } from "@/lib/admin-auth";

import Instituicoes from "./Instituicoes";
import styles from "./dashboard.module.css";

import { logoutAdmin } from "@/app/admin/logout/actions";

export default async function AdminDashboard() {

  const admin = await obterAdminAutenticado();

  return (
    <main className={styles.container}>
      <header className={styles.header}>
        <div>
          <span className={styles.eyebrow}>
            ADMINISTRAÇÃO
          </span>
          <h1>
            Dashboard
          </h1>
          <p>
            Bem-vindo, {admin.nome}.
          </p>
        </div>

        <form action={logoutAdmin}>
          <button
            type="submit"
            className={styles.logoutButton}
          >
            Sair
          </button>
        </form>

      </header>

      <section className={styles.content}>

        <div className={styles.sectionHeader}>

          <div>
            <h2>
              Instituições
            </h2>

            <p>
              Gerencie as instituições conectadas ao PrevUni.
            </p>
          </div>

        </div>

        <Instituicoes />

      </section>

    </main>
  );
}