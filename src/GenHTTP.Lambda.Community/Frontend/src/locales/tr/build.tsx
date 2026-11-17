import type { Messages } from '../en';

export const build: Messages['build'] = {
  title: 'Anlatarak bir uygulama oluşturun',
  intro:
    'Bu kurulumun oluşturma ajanına neye ihtiyacınız olduğunu söyleyin. Ajan uygulamayı yazar, burada yayına alır ve listenizde tutar - daha sonra aynı şekilde hakkında soru sormanız ve onu değiştirmeniz için.',
  other: (ship, create) => (
    <>
      Ya da {ship('kendi ajanınızı bağlayın')}, ya da {create('kodu kendiniz yazın')}.
    </>
  ),
};
