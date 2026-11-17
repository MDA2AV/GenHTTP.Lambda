import type { KitMessages } from '../en';
import { abuse, common, notFound, shell } from './common';
import { buildBox } from './buildBox';
import { connect } from './connect';
import { kinds, licenses } from './licenses';
import { card, showcase } from './showcase';
import { workspace } from './workspace';

export const kit: KitMessages = {
  shell,
  common,
  connect,
  buildBox,
  workspace,
  notFound,
  abuse,
  showcase,
  card,
  licenses,
  kinds,
};
