"use client";

import { useActionState } from "react";
import { loginAction, ActionState } from "./actions";
import styles from "./login.module.css";

const initialState: ActionState = {
  success: false,
  message: "",
};

export function LoginForm() {  
  const [state, formAction, isPending] = useActionState(loginAction, initialState);

  return (
    <form action={formAction} className={styles.form}>      
      {state?.message && (
        <div className={styles.errorMessage}>
          {state.message}
        </div>
      )}

      <div className={styles.field}>
        <label htmlFor="email">Email</label>
        <input
          id="email"
          name="email"
          type="email"
          placeholder="Digite seu email"
          required
        />
      </div>

      <div className={styles.field}>
        <label htmlFor="senha">Senha</label>
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
        disabled={isPending}
      >
        {isPending ? "Entrando..." : "Entrar"}
      </button>
    </form>
  );
}