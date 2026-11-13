import { abuse, common, notFound, shell } from './common';
import { buildBox } from './buildBox';
import { connect } from './connect';
import { guide } from './guide';
import { kinds, licenses } from './licenses';
import { card, showcase } from './showcase';

/**
 * The words every page of the platform shares, in English, which every other
 * language translates: the shell around the pages, what is said in many
 * places, connecting an agent, a card of the showcase, the guide and the
 * licenses a source is published under. Each bundle puts them together with
 * its own words into the catalog its pages read (see i18n).
 */
export const kit = {
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

export type KitMessages = typeof kit;
