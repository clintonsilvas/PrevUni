"use client";
import styles from "./Footer.module.css";

export default function Navbar() { 

  return (
    <footer className={styles.footer}>      
      <p>PrevUni - Todos os direitos reservados</p> 
      <img
        src="/logovetor.svg"
        alt="PrevUni"        
      />             
    </footer>
  );
}