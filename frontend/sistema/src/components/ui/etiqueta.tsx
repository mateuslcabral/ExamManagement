export type CorEtiqueta = "teal" | "cinza" | "verde" | "amarelo" | "vermelho";

const cores: Record<CorEtiqueta, string> = {
  teal: "bg-teal/10 text-teal",
  cinza: "bg-fundo-alt text-texto-suave",
  verde: "bg-sucesso/10 text-sucesso",
  amarelo: "bg-alerta/10 text-alerta",
  vermelho: "bg-erro/10 text-erro",
};

export function Etiqueta({ cor, className = "", children }: { cor: CorEtiqueta; className?: string; children: React.ReactNode }) {
  return (
    <span className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-semibold ${cores[cor]} ${className}`}>{children}</span>
  );
}
