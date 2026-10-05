import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Sklonuj',
  title: 'Sklonuj przez git',
  intro:
    'Pracuj nad nią własnymi narzędziami i własnym agentem: repozytorium to projekt, w którym aplikacja działa, każda wersja jest commitem na main, a każdy szkic gałęzią.',
  keyWarning: 'Adres zawiera link do edytora: kto go ma, może zmienić aplikację. Nie udostępniaj go dalej.',
  draft: (branch) => <>Ten szkic to gałąź {branch}.</>,
  pushing: 'Wypychanie',
  toMain: (deploy) => <>Commit wypchnięty na main to kolejna wersja, jeszcze nie online - {deploy} udostępnia ją razem z wypchnięciem.</>,
  toBranch: 'Wypchnięta gałąź to szkic, a jego podgląd działa pod osobnym adresem.',
  agents: (file) => <>{file} w repozytorium podpowiada agentowi resztę.</>,
  readOnly: 'Demo jest tylko do odczytu: sklonuj je, żeby je przeczytać, i zacznij z niego własną lambdę, żeby coś zmienić.',
  copy: 'Kopiuj',
  copied: 'Skopiowano',
};
