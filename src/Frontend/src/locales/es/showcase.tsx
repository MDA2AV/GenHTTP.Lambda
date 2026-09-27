import type { Messages } from '../en';

export const showcase: Messages['showcase'] = {
  eyebrow: 'Galería',
  title: 'Creadas aquí, en funcionamiento ahora',
  intro:
    'Lambdas que sus propietarios han decidido mostrar. Todos están en línea, por lo que cada tarjeta abre la aplicación real. Los más utilizados recientemente aparecen primero.',
  counted: (total) => (total === 1 ? '1 lambda' : `${total} lambdas`),
  failed: 'No se pudo cargar la galería.',
  loadingMore: 'Cargando más…',
  showMore: 'Mostrar más',
  nothingTitle: 'Todavía no hay nada que mostrar',
  nothing: (tab) => (
    <>
      ¿Ha creado algo que funciona? Abra su centro de control, elija {tab('Galería')} y añada un título, una breve
      descripción y una imagen. Aparecerá aquí mientras esté en línea.
    </>
  ),
  buildOne: 'Crear una aplicación',
  yoursTitle: '¿Desea mostrar la suya?',
  yours: (tab) => (
    <>
      Abra el centro de control de su lambda y elija {tab('Galería')}, o pida al agente que lo creó que lo añada. Solo
      quien tiene la clave de edición puede hacerlo, y puede retirarse en cualquier momento.
    </>
  ),
  buildSomething: 'Crear una aplicación',
};

export const card: Messages['card'] = {
  noPicture: 'Sin imagen todavía',
  title: 'Título',
  description: 'Lo que un visitante puede hacer con ella.',
  opens: (title, address) => `${title}, abre ${address} en una pestaña nueva`,
};
