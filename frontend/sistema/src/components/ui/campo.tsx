import type { InputHTMLAttributes } from "react";

export function Campo({
  label,
  erro,
  dica,
  id,
  className = "",
  ...props
}: InputHTMLAttributes<HTMLInputElement> & { label: string; erro?: string; dica?: string }) {
  const inputId = id ?? props.name;
  return (
    <div className={className}>
      <label htmlFor={inputId} className="mb-1 block text-sm font-medium text-texto">
        {label}
      </label>
      <input
        id={inputId}
        aria-invalid={!!erro}
        aria-describedby={erro ? `${inputId}-erro` : dica ? `${inputId}-dica` : undefined}
        className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-texto outline-none transition placeholder:text-texto-suave/60 focus:ring-2 focus:ring-primaria/40 ${
          erro ? "border-erro" : "border-borda focus:border-primaria"
        }`}
        {...props}
      />
      {erro ? (
        <p id={`${inputId}-erro`} className="mt-1 text-xs text-erro">
          {erro}
        </p>
      ) : dica ? (
        <p id={`${inputId}-dica`} className="mt-1 text-xs text-texto-suave">
          {dica}
        </p>
      ) : null}
    </div>
  );
}
