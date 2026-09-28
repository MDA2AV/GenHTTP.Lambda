import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Galería',
  title: 'Creadas aquí, funcionando ahora',
  intro:
    'Lambdas que sus dueños quisieron mostrar. Todas están en línea, así que cada tarjeta abre la app de verdad. Primero van las que más se han usado últimamente.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'No se pudo cargar la galería.',
  loadingMore: 'Cargando más…',
  showMore: 'Ver más',
  nothingTitle: 'Todavía no hay nada que ver',
  nothing: (tab) => (
    <>
      ¿Hiciste algo que funciona? Abre su centro de control, elige {tab('Galería')} y añade un título, unas palabras y
      una imagen. Aparecerá aquí mientras esté en línea.
    </>
  ),
  buildOne: 'Crea tu app',
  yoursTitle: '¿Quieres que aparezca la tuya?',
  yours: (tab) => (
    <>
      Abre el centro de control de tu lambda y elige {tab('Galería')}, o pídele al agente que la creó que la muestre
      aquí. Solo puede hacerlo quien tenga la clave de edición, y puedes retirarla cuando quieras.
    </>
  ),
  buildSomething: 'Crea tu app',
};

export const card: Messages['card'] = {
  noPicture: 'Todavía sin imagen',
  title: 'Título',
  description: 'Lo que un visitante puede hacer con ella.',
  opens: (title, address) => `${title}: abre ${address} en una pestaña nueva`,
};
