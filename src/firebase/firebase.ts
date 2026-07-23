import { initializeApp, getApps, getApp } from "firebase/app";
import { getFirestore } from "firebase/firestore";

const firebaseConfig = {
  apiKey: "AIzaSyD-PYcUeAQGtbBixOLWLsdzsJd3H0nvvAc",
  authDomain: "prev-uni.firebaseapp.com",
  projectId: "prev-uni",
  storageBucket: "prev-uni.firebasestorage.app",
  messagingSenderId: "431872106934",
  appId: "1:431872106934:web:e620a6244d96977b0a6117",
  measurementId: "G-RQ8GXL4T2K"
};

const app = getApps().length ? getApp() : initializeApp(firebaseConfig);

export const db = getFirestore(app, "dbprevuni");