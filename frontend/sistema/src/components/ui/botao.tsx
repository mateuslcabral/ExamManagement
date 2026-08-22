import type { ButtonHTMLAttributes } from "react";

type Variante = "primaria" | "secundaria" | "perigo" | "fantasma";

const estilos: Record<Variante, string> = {
  primaria: "bg-primaria text-white hover:bg-primaria-clara focus-visible:ring-primaria",
  secundaria: "bg-white text-primaria border border-primaria hover:bg-fundo-alt focus-visible:ring-primaria",
  perigo: "bg-erro text-white hover:bg-erro/90 focus-visible:ring-erro",
  fantasma: "bg-transparent text-primaria hover:bg-primaria/10 focus-visible:ring-primaria",
};

export function Botao({
  variante = "primaria",
  carregando = false,
  tamanho = "md",
  className = "",
  children,
  disabled,
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variante?: Variante; carregando?: boolean; tamanho?: "sm" | "md" }) {
  const pad = tamanho === "sm" ? "px-3 py-1.5 text-xs" : "px-5 py-2.5 text-sm";
  return (
    <button
      disabled={disabled || carregando}
      className={`inline-flex items-center justify-center gap-2 rounded-pill font-semibold transition focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60 ${pad} ${estilos[variante]} ${className}`}
      {...props}
    >
      {carregando && (
        <span className="size-4 animate-spin rounded-full border-2 border-current border-t-transparent" aria-hidden />
      )}
      {children}
    </button>
  );
}
