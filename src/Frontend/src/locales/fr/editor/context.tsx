import type { EditorMessages } from '../../en/editor';

export const context: EditorMessages['context'] = {
  docs: {
    title: 'Documentation',
    titleSimple: 'À propos de votre application',
    hint: 'Ce qu’est cette application, à qui elle s’adresse et pourquoi, et pourquoi elle est construite ainsi. Les agents la rédigent à chaque modification, et elle est gardée avec chaque version : une version plus ancienne revient avec la documentation qui valait pour elle.',
    hintSimple: 'À quoi sert votre application et pourquoi, tel que l’agent l’a compris à partir de vos demandes. Il tient cette description à jour à chaque modification.',
    inDraft: 'La documentation de ce brouillon. Elle devient celle de votre application quand le brouillon est mis en ligne.',
    pages: { product: 'Produit', decisions: 'Décisions' },
    emptyTitle: 'Rien n’est encore rédigé',
    emptyText: (code) => (
      <>
        Les agents rédigent la documentation avec leurs modifications : ce qu’est l’application, à qui elle s’adresse
        et pourquoi dans {code('.lambda/docs/product.md')}, et pourquoi elle est construite ainsi dans{' '}
        {code('decisions.md')}. Elle fait partie de la version, à côté du code.
      </>
    ),
    emptySimpleTitle: 'Rien n’est encore écrit sur votre application',
    emptySimple: 'L’agent peut décrire à quoi sert votre application et pourquoi, à partir de ce que vous avez demandé, puis tenir cette description à jour.',
    ask: 'Demander à l’agent de la rédiger',
    describe: 'Demander à l’agent de la décrire',
    writePrompt: 'Rédige la documentation de cette application : ce qu’elle est, à qui elle s’adresse et pourquoi, et les décisions techniques qui la sous-tendent.',
    describePrompt: 'Décris à quoi sert cette application et pourquoi, pour que je puisse le lire sous « À propos ».',
    decisionsPrompt: 'Consigne les décisions techniques derrière cette application, et pourquoi elles ont été prises.',
    missingProduct: 'Pas encore de page produit',
    missingProductText: 'Ce qu’est l’application, à qui elle s’adresse, ce qu’on en fait et pourquoi, avec les mots de la personne qui l’a demandée.',
    missingDecisions: 'Aucune décision consignée pour l’instant',
    missingDecisionsText: 'Comment l’application est construite et pourquoi : comment elle garde ses données, de quoi elle dépend, ce qui a été laissé de côté. Ce que doit savoir la prochaine personne qui la modifiera.',
    correctText: 'L’agent rédige ce texte à partir de ce que vous avez demandé, et le tient à jour à chaque modification. Quelque chose est faux ou manque ? Dites-le-lui.',
    correct: 'Le dire à l’agent',
    correctPrompt: 'Corrige la description de l’application : ',
    placeholder: 'Explique pourquoi les entrées sont gardées un an',
  },
  tests: {
    title: 'Tests',
    hint: 'Comment cette application est testée automatiquement, et les scripts et les données qu’utilisent les tests. Les agents les tiennent à jour et les lancent avant de considérer une modification comme terminée. Ils sont gardés avec chaque version.',
    inDraft: 'Les tests de ce brouillon. Ils deviennent ceux de votre application quand le brouillon est mis en ligne : lancez-les d’abord sur son aperçu.',
    pages: { testing: 'Comment elle est testée' },
    emptyTitle: 'Pas encore de tests',
    emptyText: (code) => (
      <>
        La façon dont l’application est testée (ce qui doit continuer à marcher, comment le vérifier et comment lancer
        les scripts prévus pour cela) est décrite par les agents dans {code('.lambda/tests/README.md')}, avec les
        scripts et les données de test à côté.
      </>
    ),
    ask: 'Demander à l’agent d’écrire des tests',
    writePrompt: 'Écris les tests de cette application : ce qui doit continuer à marcher et comment le vérifier automatiquement, avec un script à lancer sur son aperçu.',
    missing: 'Rien n’indique encore comment elle est testée',
    missingText: 'Ce qui doit continuer à marcher, comment chaque point est vérifié, et comment lancer les scripts qui l’accompagnent.',
    placeholder: 'Vérifie qu’une liste pleine refuse les nouvelles entrées',
  },
  files: 'Fichiers',
  noFiles: 'Aucun fichier à côté des pages.',
  none: 'à écrire',
  missingPill: 'Pas encore rédigée',
  changedIn: (version) => `Modifiée dans la version ${version}`,
  changedInDraft: 'Modifiée dans ce brouillon',
  showChanges: 'Voir ce qui a changé',
  hideChanges: 'Masquer ce qui a changé',
  noChanges: 'Rien n’a changé.',
  edit: 'Modifier',
  olderVersion: 'Une version ne change jamais : une page se modifie sur la version la plus récente, ou dans un brouillon.',
  writeIt: 'L’écrire vous-même',
  askPage: 'Demander à l’agent de la rédiger',
  editInCode: 'Ouvrir dans le code',
  cancel: 'Annuler',
  save: 'Enregistrer',
  write: 'Écrire',
  preview: 'Aperçu',
  writeOrPreview: 'Écriture ou aperçu',
  discard: 'Vos modifications de cette page seront perdues. Les abandonner ?',
  reading: 'Lecture…',
  readFailed: 'Impossible de lire ce contenu.',
  saveFailed: 'Impossible d’enregistrer.',
  savedDraft: 'Enregistré dans le brouillon.',
  savedVersion: (version) => `Enregistré en version ${version}.`,
  savedOnline: (version) => `Enregistré en version ${version}, et en ligne.`,
  savedNotOnline: (version) => `Enregistré en version ${version}, mais pas mis en ligne.`,
  saveTitle: 'Enregistrer en nouvelle version',
  saveText: (newest) =>
    `Une version ne change jamais : cette page est donc enregistrée dans la suivante, par-dessus la version ${newest}, sans rien changer d’autre.`,
  clash: (version) => `La version ${version} a été enregistrée depuis que vous avez commencé, et elle a aussi modifié cette page. Enregistrer remplace ce changement.`,
  alsoOnline: 'La mettre aussi en ligne',
  alsoOnlineNote: 'Seule la documentation change, les visiteurs ne voient donc rien de nouveau, mais ce qui est en ligne reste la version la plus récente.',
  skeleton: {
    product: '# Nom de l’application\n\nCe qu’elle est, en une ou deux phrases.\n\n## À qui elle s’adresse\n\n## Ce qu’on en fait\n\n## Fonctionnalités, et pourquoi elles existent\n\n## Ce qu’elle ne fait pas\n',
    decisions: '# Décisions\n\n## Une décision\n\nCe qui a été décidé, pourquoi, et ce qu’une modification doit garder à l’esprit.\n',
    testing: '# Comment elle est testée\n\nComment lancer les tests, et sur quelle adresse.\n\n## Ce qui doit continuer à marcher\n\n| Comportement | Requête | Attendu |\n|---|---|---|\n| | | |\n',
  },
};
