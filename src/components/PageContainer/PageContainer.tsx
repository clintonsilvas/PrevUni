import styles from "./PageContainer.module.css";

export default function PageContainer({
    children,
}:{
    children:React.ReactNode;
}){

    return(

        <main className={styles.container}>

            {children}

        </main>

    );

}