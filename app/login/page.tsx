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
          <button>Entrar</button>          
        </form>
      </div>
      <div className={styles.right}>        
      </div>
    </div>
  );
}