import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: '運営者情報',
  intro:
    'このサイトの運営者についての情報です。ドイツの法律（§ 5 DDG）では、ドイツから運営されるサイトは、これらの情報を見つけやすい1か所にまとめて示すことになっています。それがこのページです。',
  sections: {
    providerTitle: '運営者',
    contactTitle: 'お問い合わせ',
    contact: (mail, abuse) => (
      <>
        メール：{mail}。害を与えているlambdaを報告するときは、{abuse}までご連絡ください。
      </>
    ),
    editorialTitle: 'コンテンツの責任者',
    editorial:
      'ドイツの法律（MStV第18条2項）に基づき、このサイトのページについて責任を負うのは次の者です。ここでホストされているlambdaは、それぞれの所有者が書いたものであり、対象外です。',
    dsaTitle: 'デジタルサービス法（DSA）に基づく連絡窓口',
    dsa: (mail) => (
      <>
        当局、欧州委員会、そしてこのサービスを利用するすべての方は、{mail}で私たちに連絡できます。ドイツ語または英語でお書きください（DSA第11条・第12条）。
      </>
    ),
  },
};
