import {
  SignJWT,
  jwtVerify,
  type JWTPayload,
} from "jose";

const JWT_SECRET = process.env.JWT_SECRET;

if (!JWT_SECRET) {
  throw new Error("JWT_SECRET não configurado.");
}

const secret = new TextEncoder().encode(JWT_SECRET);

export interface AuthPayload extends JWTPayload {
  id: string;
  nome: string;
  email: string;
  tipo: "admin" | "coordenador" | "professor";
  instituicaoId?: string;
}

export async function criarToken(
  payload: AuthPayload
) {
  return await new SignJWT(payload)
    .setProtectedHeader({
      alg: "HS256",
    })
    .setIssuedAt()
    .setExpirationTime("8h")
    .sign(secret);
}

export async function verificarToken(
  token: string
): Promise<AuthPayload | null> {

  try {

    const { payload } = await jwtVerify(
      token,
      secret
    );

    return payload as AuthPayload;

  } catch {

    return null;

  }
}