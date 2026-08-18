"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { AdminLoginService } from "@/services/admin-login.service";

export type ActionState = {
  success: boolean;
  message?: string;
};

export async function loginAction(
  prevState: ActionState,
  formData: FormData
): Promise<ActionState> {
  const email = formData.get("email") as string;
  const senha = formData.get("senha") as string;

  if (!email || !senha) {
    return { success: false, message: "Email e senha são obrigatórios." };
  }

  try {
    const adminLoginService = new AdminLoginService();
    const resultado = await adminLoginService.login(email, senha);

    const cookieStore = await cookies();
    cookieStore.set("prevuni_admin_token", resultado.token, {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      sameSite: "lax",
      path: "/",
      maxAge: 60 * 60 * 8,
    });
  } catch (err: any) {    
    return {
      success: false,
      message: err.message || "Ocorreu um erro ao realizar o login.",
    };
  }  
  redirect("/admin/dashboard");
}