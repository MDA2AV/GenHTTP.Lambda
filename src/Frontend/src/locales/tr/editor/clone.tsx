import type { EditorMessages } from '../../en/editor';

export const clone: EditorMessages['clone'] = {
  button: 'Klonla',
  title: 'git ile klonla',
  intro:
    'Kendi araçlarınız ve kodlama ajanınızla üzerinde çalışın: depo, uygulamanın çalıştığı projenin kendisidir; her sürüm main’in bir commit’i, her taslak bir daldır.',
  keyWarning: 'Adres editör anahtarını içerir: ona sahip olan uygulamayı değiştirebilir. Paylaştıklarınızın dışında tutun.',
  draft: (branch) => <>Bu taslak {branch} dalıdır.</>,
  pushing: 'Push etme',
  toMain: (deploy) => <>main’e push edilen bir commit sonraki sürüm olur, henüz yayında değildir - {deploy} onu push ile birlikte yayına alır.</>,
  toBranch: 'Push edilen bir dal taslak olur; önizlemesi kendi adresinde yayındadır.',
  agents: (file) => <>Depodaki {file}, bir kodlama ajanına gerisini anlatır.</>,
  readOnly: 'Demo salt okunurdur: okumak için klonlayın, değiştirmek için ondan kendi lambdanızı başlatın.',
  copy: 'Kopyala',
  copied: 'Kopyalandı',
};
