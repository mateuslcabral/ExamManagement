"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { MENU, type ItemMenu } from "./menu";
import { Icone } from "./icones";

export function Sidebar() {
  const pathname = usePathname();
  const grupos = agrupar(MENU);

  return (
    <nav aria-label="Menu principal" className="flex h-full flex-col gap-6 p-4">
      {grupos.map(([grupo, itens]) => (
        <div key={grupo ?? "raiz"}>
          {grupo && (
            <p className="mb-2 px-3 text-[11px] font-semibold uppercase tracking-wider text-white/50">{grupo}</p>
          )}
          <ul className="space-y-0.5">
            {itens.map((item) => {
              const ativo = item.href && (pathname === item.href || (item.href !== "/" && pathname.startsWith(item.href)));
              const base = "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition";
              if (!item.href) {
                return (
                  <li key={item.rotulo}>
                    <span
                      className={`${base} cursor-not-allowed text-white/35`}
                      title="Em breve — aguardando definições de escopo"
                      aria-disabled
                    >
                      <Icone nome={item.icone} />
                      {item.rotulo}
                      <span className="ml-auto rounded-full bg-white/10 px-1.5 py-0.5 text-[10px] font-semibold">
                        em breve
                      </span>
                    </span>
                  </li>
                );
              }
              return (
                <li key={item.rotulo}>
                  <Link
                    href={item.href}
                    aria-current={ativo ? "page" : undefined}
                    className={`${base} ${ativo ? "bg-white/15 text-white" : "text-white/80 hover:bg-white/10 hover:text-white"}`}
                  >
                    <Icone nome={item.icone} />
                    {item.rotulo}
                  </Link>
                </li>
              );
            })}
          </ul>
        </div>
      ))}
    </nav>
  );
}

function agrupar(itens: ItemMenu[]): [string | undefined, ItemMenu[]][] {
  const mapa = new Map<string | undefined, ItemMenu[]>();
  for (const i of itens) mapa.set(i.grupo, [...(mapa.get(i.grupo) ?? []), i]);
  return [...mapa.entries()];
}
