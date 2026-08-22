import type { SelectHTMLAttributes } from "react";

export function Selecao({
  label,
  erro,
  id,
  className = "",
  children,
  ...props
}: SelectHTMLAttributes<HTMLSelectElement> & { label: string; erro?: string }) {
  const selectId = id ?? props.name;
  return (
    <div className={className}>
      <label htmlFor={selectId} className="mb-1 block text-sm font-medium text-texto">
        {label}
      </label>
      <select
        id={selectId}
        aria-invalid={!!erro}
        aria-describedby={erro ? `${selectId}-erro` : undefined}
        className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-texto outline-none transition focus:ring-2 focus:ring-primaria/40 ${
          erro ? "border-erro" : "border-borda focus:border-primaria"
        }`}
        {...props}
      >
        {children}
      </select>
      {erro && (
        <p id={`${selectId}-erro`} className="mt-1 text-xs text-erro">
          {erro}
        </p>
      )}
    </div>
  );
}
