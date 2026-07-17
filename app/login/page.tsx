"use client";

import styles from "./login.module.css";

export default function Login() {
  return (
    <div className={styles.container}>  

      <div className={styles.left}>        
        <div className={styles.textform}>
          <img src="/logo.svg" alt="PrevUni" className={styles.logo} />   
          <div className={styles.textlogin}>
            <h2>Faça seu Login</h2>
            <p>Insira seus dados abaixo!</p>             
          </div>   
        </div>
        <form className={styles.form}>
          <label >Email</label>
          <input type="email" placeholder="Insira aqui" />
          <label>Senha</label>          
          <input type="password" placeholder="Insira aqui" />
          <hr className={styles.divider} />
          <div className={styles.buttons}>
            <button type="submit" className={styles.loginButton}>
              Entrar
            </button>

            <button type="button" className={styles.googleButton}>
              <img src="/google.svg" alt="" />
              Entrar com o Google
            </button>
          </div>                    
        </form>
        <p className={styles.register}>
            Não possui conta? <a href="/cadastro">Crie uma conta</a>
          </p>
      </div>
      <div className={styles.right}>        
      </div>
    </div>
  );
}