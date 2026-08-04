interface IconProps {
  size?: number;
  className?: string;
  alt?: string;
}

export function LivroIcon({ size = 32, className, alt = "" }: IconProps) {
  return (
    <img
      src="/Livro.svg"
      width={size}
      height={size}
      className={className}
      alt={alt}
    />
  );
}

export function DisciplinaIcon({ size = 24, className, alt = "" }: IconProps) {
  return (
    <img
      src="/Diciplina.svg"
      width={size}
      height={size}
      className={className}
      alt={alt}
    />
  );
}

export function AlunosIcon({ size = 24, className, alt = "" }: IconProps) {
  return (
    <img
      src="/Alunos.svg"
      width={size}
      height={size}
      className={className}
      alt={alt}
    />
  );
}

export function LupaIcon({ size = 20, className, alt = "" }: IconProps) {
  return (
    <img
      src="/lupa.svg"
      width={size}
      height={size}
      className={className}
      alt={alt}
    />
  );
}

export function SearchButtonIcon({
  size = 40,
  className,
  alt = "Buscar",
}: IconProps) {
  return (
    <img
      src="/Botão.svg"
      width={size}
      height={size}
      className={className}
      alt={alt}
    />
  );
}

export function MenuIcon({ size = 24, className }: IconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      className={className}
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M4 6h16M4 12h16M4 18h16"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function DotsIcon({ size = 20, className }: IconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      className={className}
      xmlns="http://www.w3.org/2000/svg"
    >
      <circle cx="5" cy="12" r="1.6" fill="currentColor" />
      <circle cx="12" cy="12" r="1.6" fill="currentColor" />
      <circle cx="19" cy="12" r="1.6" fill="currentColor" />
    </svg>
  );
}

export function ChevronDownIcon({ size = 16, className }: IconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      className={className}
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="m6 9 6 6 6-6"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}