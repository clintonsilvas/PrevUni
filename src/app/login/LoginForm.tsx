"use client";


import { useRouter } from "next/navigation";
import { useState } from "react";


interface Props {

  instituicaoId: string;

  styles: any;

}


export default function LoginForm({
  instituicaoId,
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


    const email =
      formData.get("email");


    const senha =
      formData.get("senha");



    const response = await fetch(
      "/api/login",
      {

        method: "POST",

        headers: {
          "Content-Type": "application/json",
        },

        body: JSON.stringify({

          instituicaoId,

          email,

          senha,

        }),

      }
    );



    const data =
      await response.json();



    if (!response.ok) {

      setErro(
        data.message || "Erro no login."
      );

      return;

    }



    router.push("/dashboard");

  }



  return (


    <form

      onSubmit={fazerLogin}

      className={styles.form}

    >


      <label>Email</label>


      <input

        name="email"

        type="email"

        placeholder="Insira aqui"

        required

      />



      <label>Senha</label>


      <input

        name="senha"

        type="password"

        placeholder="Insira aqui"

        required

      />



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

          Entrar

        </button>



        <button

          type="button"

          className={styles.googleButton}

        >

          <img
            src="/google.svg"
            alt=""
          />

          Entrar com Google

        </button>



      </div>



    </form>


  );

}