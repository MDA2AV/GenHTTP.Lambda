import type { Messages } from '../en';

export const imprint: Messages['imprint'] = {
  title: 'Künye',
  intro:
    'Bu siteyi kimin işlettiği. Alman hukuku, Almanya’dan işletilen her sitenin bu bilgileri kolay bulunan tek bir yerde vermesini ister (§ 5 DDG). O yer burası.',
  sections: {
    providerTitle: 'Hizmet sağlayıcı',
    contactTitle: 'İletişim',
    contact: (mail, abuse) => (
      <>
        E-posta: {mail}. Zarar veren bir lambdayı bildirmek için {abuse} adresine yazın.
      </>
    ),
    editorialTitle: 'İçerikten sorumlu kişi',
    editorial:
      'Alman MStV yasası § 18(2) uyarınca bu sitenin sayfalarından sorumlu kişi (burada barındırılan lambdalardan değil; onları sahipleri kendileri yazar):',
    dsaTitle: 'Dijital Hizmetler Yasası kapsamında iletişim noktası',
    dsa: (mail) => (
      <>
        Yetkili makamlar, Avrupa Komisyonu ve bu hizmeti kullanan herkes bize {mail} adresinden, Almanca veya İngilizce olarak ulaşabilir (DSA md. 11 ve 12).
      </>
    ),
  },
};
