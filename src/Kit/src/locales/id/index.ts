import type { KitMessages } from '../en';
import { abuse, common, notFound, shell } from './common';
import { buildBox } from './buildBox';
import { connect } from './connect';
import { guide } from './guide';
import { kinds, licenses } from './licenses';
import { card, showcase } from './showcase';

export const kit: KitMessages = {
  shell,
  common,
  connect,
  buildBox,
  notFound,
  abuse,
  showcase,
  card,
  guide,
  licenses,
  kinds,
};
