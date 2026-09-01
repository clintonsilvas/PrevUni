"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";

interface Props {
  styles: any;
}

export default function CadastroForm({
  styles
}: Props) {
  const router = useRouter();
  const [erro, setErro] = useState("");

  async function fazerLogin(
    e: React.FormEvent<HTMLFormElement>
  ) {
    e.preventDefault();
    setErro("");

    const formData =
      new FormData(e.currentTarget);
    const nome =
      formData.get("nome");
    const email =
      formData.get("email");
    const senha =
      formData.get("senha");
    const response = await fetch(
      "/api/criar-usuario",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({nome,email, senha,
        }),
      }
    );
    const data =      await response.json();
    if (!response.ok) {
      setErro(
        data.message || "Erro ao cadastrar"
      );
      return;
    }
    router.push("/login");
  }
  return (
    <form
      onSubmit={fazerLogin}
      className={styles.form}
    >
      <label>Nome</label>
      <input
        name="nome"
        type="text"
        placeholder="Insira aqui"
        required
      />
      <div className={styles.later}>
        <div className={styles.field}>
          <label>Email</label>
          <input name="email" type="email" placeholder="Insira aqui" required />
        </div>
        <div className={styles.field}>
          <label>Confirme seu email</label>
          <input name="emailConfirm" type="email" placeholder="Insira aqui" required />
        </div>
      </div>
      <div className={styles.later}>
        <div className={styles.field}>
        <label>Senha</label>
        <input
          name="senha"
          type="password"
          placeholder="Insira aqui"
          required
        />
        </div>
        <div className={styles.field}>

        <label>Confirme sua senha </label>
        <input
          name="senhaConfirme"
          type="password"
          placeholder="Insira aqui"
          required
        />
        </div>
      </div>     
      
      {
        erro &&
        <p>
          {erro}
        </p>
      }
      <hr className={styles.divider} />
      <div className={styles.buttons}>
        <button
          type="submit"
          className={styles.loginButton}
        >
          Criar Conta
        </button>

      </div>
    </form>
  );
}