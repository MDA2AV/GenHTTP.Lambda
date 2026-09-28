import type { ReactNode } from 'react';

export const imprint = {
  title: 'Legal notice',
  intro:
    'Who runs this site. German law asks every site run from Germany to give these details in one place that is easy to find (§ 5 DDG), and this is it.',
  sections: {
    providerTitle: 'Provider',
    contactTitle: 'Contact',
    contact: (mail: ReactNode, abuse: ReactNode) => (
      <>
        Email: {mail}. To report a lambda that is doing harm, write to {abuse}.
      </>
    ),
    editorialTitle: 'Responsible for the content',
    editorial:
      'Responsible for the pages of this site under § 18(2) MStV - not for the lambdas hosted on it, which their owners write themselves:',
    dsaTitle: 'Contact point under the Digital Services Act',
    dsa: (mail: ReactNode) => (
      <>
        Authorities, the European Commission and anybody who uses this service can reach us at {mail}, in German or
        English (Art. 11 and 12 DSA).
      </>
    ),
  },
};
