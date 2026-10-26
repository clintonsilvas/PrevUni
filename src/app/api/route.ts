import { NextResponse } from 'next/server';
import { verificarAdminApi } from "@/lib/admin-auth-api";

export async function GET() {
  try {
    const admin = await verificarAdminApi();

    if (!admin) {
      return NextResponse.json(
        { success: false, message: "Não autenticado." },
        { status: 401 }
      );
    }       

    return NextResponse.json({ message: "BACKEND FUNCIONANDO" });
  } catch (error) {
    console.error("Erro no servidor", error);  
    return NextResponse.json(
      { success: false, message: "Erro no servidor" },
      { status: 500 }
    );
  }  
}